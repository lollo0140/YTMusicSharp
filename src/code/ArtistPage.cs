using System.Reflection.PortableExecutable;
using System.Text.Json.Nodes;

namespace YoutubeMusic
{

    internal class ArtistPage
    {
        private enum SectionType
        {
            Description,
            ItemList,
            Carousel
        }


        internal static async Task<JsonObject> FetchArtistPage(string BrowseId, JsonObject? headers)
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

            payload["browseId"] = BrowseId;
            JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);



            //PARSING HEADER ---------------
            JsonObject parsedPageHeader = await ParseArtistHeader((JsonObject?)result?["header"]?["musicImmersiveHeaderRenderer"]);

            JsonObject parsedSections = new JsonObject();
            if (parsedPageHeader != null)
            {
                parsedSections["header"] = parsedPageHeader;
            }



            //PARSING TABS ----------------

            JsonArray? sectionTab = (JsonArray?)result?["contents"]?["singleColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"];



            if (sectionTab != null)
            {
                parsedSections["sections"] = ParseSections(sectionTab);
            }


            return parsedSections;

        }

        private static async Task<JsonObject> ParseArtistHeader(JsonObject? sectionHeader)
        {
            JsonObject parsedHeader = new JsonObject();

            if (sectionHeader != null)
            {

                parsedHeader["headerTitle"] = sectionHeader?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "no title";

                parsedHeader["subscribed"] = sectionHeader?["subscriptionButton"]?["subscribeButtonRenderer"]?["subscribed"]?.GetValue<bool>() ?? false;

                parsedHeader["subscribeCount"] = sectionHeader?["subscriptionButton"]?["subscribeButtonRenderer"]?["subscriberCountWithUnsubscribeText"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "0";


                JsonArray? description = (JsonArray?)sectionHeader?["description"]?["runs"];



                parsedHeader["headerDescription"] = "";
                parsedHeader["wikipediaLink"] = "";

                //  if has description
                if (description?.Count > 1)
                {
                    parsedHeader["headerDescription"] = description?[0]?["text"]?.GetValue<string>() + description?[1]?["text"]?.GetValue<string>() + $")";

                    parsedHeader["wikipediaLink"] = description?[1]?["text"]?.GetValue<string>() ?? "";
                } else if (description?.Count > 0)
                {
                    parsedHeader["headerDescription"] = description?[0]?["text"]?.GetValue<string>();
                }




                JsonArray? headerImgs = (JsonArray?)sectionHeader?["thumbnail"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"];
                if (headerImgs != null)
                {
                    parsedHeader["headerImg"] = headerImgs?[headerImgs.Count - 1]?["url"]?.GetValue<string>() ?? "no url";
                }

                parsedHeader["listenersCount"] = sectionHeader?["monthlyListenerCount"]?["accessibility"]?["accessibilityData"]?["label"]?.GetValue<string>() ?? "no data";


            }


            return parsedHeader;

        }

        private static SectionType DetectSectionType(JsonObject section)
        {

            if (section.ContainsKey("musicShelfRenderer"))
            {
                return SectionType.ItemList;
            }
            else if (section.ContainsKey("musicCarouselShelfRenderer"))
            {
                return SectionType.Carousel;
            }
            else
            {
                return SectionType.Description;
            }


        }

        private static JsonArray ParseSections(JsonArray sections)
        {
            JsonArray parsedSections = [];

            foreach (JsonObject? item in sections)
            {
                if (item != null)
                {
                    SectionType contentType = DetectSectionType(item);

                    if (contentType == SectionType.ItemList)
                    {
                        parsedSections.Add(ParseMusicShelfRenderer((JsonObject?)item["musicShelfRenderer"]));
                    }
                    else if (contentType == SectionType.Carousel)
                    {
                        parsedSections.Add(ParseMusicCarouselShelfRenderer((JsonObject?)item["musicCarouselShelfRenderer"]));
                    }
                    else
                    {
                        continue;
                    }

                }

            }

            return parsedSections;
        }




        private static JsonObject ParseMusicShelfRenderer(JsonObject? section)
        {

            JsonObject parsedSection = new JsonObject();

            if (section != null)
            {



                parsedSection["title"] = section?["title"]?["runs"]?[0]?["text"]?.GetValue<string>();


                JsonArray items = [];

                if (section?["contents"] != null)
                {

                    foreach (JsonObject? item in (JsonArray)section?["contents"]!)
                    {

                        items.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject)item?["musicResponsiveListItemRenderer"]!));

                    }

                }

                parsedSection["items"] = items;


            }

            return parsedSection;

        }

        private static JsonObject ParseMusicCarouselShelfRenderer(JsonObject? section)
        {

            JsonObject parsedSection = new JsonObject();

            if (section != null)
            {

                parsedSection["title"] = section?["header"]?["musicCarouselShelfBasicHeaderRenderer"]?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "no title";

                JsonArray items = [];

                if (section?["contents"] != null)
                {

                    foreach (JsonObject? item in (JsonArray)section?["contents"]!)
                    {

                        items.Add(Parsing.ParsemusicTwoRowItemRenderer((JsonObject)item?["musicTwoRowItemRenderer"]!));

                    }

                }

                parsedSection["items"] = items;

            }

            return parsedSection;

        }

    }

}
