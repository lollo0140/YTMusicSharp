using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    public class Library
    {
        private readonly YTMusicSharp yt;
        private readonly JsonObject? youtubHeaders;
        internal Library(JsonObject headers, YTMusicSharp yT)
        {
            yt = yT;
            youtubHeaders = headers;
        }

        public async Task<JsonObject> GetLikedTracks()
        {
            return await Playlist.FetchUserLikePlaylist(youtubHeaders);
        }

        public async Task<JsonObject> GetSavedEpisodes()
        {
            return await Playlist.FetchUserEpisodePlaylist(youtubHeaders);
        }

        public async Task<JsonObject> GetLibraryLandingPage()
        {
            return await LibraryData.GetUserLibrary(youtubHeaders);
        }

        public async Task<JsonObject> GetLibraryContent(ContentFilter filter)
        {
            return await LibraryData.GetUserContentByFilter(youtubHeaders, filter);
        }

    }

}
