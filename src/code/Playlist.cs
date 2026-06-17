using System.Text.Json;
using System.Text.Json.Nodes;

namespace YoutubeMusic
{

    internal class Playlist
    {

        internal static async Task<JsonObject> FetchUserLikePlaylist(JsonObject? headers)
        {
            return await FetchPlaylistData("VLLM", headers);
        }

        internal static async Task<JsonObject> FetchUserEpisodePlaylist(JsonObject? headers)
        {
            return await FetchPlaylistData("VLSE", headers);
        }



        internal static async Task<JsonObject> FetchPlaylistData(string BrowseId, JsonObject? headers)
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


            JsonObject parsedPlaylist = new JsonObject();

            if (result != null)
            {
                parsedPlaylist["data"] = ParsePlaylistMetaData(result);
            }

            //PARSING elements ---------------------------------

            JsonArray musicResponsiveListItemRenderers = [];

            JsonArray? items = (JsonArray?)result?["contents"]?["twoColumnBrowseResultsRenderer"]?["secondaryContents"]?["sectionListRenderer"]?["contents"]?[0]?["musicPlaylistShelfRenderer"]?["contents"];


            if (items != null)
            {

                string? CT = FindContinuationToken(items);

                if (CT != null)
                {
                    for (int i = 0; i < items?.Count - 1; i++)
                    {
                        musicResponsiveListItemRenderers.Add(items?[i]?.DeepClone());
                    }
                }
                else
                {
                    for (int i = 0; i < items?.Count; i++)
                    {
                        musicResponsiveListItemRenderers.Add(items?[i]?.DeepClone());
                    }
                }


                while (CT != null)
                {
                    // System.Console.WriteLine(CT);

                    JsonArray? newTraks = await BrowseContinuation(CT, headers);

                    if (newTraks != null && newTraks.Count > 1)
                    {
                        CT = FindContinuationToken(newTraks);

                        if (CT != null)
                        {
                            for (int i = 0; i < newTraks?.Count - 1; i++)
                            {
                                musicResponsiveListItemRenderers.Add(newTraks?[i]?.DeepClone());
                            }
                        }
                        else
                        {
                            for (int i = 0; i < newTraks?.Count; i++)
                            {
                                musicResponsiveListItemRenderers.Add(newTraks?[i]?.DeepClone());
                            }
                        }
                    }
                    else
                    {
                        CT = null;
                    }
                }


            }

            JsonArray parsedTraks = [];

            foreach (JsonObject? item in musicResponsiveListItemRenderers)
            {

                var track = (JsonObject?)item?["musicResponsiveListItemRenderer"];


                if (track != null)
                {
                    parsedTraks.Add(Parsing.ParseMusicResponsiveListItemRenderer(track));
                }

            }

            parsedPlaylist["items"] = parsedTraks;

            return parsedPlaylist;

        }

        private static JsonObject ParsePlaylistMetaData(JsonObject? data)
        {

            JsonObject playlistData = new JsonObject();

            if (data != null)
            {

                JsonObject? microformatRenderer = (JsonObject?)data?["microformat"]?["microformatDataRenderer"] ?? null;

                playlistData["title"] = microformatRenderer?["title"]?.GetValue<string>();
                playlistData["description"] = microformatRenderer?["description"]?.GetValue<string>();
                playlistData["thumbnail"] = microformatRenderer?["thumbnail"]?["thumbnails"]?[0]?["url"]?.GetValue<string>();

            }

            return playlistData;

        }

        private static string? FindContinuationToken(JsonArray? traks)
        {
            string? token = null;


            if (traks != null)
            {

                JsonObject? lastElement = (JsonObject?)traks[traks.Count - 1];

                if (lastElement != null && lastElement.ContainsKey("continuationItemRenderer"))
                {
                    token = lastElement?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.GetValue<string>() ?? null;

                    return token;

                }

            }
            else
            {
                return null;
            }


            return null;
        }

        private static async Task<JsonArray?> BrowseContinuation(string continuation, JsonObject? cookies)
        {

            JsonObject continuationResult = await Continuation.BrowseContinuation(continuation, cookies);

            JsonArray? continuationContent = (JsonArray?)continuationResult?["onResponseReceivedActions"]?[0]?["appendContinuationItemsAction"]?["continuationItems"];

            return continuationContent;

        }

    }

}
