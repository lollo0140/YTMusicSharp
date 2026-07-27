using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeMusic
{
    public enum ContentType
    {
        Track,
        Video,
        Album,
        Artist,
        Playlist,
        Channel,
        Podcast,
        All,
        Text
    }

    public enum ContentFilter
    {
        Albums,
        Playlists,
        Artists,
        Subscribed,
        Podcasts
    }

    public enum LikeStatus
    {
        LIKE,
        DISLIKE,
        NEUTRAL
    }

    public enum DB_filter
    {
        ALBUM,
        PLAYLIST,
        ARTIST,
        LIBRARY,
        CACHEDSONG,
        DOWNLOADED
    }



    public class YTMusicSharp
    {
        private YoutubeClient? videoGetter = null;

        readonly string workspacePath;

        private readonly string headersPath;
        private readonly JsonObject? youtubHeaders;



        //endpoints: -----------------
        public Account AccountEndpoint { get; }
        public Search SearchEndpoint { get; }
        public Browse BrowseEndpoint { get; }
        public Library LibraryEndpoint { get; }
        public Interactions InteractionsEndpoint { get; }


        //initialization
        public YTMusicSharp(string workspacePath, JsonObject? youtubHeaders = null)
        {
            this.workspacePath = Path.Combine(workspacePath, "musicSharpData");
            this.headersPath = Path.Combine(this.workspacePath, "headers.json");

            Directory.CreateDirectory(this.workspacePath);

            var cachePath = Path.Combine(this.workspacePath, "cache");


            Directory.CreateDirectory(cachePath);
            Directory.CreateDirectory(Path.Combine(cachePath, "cachedvideos"));

            List<string> cachePaths = [];

            cachePaths.Add(Path.Combine(this.workspacePath, "cache", "albums.json"));
            cachePaths.Add(Path.Combine(this.workspacePath, "cache", "playlists.json"));
            cachePaths.Add(Path.Combine(this.workspacePath, "cache", "artists.json"));
            cachePaths.Add(Path.Combine(this.workspacePath, "cache", "library.json"));
            cachePaths.Add(Path.Combine(this.workspacePath, "cache", "cachedSongs.json"));
            cachePaths.Add(Path.Combine(this.workspacePath, "cache", "downloaded.json"));


            cachePaths.ForEach(P =>
            {
                if (!File.Exists(P))
                {
                    File.WriteAllText(P, "{}");
                }
            });



            if (File.Exists(headersPath))
            {
                this.youtubHeaders = (JsonObject)JsonNode.Parse(File.ReadAllText(headersPath))!;
                ytWriteLine("api initialized with existing headers");
            }
            else if (youtubHeaders != null)
            {
                this.youtubHeaders = youtubHeaders;
                File.WriteAllText(headersPath, this.youtubHeaders.ToJsonString());
                ytWriteLine("api initialized with new headers and saved to disk");
            }
            else
            {
                ytWriteLine("no headers found, limited functionality");
            }

            this.AccountEndpoint = new Account(this.youtubHeaders!, this);
            this.SearchEndpoint = new Search(this.youtubHeaders!, this);
            this.BrowseEndpoint = new Browse(this.youtubHeaders!, this);
            this.LibraryEndpoint = new Library(this.youtubHeaders!, this);
            this.InteractionsEndpoint = new Interactions(this.youtubHeaders!, this);





            ytWriteLine("api ready to use");
        }

        private void ytWriteLine(string content)
        {
            System.Console.WriteLine("yt log---------------------");
            System.Console.WriteLine(content);
            System.Console.WriteLine("---------------------------");
        }




        public JsonObject? GetFromLocalDB(DB_filter type, string id)
        {
            var DB_content = GetSavedData(type);

            if (DB_content != null && DB_content.ContainsKey(id))
            {
                return (JsonObject?)DB_content?[id] ?? [];
            }

            return [];

        }



        internal string? GetDBPath(DB_filter type)
        {
            string? p = null;

            switch (type)
            {
                case DB_filter.ALBUM:
                    p = Path.Combine(workspacePath, "cache", "albums.json");
                    break;
                case DB_filter.PLAYLIST:
                    p = Path.Combine(workspacePath, "cache", "playlists.json");
                    break;
                case DB_filter.ARTIST:
                    p = Path.Combine(workspacePath, "cache", "artists.json");
                    break;
                case DB_filter.LIBRARY:
                    p = Path.Combine(workspacePath, "cache", "library.json");
                    break;
                case DB_filter.CACHEDSONG:
                    p = Path.Combine(workspacePath, "cache", "cachedSongs.json");
                    break;
                case DB_filter.DOWNLOADED:
                    p = Path.Combine(workspacePath, "cache", "downloaded.json");
                    break;
            }

            return p;
        }
        internal void WriteJsonData(DB_filter type, JsonObject data)
        {

            string? path = GetDBPath(type);

            if (path != null)
            {
                File.WriteAllText(path, JsonSerializer.Serialize(data));
            }

        }

        internal JsonObject? GetSavedData(DB_filter type)
        {
            string? p = GetDBPath(type);


            if (p != null)
            {
                return (JsonObject?)JsonNode.Parse(File.ReadAllText(p) ?? "{}");
            }

            return [];

        }

        internal JsonObject DB_Get(string id, DB_filter filter)
        {

            JsonObject? DB_data = GetSavedData(filter);


            if (DB_data != null && DB_data.ContainsKey(id))
            {

                return (JsonObject?)DB_data?[id] ?? new JsonObject();

            }

            return [];

        }

        internal void DB_Insert(string id, JsonObject data, DB_filter filter)
        {

            JsonObject? DB_data = GetSavedData(filter);

            if (DB_data != null)
            {
                System.Console.WriteLine("writing " + id);
                DB_data[id] = data;
                WriteJsonData(filter, DB_data);
            }

        }




        public bool IsVideoCached(string id)
        {
            string path = Path.Combine(this.workspacePath, "cache", "cachedvideos", $"{id}.webm");

            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }

        public async Task<string> GetYTAudioById(string id)
        {
            if (IsVideoCached(id))
            {
                return Path.Combine(this.workspacePath, "cache", "cachedvideos", $"{id}.webm");
            }

            if (videoGetter == null)
            {
                videoGetter = new YoutubeClient();
            }

            var videoUrl = $"https://youtube.com/watch?v={id}";
            var streamManifest = await videoGetter.Videos.Streams.GetManifestAsync(videoUrl);

            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            string extension = "webm";

            var stream = await videoGetter.Videos.Streams.GetAsync(streamInfo);


            if (this.workspacePath != null)
            {
                string destinationPath = Path.Combine(this.workspacePath, "cache", "cachedvideos", $"{id}.{extension}") ?? "";

                using (FileStream fs = File.Create(destinationPath))
                {
                    await stream.CopyToAsync(fs);
                }

                Path.Combine(this.workspacePath, "cache", "cachedvideos", $"{id}.webm");

                return destinationPath;
            }
            else
            {
                System.Console.WriteLine("Please specify a workspace");
            }

            return "";
        }

    }


}
