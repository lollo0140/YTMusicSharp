using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtility;

namespace YoutubeMusic
{

    internal class Album
    {

        internal static async Task<JsonObject> FetchAlbumData(string BrowseId, JsonObject? headers)
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

            payload["browseId"] = BrowseId;
            JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);


            JsonArray? tracks = (JsonArray?)result["contents"]?["twoColumnBrowseResultsRenderer"]?["secondaryContents"]?["sectionListRenderer"]?["contents"]?[0]?["musicShelfRenderer"]?["contents"];


            JsonObject? albumData = (JsonObject?)result?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0]?["musicResponsiveHeaderRenderer"];




            JsonObject parsedAlbum = new JsonObject();


            if (albumData != null)
            {
                parsedAlbum["data"] = ParseAlbumInfos(albumData, BrowseId);
            }
            else
            {
                parsedAlbum["data"] = new JsonObject();
            }



            if (tracks != null)
            {
                parsedAlbum["items"] = ParseAlbumTracks(tracks, (JsonObject?)parsedAlbum["data"]);
            }
            else
            {
                parsedAlbum["items"] = new JsonArray();
            }






            return parsedAlbum;
        }









        //parsing

        private static JsonArray ParseAlbumTracks(JsonArray itemListRender, JsonObject? albumData)
        {

            JsonArray parsedTracks = [];





            foreach (JsonObject? track in itemListRender)
            {

                if (track != null)
                {

                    JsonObject parsedElements = Parsing.ParseMusicResponsiveListItemRenderer((JsonObject?)track?["musicResponsiveListItemRenderer"]!);



                    if (!parsedElements.ContainsKey("artists") && albumData != null)
                    {
                        JsonArray artists = [];


                        JsonObject artist = new JsonObject();
                        artist["artistId"] = albumData["artist"]?["artistId"]?.GetValue<string>();
                        artist["artistName"] = albumData["artist"]?["artistName"]?.GetValue<string>();

                        artists.Add(artist);

                        parsedElements["artists"] = artists;

                    }


                    parsedTracks.Add(parsedElements);

                }

            }


            return parsedTracks;

        }

        private static JsonObject ParseAlbumInfos(JsonObject data, string brosweId)
        {

            JsonObject parsedAlbumData = new JsonObject();


            parsedAlbumData["brosweId"] = brosweId;


            //thumbnails -------------------

            JsonArray? thumbnails = (JsonArray?)data?["thumbnail"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"];

            JsonArray parsedThumbnails = [];


            if (thumbnails != null && thumbnails.Count > 0)
            {
                foreach (JsonObject? item in (thumbnails ?? []).Cast<JsonObject?>())
                {
                    if (item != null)
                    {
                        parsedThumbnails.Add(item?["url"]?.GetValue<string>());
                    }
                }
            }

            parsedAlbumData["thumbnails"] = parsedThumbnails;

            //thumbnails -------------------


            //buttons -------------------

            parsedAlbumData["saved"] = data?["buttons"]?[0]?["toggleButtonRenderer"]?["isToggled"]?.GetValue<bool>() ?? false;
            parsedAlbumData["saveParam"] = data?["buttons"]?[0]?["toggleButtonRenderer"]?["defaultServiceEndpoint"]?["likeEndpoint"]?["target"]?["playlistId"]?.GetValue<string>();
            parsedAlbumData["shareLink"] = $"https://music.youtube.com/playlist?list={parsedAlbumData["saveParam"]!.GetValue<string>() ?? ""}";

            //buttons -------------------


            //title ----------------------

            parsedAlbumData["title"] = data?["title"]?["runs"]?[0]?["text"]?.GetValue<string>();

            //title ----------------------


            //subtitle -------------------

            JsonArray? runs = (JsonArray?)data?["subtitle"]?["runs"];

            string subtitle = "";

            if (runs != null && runs.Count > 0)
            {
                foreach (JsonObject? run in (runs ?? []).Cast<JsonObject?>())
                {
                    subtitle += run?["text"] ?? "";
                }
            }

            parsedAlbumData["subtitle"] = subtitle;

            //subtitle -------------------



            //straplineTextOne -----------

            JsonObject artist = new JsonObject();
            JsonObject? straplineTextOne = (JsonObject?)data?["straplineTextOne"]?["runs"]?[0];


            if (straplineTextOne != null)
            {
                artist["name"] = straplineTextOne?["text"]?.GetValue<string>() ?? "";
                artist["browseId"] = straplineTextOne?["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>() ?? "";
            }

            parsedAlbumData["artist"] = artist;

            //straplineTextOne -----------



            //subtitleBadge --------------

            JsonArray? badges = (JsonArray?)data?["subtitleBadge"];

            parsedAlbumData["explicit"] = false;

            if (badges != null && badges.Count > 1)
            {
                foreach (var item in badges)
                {
                    string icon = item?["musicInlineBadgeRenderer"]?["icon"]?["iconType"]?.GetValue<string>() ?? "";

                    if (icon == "MUSIC_EXPLICIT_BADGE")
                    {
                        parsedAlbumData["explicit"] = true;
                    }

                }
            }


            //subtitleBadge --------------



            //second --------------------

            JsonArray? secondRuns = (JsonArray?)data?["secondSubtitle"]?["runs"];

            string secondSubtitle = "";

            if (secondRuns != null && secondRuns.Count > 0)
            {
                foreach (JsonObject? run in (secondRuns ?? []).Cast<JsonObject?>())
                {
                    secondSubtitle += run?["text"] ?? "";
                }
            }

            parsedAlbumData["secondSubtitle"] = secondSubtitle;


            //second --------------------

            return parsedAlbumData;

        }

    }

}
