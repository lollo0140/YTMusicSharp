using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    class flexColumnParsing
    {
        internal static JsonObject ParseArtistOrChannel(JsonObject run)
        {

            JsonNode artist = new JsonObject();

            string? navPageType = run["navigationEndpoint"]?["browseEndpoint"]?["browseEndpointContextSupportedConfigs"]?["browseEndpointContextMusicConfig"]?["pageType"]?.GetValue<string>() ?? null;



            bool isChannel = navPageType == "MUSIC_PAGE_TYPE_ARTIST" ? false : true;


            artist[isChannel ? "channelId" : "artistId"] = run?["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>();

            artist[isChannel ? "channelName" : "artistName"] = run?["text"]?.GetValue<string>();

            return (JsonObject)artist;

        }

        internal static JsonObject ParseAlbum(JsonObject run)
        {
            JsonNode album = new JsonObject();

            album["titleName"] = run?["text"]?.GetValue<string>() ?? "No title";
            album["albumId"] = run?["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>();

            return (JsonObject)album;
        }

    }
}
