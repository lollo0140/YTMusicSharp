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

            string name = data?["data"]?["title"]?.GetValue<string>() ?? "";
            JsonArray thumbnails = [data?["data"]?["thumbnails"]?[0]?.GetValue<string>(), data?["data"]?["thumbnails"]?[1]?.GetValue<string>()];

            JsonArray artists = [data?["data"]?["artist"]];


            foreach (JsonNode? item in (JsonArray?)data?["items"] ?? [])
            {
                if (item != null)
                {
                    item["album"] = new JsonObject
                    {
                        ["albumId"] = browseId,
                        ["titleName"] = name
                    };
                    item["thumbnails"] = thumbnails.DeepClone();

                    if (item?["artists"] == null)
                    {
                        item?["artists"] = artists.DeepClone();
                    }
                }

            }

            if (data != null)
            {
                if (data.ContainsKey("data") && data.ContainsKey("items"))
                {
                    yt.DB_Insert(browseId, data, DB_filter.ALBUM);
                    return data;
                }
            }
            return [];
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


            string name = data?["data"]?["title"]?.GetValue<string>() ?? "";
            JsonArray thumbnails = [data?["data"]?["thumbnails"]?[0]?.GetValue<string>(), data?["data"]?["thumbnails"]?[1]?.GetValue<string>()];


            foreach (JsonNode? item in ((JsonArray?)data?["items"] ?? []))
            {

                if (item != null)
                {
                    item["album"] = new JsonObject
                    {
                        ["albumId"] = browseId,
                        ["titleName"] = name
                    };
                    item["thumbnails"] = thumbnails.DeepClone();
                }

            }



            if (data != null)
            {
                if (data.ContainsKey("data") && data.ContainsKey("items"))
                {
                    yt.DB_Insert(browseId, data, DB_filter.ALBUM);
                    return data;
                }
            }

            return [];

        }


        public async Task<JsonObject> FetchPlaylistData(string browseId)
        {
            JsonObject data = await Playlist.FetchPlaylistData(browseId, youtubHeaders);


            if (data != null)
            {
                if (data.ContainsKey("data") && data.ContainsKey("items"))
                {
                    yt.DB_Insert(browseId, data, DB_filter.PLAYLIST);
                    return data;
                }
            }

            return [];
        }

        public async Task<JsonObject> FetchArtistPage(string browseId)
        {
            JsonObject data = await ArtistPage.FetchArtistPage(browseId, youtubHeaders);


            if (data != null)
            {
                if (data.ContainsKey("header") && data.ContainsKey("sections"))
                {
                    yt.DB_Insert(browseId, data, DB_filter.ARTIST);
                    return data;
                }
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
