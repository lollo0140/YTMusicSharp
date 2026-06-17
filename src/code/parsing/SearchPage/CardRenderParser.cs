using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    class CardRenderParser
    {
        internal static JsonObject ParseCardRender(JsonObject cardRenderElement)
        {

            JsonObject? cardRender = (JsonObject?)cardRenderElement?["musicCardShelfRenderer"] ?? null;

            if (cardRender != null)
            {
                JsonObject? cardRenderTitle = (JsonObject?)cardRender?["title"]?["runs"]?[0];
                JsonArray? cardRenderSubtitle = (JsonArray?)cardRender?["subtitle"]?["runs"];
                JsonObject? cardRenderThumbnails = (JsonObject?)cardRender?["thumbnail"];
                JsonArray? cardRenderContent = (JsonArray?)cardRender?["contents"];


                JsonObject parsed = new JsonObject();

                if (cardRender!.ContainsKey("subtitleBadges"))
                {
                    parsed["explicit"] = Util.SearchExplicitContentBadge((JsonArray)cardRender?["subtitleBadges"]!);

                }


                //parsing title
                ParseTitle(cardRenderTitle, parsed);

                //parsing subtitle
                ParseSubtitle(cardRenderSubtitle, parsed);

                if (cardRenderThumbnails != null)
                {
                    parsed["thumbnails"] = ThumbnailParser.ParseThumbnails(cardRenderThumbnails);
                }

                if (cardRenderContent != null && cardRenderContent.Count > 0)
                {
                    parsed["content"] = ParseCardContent(cardRenderContent);
                }

                return parsed;
            }

            return new JsonObject();
        }

        internal static void ParseTitle(JsonObject? titleRender, JsonObject parsed)
        {
            if (titleRender != null)
            {
                string contentType = Util.DetectRunContentType(titleRender);

                parsed["type"] = contentType;

                if (contentType == "track" || contentType == "video")
                {
                    parsed["title"] = titleRender?["text"]?.GetValue<string>() ?? "No title";
                    parsed["id"] = titleRender?["navigationEndpoint"]?["watchEndpoint"]?["videoId"]?.GetValue<string>() ?? "No id";
                }
                else
                {

                    parsed["itemTitle"] = titleRender?["text"]?.GetValue<string>() ?? "No title";
                    parsed["browseId"] = titleRender?["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>();

                }

            }
        }

        internal static void ParseSubtitle(JsonArray? subtitleRenderRun, JsonObject parsed)
        {

            if (subtitleRenderRun != null)
            {
                JsonArray artists = [];
                JsonObject? albumInfo = null;

                foreach (JsonObject? run in subtitleRenderRun)
                {
                    if (run != null)
                    {
                        string contentType = Util.DetectRunContentType(run);

                        if (contentType == "text")
                        {
                            continue;
                        }

                        if (parsingUtil.IsChannel((JsonObject)run?["navigationEndpoint"]!))
                        {
                            artists.Add(flexColumnParsing.ParseArtistOrChannel(run!));
                        }
                        else if (parsingUtil.IsAlbum((JsonObject)run?["navigationEndpoint"]!))
                        {
                            albumInfo = flexColumnParsing.ParseAlbum(run!);
                        }

                    }

                }

                if (albumInfo != null)
                {
                    parsed["album"] = albumInfo;
                }

                if (artists.Count > 0)
                {
                    parsed["artists"] = artists;
                }

            }

        }

        internal static JsonArray ParseCardContent(JsonArray content)
        {

            JsonArray parsedContent = [];

            foreach (JsonObject? MRLIR in content)
            {
                if (MRLIR != null)
                {

                    parsedContent.Add(Parsing.ParseMusicResponsiveListItemRenderer((JsonObject)MRLIR?["musicResponsiveListItemRenderer"]!));

                }
            }


            return parsedContent;
        }

    }
}
