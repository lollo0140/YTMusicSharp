using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

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
        private YoutubeDL? videoGetter = null;
        private readonly JsonObject? youtubHeaders = [];


        //endpoints: -----------------
        public Account AccountEndpoint { get; }
        public Search SearchEndpoint { get; }
        public Browse BrowseEndpoint { get; }
        public Library LibraryEndpoint { get; }
        public Interactions InteractionsEndpoint { get; }


        //initialization
        public YTMusicSharp(JsonObject? youtubHeaders = null)
        {
            if (youtubHeaders != null)
            {
                this.youtubHeaders = youtubHeaders;
            }

            this.AccountEndpoint = new Account(this.youtubHeaders!, this);
            this.SearchEndpoint = new Search(this.youtubHeaders!, this);
            this.BrowseEndpoint = new Browse(this.youtubHeaders!, this);
            this.LibraryEndpoint = new Library(this.youtubHeaders!, this);
            this.InteractionsEndpoint = new Interactions(this.youtubHeaders!, this);

            YtWriteLine("api ready to use");
        }

        public async Task<bool> IsUserLogged()
        {
            var _ = await AccountEndpoint.GetLoggedUser();
            return _?["logged"]?.GetValue<bool>() ?? false;
        }

        private static void YtWriteLine(string content)
        {
            System.Console.WriteLine("yt log---------------------");
            System.Console.WriteLine(content);
            System.Console.WriteLine("---------------------------");
        }


        public async Task<JsonObject> GetLyrics(
            string name,
            string artist,
            string? album = null,
            int? duration = null
        )
        {

            var queryParams = new Dictionary<string, string>
            {
                { "track_name", name },
                { "artist_name", artist },
            };

            if (album != null)
            {
                queryParams.Add("album_name", album);
            }

            if (duration != null)
            {
                queryParams.Add("duration", duration.ToString()!);
            }


            using var content = new FormUrlEncodedContent(queryParams);
            string queryString = await content.ReadAsStringAsync();
            string completeQuery = $"https://lrclib.net/api/get?{queryString}";

            var res = await new HttpClient().GetAsync(completeQuery);

            JsonObject? rBody = (JsonObject?)JsonNode.Parse(await res.Content.ReadAsStringAsync());

            if (rBody == null) return [];

            string syncLyrics = rBody?["syncedLyrics"]?.GetValue<string>() ?? "";


            List<string> sliced = [.. syncLyrics.Split("\n")];

            JsonArray formatted = [];

            sliced.ForEach(x =>
            {

                int start = 1;
                int end = x.IndexOf(']', start);

                string timestamp = x.Substring(start, end - start);

                string content = x[(end + 2)..];


                if (TimeSpan.TryParseExact(timestamp, @"mm\:ss\.ff", CultureInfo.InvariantCulture, out TimeSpan timeSpan))
                {
                    double totalSeconds = timeSpan.TotalSeconds; // Es: 9.35

                    JsonObject node = new()
                    {
                        ["timestamp"] = timestamp,
                        ["seconds"] = (int)totalSeconds,
                        ["content"] = content
                    };
                }
            });


            JsonObject finalResult = new()
            {
                ["plainLyrics"] = rBody?["plainLyrics"]?.GetValue<string>() ?? "",
                ["syncedLyrics"] = formatted
            };

            return finalResult;
        }

        public async Task DownloadVideoById(string id, string path)
        {
            var videoUrl = $"https://youtube.com/watch?v={id}";

            if (videoGetter == null)
            {
                videoGetter ??= new YoutubeDL
                {
                    YoutubeDLPath = "yt-dlp"
                };

                await Utils.DownloadYtDlp();

            }

            var options = new OptionSet
            {
                Format = "bestaudio[ext=webm]/bestaudio",
                Output = path
            };

            var progress = new Progress<DownloadProgress>(p =>
            {
                Console.WriteLine($"[yt-dlp] {p.Progress * 100:F1}% - Velocità: {p.DownloadSpeed}");
            });

            var result = await videoGetter.RunVideoDownload(
                videoUrl,
                progress: progress,
                overrideOptions: options
            );
        }

    }
}
