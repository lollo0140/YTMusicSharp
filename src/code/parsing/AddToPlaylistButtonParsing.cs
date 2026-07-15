using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    class AddToPlaylistButtonParsing
    {

        internal static JsonObject ParseATPmusicTwoRowItemRenderer(JsonObject? data)
        {
            if (data != null && data.ContainsKey("musicTwoRowItemRenderer"))
            {

                JsonObject? root = (JsonObject?)data?["musicTwoRowItemRenderer"];

                JsonObject parsedMusicTwoRowItemRenderer = [];




                parsedMusicTwoRowItemRenderer["title"] = root?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "none";

                parsedMusicTwoRowItemRenderer["subtitle"] = root?["subtitle"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "none";

                parsedMusicTwoRowItemRenderer["browseId"] = root?["navigationEndpoint"]?["playlistEditEndpoint"]?["playlistId"]?.GetValue<string>() ?? "none";


                JsonArray thumbnails = [];

                JsonArray toParseThumbnails = root?["thumbnailRenderer"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"]?.AsArray() ?? [];

                foreach (JsonObject item in (toParseThumbnails ?? []).Cast<JsonObject>())
                {

                    thumbnails.Add(item["url"]?.GetValue<string>() ?? "none");

                }

                parsedMusicTwoRowItemRenderer["thumbnails"] = thumbnails;


                return parsedMusicTwoRowItemRenderer;
            }

            return [];

        }


        internal static JsonObject ParsePlaylistAddToOptionRenderer(JsonObject? data)
        {
            if (data != null && data.ContainsKey("playlistAddToOptionRenderer"))
            {
                JsonObject? root = (JsonObject?)data?["playlistAddToOptionRenderer"];

                JsonObject parsedPlaylistAddToOptionRenderer= [];


                parsedPlaylistAddToOptionRenderer["title"] = root?["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? "none";

                parsedPlaylistAddToOptionRenderer["subtitle"] = root?["shortBylineText"]?["simpleText"]?.GetValue<string>() ?? "none";

                parsedPlaylistAddToOptionRenderer["browseId"] = root?["playlistId"]?.GetValue<string>() ?? "none";


                JsonArray thumbnails = [];

                JsonArray toParseThumbnails = root?["thumbnailRenderer"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"]?.AsArray() ?? [];

                foreach (JsonObject item in (toParseThumbnails ?? []).Cast<JsonObject>())
                {

                    thumbnails.Add(item["url"]?.GetValue<string>() ?? "none");

                }

                parsedPlaylistAddToOptionRenderer["thumbnails"] = thumbnails;


                return parsedPlaylistAddToOptionRenderer;



            }

            return [];
        }

    }
}
