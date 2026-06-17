using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    internal class HomeData
    {
        internal static async Task<JsonObject> GetHomeSection(JsonObject? headers, string? continuation = null)
        {

            JsonObject? res = new JsonObject();

            if (continuation == null)
            {
                res = await GetFirstSection(headers);
            }
            else
            {
                res = await GetContinuationSection(headers, continuation);
            }

            if (res != null)
            {
                JsonArray sections = [];

                if (res?["sections"] != null)
                {

                    foreach (var item in (JsonArray)res?["sections"]!)
                    {
                        sections.Add(await ParseSection((JsonObject?)item));
                    }
                }

                var parsedResult = new JsonObject();
                parsedResult["sections"] = sections;
                parsedResult["continuationToken"] = res?["continuationToken"]?.GetValue<string>() ?? null;


                return parsedResult;
            }

            return new JsonObject();

        }

        internal static async Task<JsonObject> GetHomeSections(JsonObject? headers)
        {

            JsonArray sections = [];
            JsonObject parsedResult = new JsonObject();

            var firstCallContent = await GetHomeSection(headers);

            foreach (JsonObject? sec in (JsonArray?)firstCallContent?["sections"]!)
            {
                sections.Add(sec?.DeepClone());
            }


            string? continuation = firstCallContent?["continuationToken"]?.GetValue<string>() ?? null;

            while (continuation != null)
            {

                var newContet = await GetHomeSection(headers, continuation);

                foreach (JsonObject? sec in (JsonArray?)newContet?["sections"]!)
                {
                    sections.Add(sec?.DeepClone());
                }

                continuation = newContet["continuationToken"]?.GetValue<string>() ?? null;

            }

            parsedResult["sections"] = sections;
            return parsedResult;

        }

        private static async Task<JsonObject?> GetFirstSection(JsonObject? headers)
        {
            JsonObject? C;

            if (headers != null)
            {
                C = headers;
            }
            else
            {
                C = new JsonObject();
            }

            var payload = new JsonObject();
            payload["browseId"] = "FEmusic_home";

            JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);

            var baseNode = result?["contents"]?["singleColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"];

            JsonArray? sections = (JsonArray?)baseNode?["contents"]?.DeepClone();
            string? continuationToken = baseNode?["continuations"]?[0]?["nextContinuationData"]?["continuation"]?.GetValue<string>() ?? null;


            var formattedResult = new JsonObject();
            formattedResult["sections"] = sections;
            formattedResult["continuationToken"] = continuationToken;

            return formattedResult;
        }

        private static async Task<JsonObject?> GetContinuationSection(JsonObject? headers, string CToken)
        {
            JsonObject? C;

            if (headers != null)
            {
                C = headers;
            }
            else
            {
                C = new JsonObject();
            }

            var payload = new JsonObject();
            string continuationUrl = $"browse?ctoken={CToken}&continuation={CToken}&type=next";

            JsonObject result = await Requester.PostRequest(endpointUrl: continuationUrl, cookies: C, payload: payload);


            var baseNode = result?["continuationContents"]?["sectionListContinuation"];

            JsonArray? sections = (JsonArray?)baseNode?["contents"]?.DeepClone();
            string? continuationToken = baseNode?["continuations"]?[0]?["nextContinuationData"]?["continuation"]?.GetValue<string>() ?? null;


            var formattedResult = new JsonObject();
            formattedResult["sections"] = sections;
            formattedResult["continuationToken"] = continuationToken;

            return formattedResult;
        }


        //parsing

        private static async Task<JsonObject> ParseSection(JsonObject? section)
        {
            if (section != null)
            {

                var baseNode = section["musicCarouselShelfRenderer"];

                JsonObject parsedSection = await ParseSectionHeader((JsonObject?)baseNode?["header"]?["musicCarouselShelfBasicHeaderRenderer"]);


                JsonArray? items = (JsonArray?)baseNode?["contents"];




                if (items != null)
                {
                    JsonObject? firstElement = (JsonObject?)items?[0];

                    if (firstElement != null)
                    {
                        if (firstElement.ContainsKey("musicTwoRowItemRenderer"))
                        {
                            parsedSection["displayType"] = "carousel";
                        }
                        else if (firstElement.ContainsKey("musicResponsiveListItemRenderer"))
                        {
                            parsedSection["displayType"] = "itemList";
                        }
                    }

                }



                await ParseAndAddContent(parsedSection, items);


                return parsedSection;

            }

            return new JsonObject();
        }

        private static async Task<JsonObject> ParseSectionHeader(JsonObject? header)
        {

            if (header != null)
            {

                var parsedHeader = new JsonObject();


                parsedHeader["title"] = header?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? null;
                parsedHeader["subtitle"] = header?["strapline"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? null;
                parsedHeader["thumbnail"] = header?["thumbnail"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"]?[0]?["url"]?.GetValue<string>() ?? null;

                return parsedHeader;

            }

            return new JsonObject();

        }

        private static async Task ParseAndAddContent(JsonObject parsedHeader, JsonArray? items)
        {

            if (items != null && items.Count > 0)
            {

                JsonArray parsedItems = [];

                foreach (JsonObject? item in items)
                {
                    if (item != null)
                    {

                        if (item.ContainsKey("musicTwoRowItemRenderer"))
                        {
                            parsedItems.Add(Parsing.ParsemusicTwoRowItemRenderer((JsonObject?)item?["musicTwoRowItemRenderer"]?.DeepClone()));

                        }
                        else if (item.ContainsKey("musicResponsiveListItemRenderer"))
                        {
                            parsedItems.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject?)item?["musicResponsiveListItemRenderer"]?.DeepClone()!));
                        }

                    }
                }

                parsedHeader["items"] = parsedItems;

            }

        }


    }
}
