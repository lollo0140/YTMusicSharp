using System.Text.Json;
using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    internal class LibraryData
    {

        internal static async Task<JsonObject> GetUserLibrary(JsonObject? headers)
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

            payload["browseId"] = "FEmusic_library_landing";
            JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);



            JsonObject? baseNode = (JsonObject?)result?["contents"]?["singleColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0]?["gridRenderer"];

            JsonArray? parsedeElements = [];

            if (baseNode != null)
            {
                JsonArray? elements = (JsonArray?)baseNode?["items"];
                string? continuation = baseNode?["continuations"]?[0]?["nextContinuationData"]?["continuation"]?.GetValue<string>() ?? null;






                while (continuation != null)
                {
                    string url = $"browse?ctoken={continuation}&continuation={continuation}";

                    JsonObject continuationContent = await Requester.PostRequest(endpointUrl: url, cookies: C, payload: new JsonObject());

                    JsonArray? items = (JsonArray?)continuationContent?["continuationContents"]?["gridContinuation"]?["items"];

                    if (items != null && items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            elements?.Add(item?.DeepClone());
                        }
                    }

                    continuation = continuationContent?["continuations"]?[0]?["nextContinuationData"]?["continuation"]?.GetValue<string>() ?? null;

                }





                if (elements != null)
                {
                    foreach (JsonObject? item in elements)
                    {

                        if (item?["musicTwoRowItemRenderer"] != null)
                        {
                            parsedeElements.Add(Parsing.ParsemusicTwoRowItemRenderer((JsonObject?)item?["musicTwoRowItemRenderer"]).DeepClone());
                        }

                    }
                }

            }


            JsonObject library = new JsonObject();

            library["items"] = parsedeElements?.DeepClone();

            return library;
        }

        internal static async Task<JsonObject> GetUserContentByFilter(JsonObject? headers, ContentFilter? filter = null)
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

            if (filter == null)
            {
                return await GetUserLibrary(headers);
            }


            switch (filter)
            {

                case ContentFilter.Albums:
                    payload["browseId"] = "FEmusic_liked_albums";
                    break;
                case ContentFilter.Playlists:
                    payload["browseId"] = "FEmusic_liked_playlists";
                    break;
                case ContentFilter.Artists:
                    payload["browseId"] = "FEmusic_library_corpus_track_artists";
                    break;
                case ContentFilter.Subscribed:
                    payload["browseId"] = "FEmusic_library_corpus_artists";
                    payload["params"] = "ggMCCAU%3D";
                    break;
                case ContentFilter.Podcasts:
                    payload["browseId"] = "FEmusic_library_non_music_audio_list";
                    break;


            }

            JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);

            JsonObject? baseNode = (JsonObject?)result?["contents"]?["singleColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0]?["gridRenderer"];


            if (baseNode == null)
            {
                baseNode = (JsonObject?)result?["contents"]?["singleColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0]?["musicShelfRenderer"];
            }




            JsonArray? parsedeElements = [];

            if (baseNode != null)
            {
                JsonArray? elements;

                if (filter == ContentFilter.Artists || filter == ContentFilter.Subscribed)
                {
                    elements = (JsonArray?)baseNode?["contents"];
                }
                else
                {
                    elements = (JsonArray?)baseNode?["items"];
                }


                string? continuation = baseNode?["continuations"]?[0]?["nextContinuationData"]?["continuation"]?.GetValue<string>() ?? null;



                while (continuation != null)
                {
                    string url = $"browse?ctoken={continuation}&continuation={continuation}";

                    JsonObject continuationContent = await Requester.PostRequest(endpointUrl: url, cookies: C, payload: new JsonObject());



                    JsonArray? items;


                    if (filter == ContentFilter.Artists || filter == ContentFilter.Subscribed)
                    {
                        items = (JsonArray?)continuationContent?["continuationContents"]?["musicShelfContinuation"]?["contents"];
                    }
                    else
                    {
                        items = (JsonArray?)continuationContent?["continuationContents"]?["gridContinuation"]?["items"];
                    }







                    if (items != null && items.Count > 0)
                    {
                        foreach (var item in items)
                        {
                            elements?.Add(item?.DeepClone());
                        }
                    }

                    continuation = continuationContent?["continuations"]?[0]?["nextContinuationData"]?["continuation"]?.GetValue<string>() ?? null;

                }





                if (elements != null)
                {
                    foreach (JsonObject? item in elements)
                    {


                        if (filter == ContentFilter.Artists || filter == ContentFilter.Subscribed)
                        {
                            if (item?["musicResponsiveListItemRenderer"] != null)
                            {
                                parsedeElements.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject?)item?["musicResponsiveListItemRenderer"]!));
                            }
                        }
                        else
                        {
                            if (item?["musicTwoRowItemRenderer"] != null)
                            {
                                parsedeElements.Add(Parsing.ParsemusicTwoRowItemRenderer((JsonObject?)item?["musicTwoRowItemRenderer"]));
                            }
                        }



                    }
                }

            }


            JsonObject library = new JsonObject();

            library["items"] = parsedeElements?.DeepClone();

            return library;
        }


    }
}
