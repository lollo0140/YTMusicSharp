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
