using System.Text.Json;
using System.Text.Json.Nodes;

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
                parsedAlbum["data"] = ParseAlbumInfos(albumData);
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

        private static JsonObject ParseAlbumInfos(JsonObject data)
        {

            JsonObject parsedAlbumData = new JsonObject();


            string? title = data["title"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? null;


            JsonArray? albumThumbnails = (JsonArray?)data?["thumbnail"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"] ?? null;


            JsonObject? strapLineTextOne = (JsonObject?)data?["straplineTextOne"]?["runs"]?[0];
            JsonArray? strapLineThumbnails = (JsonArray?)data?["straplineThumbnail"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"];







            if (title != null)
            {
                parsedAlbumData["title"] = title;
            }
            else
            {
                parsedAlbumData["title"] = "no title";
            }

            JsonObject artistData = new JsonObject();

            if (strapLineTextOne != null)
            {

                artistData["artistName"] = strapLineTextOne?["text"]?.GetValue<string>() ?? "no name";
                artistData["artistId"] = strapLineTextOne?["navigationEndpoint"]?["browseEndpoint"]?["browseId"]?.GetValue<string>() ?? "no browseId";

                parsedAlbumData["artist"] = artistData;
            }



            JsonArray thumbnails = [];

            if (strapLineThumbnails != null)
            {

                foreach (JsonObject? thumbnail in strapLineThumbnails)
                {

                    thumbnails.Add(thumbnail?["url"]?.GetValue<string>() ?? "no url");

                }

                artistData["thumbnails"] = thumbnails;

            }

            JsonArray parsedAlbumThumbnails = [];

            if (albumThumbnails != null)
            {
                foreach (JsonObject? thumbnail in albumThumbnails)
                {

                    parsedAlbumThumbnails.Add(thumbnail?["url"]?.GetValue<string>() ?? "no url");

                }

                parsedAlbumData["thumbnails"] = parsedAlbumThumbnails.DeepClone();
            }


            return parsedAlbumData;

        }

    }

}
