using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    public class Browse
    {
        private readonly JsonObject? youtubHeaders;
        private readonly YTMusicSharp yt;


        internal Browse(JsonObject headers, YTMusicSharp yT)
        {
            yt = yT;
            youtubHeaders = headers;
        }





        public async Task<JsonObject> FetchAlbumData(string browseId)
        {

            JsonObject data = await Album.FetchAlbumData(browseId, youtubHeaders);

            yt.DB_Insert(browseId, data, DB_filter.ALBUM);

            return data;

        }

        public async Task<JsonObject> FetchPlaylistData(string browseId)
        {
            JsonObject data = await Playlist.FetchPlaylistData(browseId, youtubHeaders);

            yt.DB_Insert(browseId, data, DB_filter.PLAYLIST);

            return data;
        }

        public async Task<JsonObject> FetchArtistPage(string browseId)
        {
            JsonObject data = await ArtistPage.FetchArtistPage(browseId, youtubHeaders);

            yt.DB_Insert(browseId, data, DB_filter.ARTIST);

            return data;
        }

        public async Task<JsonObject> FetchHomeSection(string? continuationToken = null)
        {

            return await HomeData.GetHomeSection(youtubHeaders, continuation: continuationToken);

        }

        public async Task<JsonObject> FetchHomeSections()
        {
            return await HomeData.GetHomeSections(youtubHeaders);
        }

    }
}
