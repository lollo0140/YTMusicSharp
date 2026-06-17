using System.Text.Json;
using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    class Parsing
    {
        internal static JsonObject ParseMusicResponsiveListItemRenderer(JsonObject MRLIR)
        {
            if (MRLIR == null) return new JsonObject();

            string contentType = Util.DetectContentType(MRLIR);

            // Start parsing from the flexcolumns
            JsonArray? flexColumns = (JsonArray?)MRLIR["flexColumns"];
            JsonObject parsedTrack;

            if (flexColumns != null)
            {
                parsedTrack = ParseFlexColumns(flexColumns);
            }
            else
            {
                System.Console.WriteLine("no fc");
                parsedTrack = new JsonObject();
            }

            if (!parsedTrack.ContainsKey("browseId") && !parsedTrack.ContainsKey("id"))
            {
                parsedTrack["browseId"] = MRLIR["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>() ?? "none";
            }

            parsedTrack["explicit"] = Util.SearchExplicitContentBadge((JsonArray)MRLIR?["badges"]! ?? []);

            // Add the content type
            parsedTrack["type"] = contentType;


            string? setVideoIdPos = MRLIR?["overlay"]?["musicItemThumbnailOverlayRenderer"]?["content"]?["musicPlayButtonRenderer"]?["playNavigationEndpoint"]?["watchEndpoint"]?["playlistSetVideoId"]?.GetValue<string>() ?? null;


            if (setVideoIdPos != null)
            {
                parsedTrack["setVideoId"] = setVideoIdPos;
            }


            if (MRLIR!.ContainsKey("thumbnail"))
            {
                parsedTrack["thumbnails"] = ThumbnailParser.ParseThumbnails((JsonObject)MRLIR?["thumbnail"]!);
            }

            return parsedTrack;
        }

        internal static JsonObject ParseFlexColumns(JsonArray flexColumns)
        {
            JsonObject parsedFlexColumn = new JsonObject();
            JsonArray? artists = new JsonArray();

            foreach (var (item, i) in flexColumns.Select((value, i) => (value, i)))
            {
                JsonArray? _ = (JsonArray?)item?["musicResponsiveListItemFlexColumnRenderer"]?["text"]?["runs"] ?? null;



                if (_ != null)
                {


                    foreach (JsonObject? run in _)
                    {

                        if (run != null && run.ContainsKey("navigationEndpoint"))
                        {

                            JsonObject? navEndpoint = (JsonObject?)run?["navigationEndpoint"] ?? null;


                            //if video
                            if (navEndpoint != null && navEndpoint.ContainsKey("watchEndpoint"))
                            {
                                parsedFlexColumn["title"] = run?["text"]?.GetValue<string>() ?? "No title";
                                parsedFlexColumn["id"] = navEndpoint?["watchEndpoint"]?["videoId"]?.GetValue<string>() ?? "No id";
                            }

                            if (navEndpoint != null && run != null && navEndpoint.ContainsKey("browseEndpoint"))
                            {

                                //if page type points to a channel
                                if (parsingUtil.IsChannel(navEndpoint!))
                                {
                                    artists.Add(flexColumnParsing.ParseArtistOrChannel(run));
                                }
                                //now check for the album link
                                else if (parsingUtil.IsAlbum(navEndpoint))
                                {
                                    parsedFlexColumn["album"] = flexColumnParsing.ParseAlbum(run);
                                }
                                else
                                {
                                    parsedFlexColumn["itemTitle"] = run?["text"]?.GetValue<string>() ?? "No title";
                                    parsedFlexColumn["browseId"] = navEndpoint?["browseEndpoint"]?["browseId"]?.GetValue<string>();
                                }


                            }
                        }
                        else if (i == 0)
                        {

                            parsedFlexColumn["itemTitle"] = run?["text"]?.GetValue<string>();

                        }

                    }

                }

            }

            if (artists.Count > 0)
            {
                parsedFlexColumn["artists"] = artists;
            }



            return parsedFlexColumn;

        }




        internal static JsonObject ParsemusicTwoRowItemRenderer(JsonObject? MCSR)
        {

            JsonObject parsedTwoRowItemRenderer = new JsonObject();


            if (MCSR != null)
            {

                JsonObject? titleNode = (JsonObject?)MCSR?["title"]?["runs"]?[0];
                JsonArray? subtitleNode = (JsonArray?)MCSR?["subtitle"]?["runs"];

                parsedTwoRowItemRenderer["thumbnails"] = ThumbnailParser.ParseThumbnails((JsonObject)MCSR?["thumbnailRenderer"]!);

                if (titleNode != null)
                {
                    string type = Util.DetectRunContentType(MCSR!);
                    parsedTwoRowItemRenderer["type"] = type;





                    parsedTwoRowItemRenderer[type == "track" || type == "video" ? "id" : "browseId"] = titleNode?["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>() ?? MCSR?["navigationEndpoint"]?["watchEndpoint"]?["videoId"]?.GetValue<string>();

                    parsedTwoRowItemRenderer["title"] = titleNode?["text"]?.GetValue<string>();
                }

                JsonArray artists = [];

                if (subtitleNode != null)
                {
                    foreach (JsonObject item in subtitleNode!)
                    {

                        string runType = Util.DetectRunContentType(item!);


                        if (runType == "artist")
                        {

                            artists.Add(flexColumnParsing.ParseArtistOrChannel(item));

                        }

                        if (runType == "album")
                        {
                            parsedTwoRowItemRenderer["album"] = flexColumnParsing.ParseAlbum(item);
                        }


                    }

                }


                if (artists.Count > 0)
                {
                    parsedTwoRowItemRenderer["artists"] = artists;
                }

            }



            return parsedTwoRowItemRenderer;

        }

    }
}
