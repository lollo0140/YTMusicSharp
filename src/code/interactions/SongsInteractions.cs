using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using DebugUtility;

namespace YoutubeMusic
{
    internal class SongsInteractions
    {

        internal static async Task SetSongLikeStatus(string VideoId, LikeStatus likeStatus, JsonObject? headers)
        {
            if (headers == null)
            {
                return;
            }


            var payload = new JsonObject();

            var target = new JsonObject();
            target["videoId"] = VideoId;
            payload["target"] = target;


            switch (likeStatus)
            {

                case LikeStatus.LIKE:
                    await Requester.PostRequest(endpointUrl: "like/like", cookies: headers, payload: payload);
                    break;
                case LikeStatus.DISLIKE:
                    await Requester.PostRequest(endpointUrl: "like/dislike", cookies: headers, payload: payload);
                    break;
                case LikeStatus.NEUTRAL:
                    await Requester.PostRequest(endpointUrl: "like/removelike    ", cookies: headers, payload: payload);
                    break;
            }







        }

        internal static async Task<JsonObject> GetAddToPlaylistOptionList(JsonObject? headers)
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

            payload["excludeWatchLater"] = true;
            payload["videoIds"] = new JsonArray(["q9JITaxvwFc"]);

            JsonObject result = await Requester.PostRequest(endpointUrl: "playlist/get_add_to_playlist", cookies: C, payload: payload);


            JsonObject? content = (JsonObject?)result?["contents"]?[0]?["addToPlaylistRenderer"];

            JsonArray? playlists = (JsonArray?)content?["playlists"];
            string title = content?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "none";


            JsonObject parsedAddToPlaylist = [];

            //TOPSHELF ------------------
            JsonObject? topShelf = (JsonObject?)content?["topShelf"]?["musicCarouselShelfRenderer"];
            JsonArray? topShelfPlaylists = (JsonArray?)topShelf?["contents"];
            string topShelfTitle = topShelf?["header"]?["musicCarouselShelfBasicHeaderRenderer"]?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "none";

            JsonObject parsedTopShelf = [];
            JsonArray parsedTopShelfPlaylists = [];


            foreach (JsonObject P in (topShelfPlaylists ?? []).Cast<JsonObject>())
            {
                parsedTopShelfPlaylists.Add(AddToPlaylistButtonParsing.ParseATPmusicTwoRowItemRenderer(P));
            }

            parsedTopShelf["playlists"] = parsedTopShelfPlaylists;
            parsedTopShelf["title"] = topShelfTitle;
            //TOPSHELF ------------------


            //CONTENT -------------------

            JsonObject parsedMainSec = [];

            JsonArray? parsedPlaylist = [];

            foreach (JsonObject P in (playlists ?? []).Cast<JsonObject>())
            {
                parsedPlaylist.Add(AddToPlaylistButtonParsing.ParsePlaylistAddToOptionRenderer(P));
            }

            parsedMainSec["playlists"] = parsedPlaylist;
            parsedMainSec["title"] = title;

            //CONTENT -------------------


            parsedAddToPlaylist["content"] = parsedMainSec;
            parsedAddToPlaylist["topShelf"] = parsedTopShelf;

            YTSharpDebugClass.WriteJsonToTestFile(parsedAddToPlaylist ?? [], 1);

            return parsedAddToPlaylist ?? [];

        }

        internal static async Task AddToPlaylist(string playlistId, string videoId, JsonObject? headers)
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


            JsonObject songAction = [];
            songAction["action"] = "ACTION_ADD_VIDEO";
            songAction["addedVideoId"] = videoId;


            payload["actions"] = new JsonArray([songAction]);
            payload["playlistId"] = playlistId;
            await Requester.PostRequest(endpointUrl: "browse/edit_playlist", cookies: C, payload: payload);
        }

        internal static async Task AddToPlaylist(string playlistId, string[] videosId, JsonObject? headers)
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


            JsonArray actions = [];


            foreach (string id in videosId)
            {
                JsonObject songAction = [];
                songAction["action"] = "ACTION_ADD_VIDEO";
                songAction["addedVideoId"] = id;

                actions.Add(songAction.DeepClone());
            }


            payload["actions"] = actions;
            payload["playlistId"] = playlistId;
            await Requester.PostRequest(endpointUrl: "browse/edit_playlist", cookies: C, payload: payload);
        }

        internal static async Task RemoveFromPlaylist(string playlistId, string videoId, string setVideoId, JsonObject? headers)
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


            JsonObject songAction = [];
            songAction["action"] = "ACTION_REMOVE_VIDEO";
            songAction["removedVideoId"] = videoId;
            songAction["setVideoId"] = setVideoId;


            payload["actions"] = new JsonArray([songAction]);
            payload["playlistId"] = playlistId;
            await Requester.PostRequest(endpointUrl: "browse/edit_playlist", cookies: C, payload: payload);
        }


    }
}
