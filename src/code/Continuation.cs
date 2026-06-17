using System.Text.Json.Nodes;

namespace YoutubeMusic;


internal class Continuation
{
    internal static async Task<JsonObject> BrowseContinuation(string continuationToken, JsonObject? headers)
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

        payload["continuation"] = continuationToken;
        JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);

        return result;

    }
}
