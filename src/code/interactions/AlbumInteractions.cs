using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    class AlbumInteractions
    {
        internal static async Task SetPlaylistSaveStatus(string browseId, bool state, JsonObject? headers)
        {
            if (headers == null)
            {
                return;
            }


            var payload = new JsonObject();

            var target = new JsonObject();
            target["playlistId"] = browseId;
            payload["target"] = target;



            if (state)
            {
                await Requester.PostRequest(endpointUrl: "like/like", cookies: headers, payload: payload);
            } else
            {
                await Requester.PostRequest(endpointUrl: "like/removelike", cookies: headers, payload: payload);
            }

        }
    }
}
