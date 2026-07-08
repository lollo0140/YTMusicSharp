using System.Net.Http.Headers;
using System.Text.Json.Nodes;

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


    }
}
