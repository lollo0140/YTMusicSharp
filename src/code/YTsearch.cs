using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace YoutubeMusic
{
    internal class YTsearch
    {

        private static class RequestParams
        {
            //request["params"] = --parametro--
            public const string songParam = "EgWKAQIIAWoQEAUQCRADEAQQChAQEBUQEQ%3D%3D";
            public const string videoParam = "EgWKAQIQAWoQEAUQCRADEAQQChAQEBUQEQ%3D%3D";
            public const string albumParam = "EgWKAQIYAWoQEAUQCRADEAQQChAQEBUQEQ%3D%3D";
            public const string artistParam = "EgWKAQIgAWoQEAUQCRADEAQQChAQEBUQEQ%3D%3D";
            public const string profilesParam = "EgWKAQJYAWoQEAUQCRADEAQQChAQEBUQEQ%3D%3D";
            public const string comunityPlaylistParam = "EgeKAQQoAEABahAQBRAJEAMQBBAKEBAQFRAR";
            public const string podcastParam = "EgWKAQJIAWoQEAUQCRADEAQQChAQEBUQEQ%3D%3D";

            public static string GetRightParameter(ContentType contentType)
            {

                switch (contentType)
                {
                    case ContentType.Track: return songParam;
                    case ContentType.Video: return videoParam;
                    case ContentType.Album: return albumParam;
                    case ContentType.Artist: return artistParam;
                    case ContentType.Channel: return profilesParam;
                    case ContentType.Playlist: return comunityPlaylistParam;
                    case ContentType.Podcast: return podcastParam;
                    case ContentType.All: return "allTypes";
                    default: return "allTypes";
                }

            }

        }

        internal static async Task<JsonArray> GetSearchSuggestions(string input, JsonObject? headers)
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

            payload["input"] = input;
            JsonObject result = await Requester.PostRequest(endpointUrl: "music/get_search_suggestions", cookies: C, payload: payload);


            JsonArray textSuggestionContent = result?["contents"]?[0]?["searchSuggestionsSectionRenderer"]?["contents"]?.AsArray() ?? [];

            JsonArray textSuggestions = [];

            foreach (var sugg in textSuggestionContent)
            {

                var runs = sugg?["searchSuggestionRenderer"]?["suggestion"]?["runs"] ?? sugg?["historySuggestionRenderer"]?["suggestion"]?["runs"];

                JsonObject suggestion = new JsonObject();

                suggestion["query"] = runs?[0]?["text"]?.GetValue<string>();
                suggestion["completition"] = runs?[1]?["text"]?.GetValue<string>();

                textSuggestions.Add(suggestion);

            }

            return textSuggestions;


        }

        internal static async Task<JsonObject> Search(string input, JsonObject? headers)
        {

            //requesting data

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

            //parsing setup

            payload["query"] = input;
            JsonObject result = await Requester.PostRequest(endpointUrl: "search", cookies: C, payload: payload);

            JsonArray sectionListRenderer = (JsonArray)result?["contents"]?["tabbedSearchResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]! ?? null!;

            JsonObject cardRender = new JsonObject();
            JsonArray musicShefls = [];
            JsonArray itemSections = [];


            //return inizialization
            JsonObject searchResult = new JsonObject();
            JsonArray parsedSections = new JsonArray();



            //sections sorting
            foreach (JsonNode? node in sectionListRenderer)
            {
                if (node is JsonObject obj)
                {
                    if (obj.ContainsKey("musicCardShelfRenderer"))
                    {
                        cardRender = obj.DeepClone().AsObject();
                    }
                    else if (obj.ContainsKey("musicShelfRenderer"))
                    {
                        musicShefls.Add(obj.DeepClone());
                    }
                    else if (obj.ContainsKey("itemSectionRenderer"))
                    {
                        itemSections.Add(obj.DeepClone());
                    }
                }
            }




            //parsing card render
            if (cardRender != null)
            {

                JsonObject? cardRenderParseResult = CardRenderParser.ParseCardRender(cardRender);

                if (result != null)
                {
                    searchResult["bestResult"] = cardRenderParseResult;
                }
                else
                {
                    searchResult["bestResult"] = new JsonObject();
                }
            }
            else
            {
                searchResult["bestResult"] = new JsonObject();
            }


            //parsing musicshelfs
            foreach (var musicShelf in musicShefls)
            {

                //--------------------------------------------------
                JsonObject section = new JsonObject();
                JsonArray parsedContent = new JsonArray();
                //--------------------------------------------------

                string sectionTitle = musicShelf?["musicShelfRenderer"]?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "not specified";



                JsonArray content = (JsonArray)musicShelf?["musicShelfRenderer"]?["contents"]! ?? null!;





                //parsing section elements
                foreach (var musicResponsiveListItemRenderer in content)
                {
                    JsonObject? mrlir = (JsonObject?)musicResponsiveListItemRenderer?["musicResponsiveListItemRenderer"];
                    if (mrlir != null)
                    {
                        parsedContent.Add(Parsing.ParseMusicResponsiveListItemRenderer(mrlir));
                    }
                }

                section["sectionTitle"] = sectionTitle;
                section["content"] = parsedContent;

                parsedSections.Add(section);


            }


            //parsing single elements
            JsonArray parsedSingleElements = [];
            JsonObject singleItemsSection = new JsonObject();


            foreach (JsonObject? item in itemSections)
            {


                if (item?["itemSectionRenderer"]?["contents"]?[0] != null)
                {

                    JsonObject? ListElement = (JsonObject?)item?["itemSectionRenderer"]?["contents"]?[0];

                    if (ListElement != null && ListElement.ContainsKey("musicResponsiveListItemRenderer"))
                    {
                        parsedSingleElements.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject)ListElement["musicResponsiveListItemRenderer"]!));
                    }
                }
            }

            singleItemsSection["sectionTitle"] = "More results";
            singleItemsSection["content"] = parsedSingleElements;

            parsedSections.Add(singleItemsSection);



            searchResult["sections"] = parsedSections;


            return searchResult;
        }

        internal static async Task<JsonObject> SpecificSearch(string input,
                                                              JsonObject? headers,
                                                              ContentType contentType = ContentType.All
                                                              )
        {

            string Type = RequestParams.GetRightParameter(contentType);

            if (Type == "allTypes")
            {
                return await Search(input, headers);
            }

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

            payload["query"] = input;
            payload["params"] = Type;

            JsonObject result = await Requester.PostRequest(endpointUrl: "search", cookies: C, payload: payload);




            JsonObject parsedResult = new JsonObject();

            JsonArray? itemList = (JsonArray?)result?["contents"]?["tabbedSearchResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[1]?["musicShelfRenderer"]?["contents"];


            if (itemList != null)
            {
                JsonArray parsedList = [];

                foreach (JsonObject? listItem in itemList)
                {
                    if (listItem != null)
                    {
                        parsedList.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject)listItem?["musicResponsiveListItemRenderer"]!));
                    }


                }



                parsedResult["results"] = parsedList;
            }
            else
            {
                parsedResult["results"] = new JsonArray();
            }



            // File.WriteAllText("./test2.json", JsonSerializer.Serialize(result));

            return parsedResult;

        }


        internal static async Task<JsonObject> SpecificIncognitoSearch(string input,
                                                              ContentType contentType = ContentType.All
                                                              )
        {

            string Type = RequestParams.GetRightParameter(contentType);

            JsonObject? C = [];


            var payload = new JsonObject();

            payload["query"] = input;
            payload["params"] = Type;

            JsonObject result = await Requester.PostRequest(endpointUrl: "search", cookies: C, payload: payload, noAuth: true);




            JsonObject parsedResult = new JsonObject();

            JsonArray? itemList = (JsonArray?)result?["contents"]?["tabbedSearchResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[1]?["musicShelfRenderer"]?["contents"];


            if (itemList != null)
            {
                JsonArray parsedList = [];

                foreach (JsonObject? listItem in itemList)
                {
                    if (listItem != null)
                    {
                        parsedList.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject)listItem?["musicResponsiveListItemRenderer"]!));
                    }


                }



                parsedResult["results"] = parsedList;
            }
            else
            {
                parsedResult["results"] = new JsonArray();
            }



            // File.WriteAllText("./test2.json", JsonSerializer.Serialize(result));

            return parsedResult;

        }

        private record Matching(JsonObject Data, int Score);

        internal static async Task<JsonObject> SearchMatchingTrack(
        string title,
        string firstArtistName,
        string albumName = "",
        bool isExplicit = true)
        {

            string inputString = $"{title} {firstArtistName} {albumName}";

            JsonObject traks = await SpecificIncognitoSearch(inputString, ContentType.Track);

            Matching? matching = new([], Int16.MinValue);

            foreach (JsonObject item in (traks?["results"]?.AsArray() ?? []).Cast<JsonObject>())
            {

                string itemTitle = (item?["title"]?.GetValue<string>() ?? "").ToLower();
                string itemAlbumName = (item?["album"]?["titleName"]?.GetValue<string>() ?? "").ToLower();
                string itemArtistName = (item?["artists"]?[0]?["artistName"]?.GetValue<string>() ?? "").ToLower();

                int score = 0;

                if (title.Equals(itemTitle, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 10;
                } else if (title.Contains(itemTitle, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 5;
                }

                if (!title.Equals(albumName))
                if (albumName.Equals(itemAlbumName, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 10;
                } else if (albumName.Contains(itemAlbumName, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 5;
                };

                if (firstArtistName.Equals(itemArtistName, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 10;
                } else if (firstArtistName.Contains(itemArtistName, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 5;
                }

                if ((item?["explicit"]?.GetValue<bool>() ?? false) == isExplicit) score += 5;

                if (score > matching.Score) matching = new(item ?? [], score);

            }

            return matching.Data;

        }

    }
}
