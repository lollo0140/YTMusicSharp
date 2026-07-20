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





        public async Task<JsonObject> FetchAlbumData(string browseId)
        {

            JsonObject data = await Album.FetchAlbumData(browseId, youtubHeaders);

            yt.DB_Insert(browseId, data, DB_filter.ALBUM);

            return data;

        }

        public async Task<JsonObject> FetchAlbumDataSongsOnly(string browseId)
        {
            JsonObject data = await Album.FetchAlbumData(browseId, youtubHeaders);

            if (data.ContainsKey("items") && data?["items"] != null)
            {
                JsonArray items = data?["items"]?.AsArray() ?? [];

                string artistId = data?["data"]?["artist"]?["browseId"]?.GetValue<string>() ?? "";
                string artistName = data?["data"]?["artist"]?["name"]?.GetValue<string>() ?? "";

                var searchTasks = items.Select(async S =>
                {
                    string songTitle = S?["title"]?.GetValue<string>() ?? "";
                    bool explicitBadge = S?["explicit"]?.GetValue<bool>() ?? false;
                    string query = $"{songTitle}+{artistName}";

                    JsonObject result = await yt.SearchEndpoint.SpecificSearch(query, ContentType.Track);

                    if (result == null)
                    {
                        return null;
                    }

                    JsonArray searchResults = result?["results"]?.AsArray() ?? [];

                    foreach (JsonObject? resItem in searchResults.Cast<JsonObject>())
                    {
                        bool validTitle = (resItem?["title"]?.GetValue<string>() ?? "").Contains(songTitle);
                        bool validArtId = (resItem?["artists"]?[0]?["artistId"]?.GetValue<string>() ?? "") == artistId;
                        bool sameExplicitValue = (resItem?["explicit"]?.GetValue<bool>() ?? false) == explicitBadge;

                        if (validTitle && validArtId && sameExplicitValue)
                        {
                            return resItem?.DeepClone() as JsonObject;
                        }
                    }

                    return (JsonObject?)S?.DeepClone() ?? null;
                });

                JsonObject?[] completedResults = await Task.WhenAll(searchTasks);

                JsonArray filteredTracks = new JsonArray();
                foreach (var track in completedResults)
                {
                    if (track != null)
                    {
                        filteredTracks.Add(track);
                    }
                }

                data!["items"] = filteredTracks;
            }

            if (data != null)
            {
                yt.DB_Insert(browseId, data, DB_filter.ALBUM);

                return data;
            }

            return [];

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
