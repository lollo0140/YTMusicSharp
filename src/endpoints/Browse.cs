using System.Text.Json.Nodes;
using DebugUtility;

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





        public async Task<JsonObject> FetchAlbumData(string browseId, bool filterTracks = false)
        {

            JsonObject data = await Album.FetchAlbumData(browseId, youtubHeaders);

            if (filterTracks)
            {
                data["items"] = (await Album.FilterTracks(data)).DeepClone();
            }


            if (data != null)
            {
                return data;
            }
            return [];
        }




        public async Task<JsonObject> FetchPlaylistData(string browseId)
        {
            JsonObject data = await Playlist.FetchPlaylistData(browseId, youtubHeaders);


            if (data != null)
            {
                return data;
            }

            return [];
        }

        public async Task<JsonObject> FetchArtistPage(string browseId)
        {

            JsonObject data = await ArtistPage.FetchArtistPage(browseId, youtubHeaders);


            if (data != null)
            {
                return data;
            }


            return [];
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
