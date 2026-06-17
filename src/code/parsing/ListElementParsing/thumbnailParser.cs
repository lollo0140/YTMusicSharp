using System.Text.Json.Nodes;

namespace YoutubeMusic
{

    internal static class ThumbnailParser
    {

        internal static JsonArray ParseThumbnails(JsonObject thumbnail)
        {

            JsonArray? T = (JsonArray?)thumbnail?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"];

            if (T != null)
            {

                JsonArray TUrls = new JsonArray();

                foreach (JsonObject? img in T)
                {
                    TUrls.Add(img?["url"]?.GetValue<string>() ?? "none");
                }

                return TUrls;

            }

            return [];

        }

    }

}
