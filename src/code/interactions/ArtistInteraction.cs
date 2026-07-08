using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    internal class ArtistInteraction
    {
        internal static async Task SetArtistSubscriptionStatus(string browseId, bool subscribe, JsonObject? headers)
        {
            if (headers == null)
            {
                return;
            }

            JsonArray targets = [browseId];

            var payload = new JsonObject();
            payload["channelIds"] = targets;


            if (subscribe)
            {
                await Requester.PostRequest(endpointUrl: "subscription/subscribe", cookies: headers, payload: payload);
            }
            else
            {
                await Requester.PostRequest(endpointUrl: "subscription/unsubscribe", cookies: headers, payload: payload);
            }




        }
    }
}
