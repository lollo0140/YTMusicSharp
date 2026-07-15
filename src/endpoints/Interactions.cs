using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    public class Interactions
    {
        private readonly YTMusicSharp yt;
        private readonly JsonObject? youtubHeaders;
        internal Interactions(JsonObject headers, YTMusicSharp yT)
        {
            yt = yT;
            youtubHeaders = headers;
        }

        public async void SetSongLikeStatus(string id, LikeStatus likeStatus)
        {
            await SongsInteractions.SetSongLikeStatus(id, likeStatus, youtubHeaders);
        }

        public async void SetArtistSubscription(string browseId, bool subscribe)
        {
            await ArtistInteraction.SetArtistSubscriptionStatus(browseId, subscribe, youtubHeaders);
        }

        public async void SetPlaylistSave(string browseId, bool save)
        {
            await AlbumInteractions.SetPlaylistSaveStatus(browseId, save, youtubHeaders);
        }

        public async Task<JsonObject> GetAddToPlaylistMenu()
        {
            return await SongsInteractions.GetAddToPlaylistOptionList(youtubHeaders);
        }

        public async void AddVideoToPlaylist(string[] ids, string playlistId)
        {

            await SongsInteractions.AddToPlaylist(playlistId, ids, youtubHeaders);

        }

        public async void RemoveVideoFromPlaylist(string ids, string setVideoId, string playlistId)
        {

            await SongsInteractions.RemoveFromPlaylist(playlistId, ids, setVideoId, youtubHeaders);

        }
    }
}
