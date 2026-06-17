using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    internal static class Util
    {

        internal static bool SearchExplicitContentBadge(JsonArray badges)
        {
            foreach (var badge in badges)
            {
                string? badgeIcon = badge?["musicInlineBadgeRenderer"]?["icon"]?["iconType"]?.GetValue<string>() ?? null!;

                if (badgeIcon != null)
                {
                    if (badgeIcon == "MUSIC_EXPLICIT_BADGE")
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        internal static string DetectContentType(JsonNode musicResponsiveListItemRenderer)
        {

            //cerca il video type per prima cosa
            string? videoType = musicResponsiveListItemRenderer?["flexColumns"]?[0]?["musicResponsiveListItemFlexColumnRenderer"]?["text"]?["runs"]?[0]?["navigationEndpoint"]?["watchEndpoint"]?["watchEndpointMusicSupportedConfigs"]?["watchEndpointMusicConfig"]?["musicVideoType"]?.GetValue<string>() ?? null;

            if (videoType != null)
            {
                if (videoType == "MUSIC_VIDEO_TYPE_ATV")
                {
                    return "track";
                }
                else if (videoType == "MUSIC_VIDEO_TYPE_OMV" || videoType == "MUSIC_VIDEO_TYPE_UGC")
                {
                    return "video";
                }
            }

            //se non è ne traccia ne video controlla se è un album

            string? pageType = musicResponsiveListItemRenderer?["navigationEndpoint"]?["browseEndpoint"]?["browseEndpointContextSupportedConfigs"]?["browseEndpointContextMusicConfig"]?["pageType"]?.GetValue<string>() ?? null;

            if (pageType != null)
            {
                switch (pageType)
                {
                    case "MUSIC_PAGE_TYPE_ALBUM":
                        return "album";

                    case "MUSIC_PAGE_TYPE_ARTIST":
                        return "artist";

                    case "MUSIC_PAGE_TYPE_PLAYLIST":
                        return "playlist";

                    case "MUSIC_PAGE_TYPE_USER_CHANNEL":
                        return "channel";

                    case "MUSIC_PAGE_TYPE_PODCAST_SHOW_DETAIL_PAGE":
                        return "podcast";

                    default:
                        return "text";
                }

            }

            //verifica ulteriore per le puntate dei podcast

            string? episode = musicResponsiveListItemRenderer?["overlay"]?["musicItemThumbnailOverlayRenderer"]?["content"]?["musicPlayButtonRenderer"]?["playNavigationEndpoint"]?["watchEndpoint"]?["watchEndpointMusicSupportedConfigs"]?["watchEndpointMusicConfig"]?["musicVideoType"]?.GetValue<string>();

            if (episode == "MUSIC_VIDEO_TYPE_PODCAST_EPISODE")
            {
                return "episode";
            }


            return "none";

        }

        internal static string DetectRunContentType(JsonObject run)
        {

            if (!run.ContainsKey("navigationEndpoint"))
            {
                return "text";
            }

            if (run["navigationEndpoint"] == null)
            {
                return "text";
            }

            JsonObject endpoint = (JsonObject)run?["navigationEndpoint"]!;


            if (endpoint.ContainsKey("watchEndpoint"))
            {
                JsonObject watchEndpoint = (JsonObject)endpoint?["watchEndpoint"]!;

                string? videoType = watchEndpoint?["watchEndpointMusicSupportedConfigs"]?["watchEndpointMusicConfig"]?["musicVideoType"]?.GetValue<string>() ?? null;

                if (videoType != null)
                {
                    if (videoType == "MUSIC_VIDEO_TYPE_ATV")
                    {
                        return "track";
                    }
                    else if (videoType == "MUSIC_VIDEO_TYPE_OMV" || videoType == "MUSIC_VIDEO_TYPE_UGC")
                    {
                        return "video";
                    }
                    else
                    {
                        return "text";
                    }
                }


            }
            else if (endpoint.ContainsKey("browseEndpoint"))
            {
                JsonObject browseEndpoint = (JsonObject)endpoint?["browseEndpoint"]!;

                string? pageType = browseEndpoint?["browseEndpointContextSupportedConfigs"]?["browseEndpointContextMusicConfig"]?["pageType"]?.GetValue<string>() ?? null;

                if (pageType != null)
                {

                    switch (pageType)
                    {
                        case "MUSIC_PAGE_TYPE_ALBUM":
                            return "album";

                        case "MUSIC_PAGE_TYPE_ARTIST":
                            return "artist";

                        case "MUSIC_PAGE_TYPE_PLAYLIST":
                            return "playlist";

                        case "MUSIC_PAGE_TYPE_USER_CHANNEL":
                            return "channel";

                        case "MUSIC_PAGE_TYPE_PODCAST_SHOW_DETAIL_PAGE":
                            return "podcast";

                        default:
                            return "text";
                    }

                }

            }
            else
            {
                if (run!.ContainsKey("text"))
                {
                    return "text";
                } else
                {
                    return "none";
                }
            }

            return "none";

        }
 
    }
}
