using System.Text.Json.Nodes;

namespace YoutubeMusic
{

    public enum PrivacyStatus
    {

        UNLISTED,
        PUBLIC,
        PRIVATE


    }


    public class Interactions
    {
        private readonly YTMusicSharp yt;
        private readonly JsonObject? youtubHeaders;
        internal Interactions(JsonObject headers, YTMusicSharp yT)
        {
            yt = yT;
            youtubHeaders = headers;
        }


        public async Task SetSongLikeStatus(string id, LikeStatus likeStatus)
        {
            await SongsInteractions.SetSongLikeStatus(id, likeStatus, youtubHeaders);
        }

        public async Task SetArtistSubscription(string browseId, bool subscribe)
        {
            await ArtistInteraction.SetArtistSubscriptionStatus(browseId, subscribe, youtubHeaders);
        }

        public async Task SetPlaylistSave(string browseId, bool save)
        {
            await AlbumInteractions.SetPlaylistSaveStatus(browseId, save, youtubHeaders);
        }

        public async Task<JsonObject> GetAddToPlaylistMenu()
        {
            return await SongsInteractions.GetAddToPlaylistOptionList(youtubHeaders);
        }

        public async Task AddVideoToPlaylist(string[] ids, string playlistId)
        {

            await SongsInteractions.AddToPlaylist(playlistId, ids, youtubHeaders);

        }

        public async Task RemoveVideoFromPlaylist(string ids, string setVideoId, string playlistId)
        {

            await SongsInteractions.RemoveFromPlaylist(playlistId, ids, setVideoId, youtubHeaders);

        }


        public async Task SetPlaylistCollaborate(string playlistId, bool collaborate)
        {
            await PlaylistInteraction.SetPlaylistCollaborators(playlistId, collaborate, youtubHeaders);
        }

        public async Task EditPLaylist(string playlistId,
        string? pTitle = null,
        string? pDescriprtion = null,
        PrivacyStatus? privacyStatus = null)
        {
            await PlaylistInteraction.EditPlaylist(playlistId, youtubHeaders, title: pTitle, description: pDescriprtion, privacyStatus: privacyStatus);
        }

        public async Task DeletePLaylist(string playlistId)
        {
            await PlaylistInteraction.DeletePlaylist(playlistId, youtubHeaders);
        }

        public async Task CreatePlaylist(
        string? pTitle = null,
        string? pDescriprtion = null,
        PrivacyStatus? privacyStatus = null)
        {

            await PlaylistInteraction.CreatePlaylist(youtubHeaders, pTitle, pDescriprtion, privacyStatus);

        }
    }
}
