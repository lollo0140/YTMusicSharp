using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    internal static class parsingUtil
    {

        internal static bool IsChannel(JsonObject navEndpoint)
        {
            //navigationEndpoint browseEndpoint browseEndpointContextSupportedConfigs browseEndpointContextMusicConfig pageType

            if (navEndpoint != null)
            {
                string? navPageType = navEndpoint?["browseEndpoint"]?["browseEndpointContextSupportedConfigs"]?["browseEndpointContextMusicConfig"]?["pageType"]?.GetValue<string>() ?? null;


                if (navPageType != null && navPageType == "MUSIC_PAGE_TYPE_ARTIST" || navPageType == "MUSIC_PAGE_TYPE_USER_CHANNEL")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        internal static bool IsAlbum(JsonObject navEndpoint)
        {

            if (navEndpoint != null)
            {
                string? navPageType = navEndpoint?["browseEndpoint"]?["browseEndpointContextSupportedConfigs"]?["browseEndpointContextMusicConfig"]?["pageType"]?.GetValue<string>() ?? null;

                if (navPageType == "MUSIC_PAGE_TYPE_ALBUM")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;

        }

    }

}
