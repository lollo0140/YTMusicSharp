using System.Text.Json;
using System.Text.Json.Nodes;

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

    public class YTmusicApi
    {

        readonly string workspacePath;


        private readonly string headersPath;
        private readonly JsonObject? youtubHeaders;


        //endpoints: -----------------
        public Account AccountEndpoint { get; }
        public Search SearchEndpoint { get; }
        public Browse BrowseEndpoint { get; }
        public Library LibraryEndpoint { get; }

        //initialization
        public YTmusicApi(string workspacePath, JsonObject? youtubHeaders = null)
        {
            this.workspacePath = Path.Combine(workspacePath, "musicSharpData");
            this.headersPath = Path.Combine(this.workspacePath, "headers.json");

            Directory.CreateDirectory(this.workspacePath);
            Directory.CreateDirectory(Path.Combine(this.workspacePath, "cache"));

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

            this.AccountEndpoint = new Account(this.youtubHeaders!);
            this.SearchEndpoint = new Search(this.youtubHeaders!);
            this.BrowseEndpoint = new Browse(this.youtubHeaders!);
            this.LibraryEndpoint = new Library(this.youtubHeaders!);
            ytWriteLine("api ready to use");
        }

        private void ytWriteLine(string content)
        {
            System.Console.WriteLine("yt log---------------------");
            System.Console.WriteLine(content);
            System.Console.WriteLine("---------------------------");
        }



        public class Account
        {
            private readonly JsonObject? youtubHeaders;
            internal Account(JsonObject headers)
            {
                youtubHeaders = headers;
            }

            public async Task<JsonObject> GetLoggedUser()
            {
                string path = Path.Join("./ytdata/cookies.json");

                if (youtubHeaders != null)
                {
                    JsonObject? C = youtubHeaders;
                    JsonObject result = await Requester.PostRequest(endpointUrl: "account/account_menu", cookies: C, payload: new JsonObject());

                    JsonNode loggedUser = new JsonObject();

                    var infos = result["actions"]?[0]?["openPopupAction"]?["popup"]?["multiPageMenuRenderer"]?["header"]?["activeAccountHeaderRenderer"];


                    loggedUser["name"] = infos?["accountName"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? string.Empty;
                    loggedUser["username"] = infos?["channelHandle"]?["runs"]?[0]?["text"]?.GetValue<string>() ?? string.Empty;
                    loggedUser["imgUrl"] = infos?["accountPhoto"]?["thumbnails"]?[0]?["url"]?.GetValue<string>() ?? string.Empty;
                    loggedUser["logged"] = true;

                    return (JsonObject)loggedUser;

                }
                else
                {
                    var res = new JsonObject();

                    res["logged"] = false;

                    return res;
                }


            }
        }

        public class Search
        {
            private JsonObject? youtubHeaders;
            internal Search(JsonObject headers)
            {
                youtubHeaders = headers;
            }

            public async Task<JsonArray> GetSearchSugg(string input)
            {
                return await YTsearch.GetSearchSuggestions(input, youtubHeaders);
            }

            public async Task<JsonObject> GenericSearch(string query)
            {
                return await YTsearch.Search(query, youtubHeaders);
            }

            public async Task<JsonObject> SpecificSearch(string query, ContentType contentType)
            {

                if ( contentType == ContentType.Text)
                {
                    return [];
                }

                return await YTsearch.SpecificSearch(query, youtubHeaders, contentType: contentType);

            }


        }

        public class Browse
        {
            private JsonObject? youtubHeaders;
            internal Browse(JsonObject headers)
            {
                youtubHeaders = headers;
            }

            public async Task<JsonObject> FetchAlbumData(string browseId)
            {

                return await Album.FetchAlbumData( browseId, youtubHeaders);

            }

            public async Task<JsonObject> FetchPlaylistData(string browseId)
            {
                return await Playlist.FetchPlaylistData( browseId, youtubHeaders);
            }

            public async Task<JsonObject> FetchArtistPage(string browseId)
            {
                return await ArtistPage.FetchArtistPage(browseId, youtubHeaders);
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

        public class Library
        {
            private JsonObject? youtubHeaders;
            internal Library(JsonObject headers)
            {
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
                return await LibraryData.GetUserContentByFilter( youtubHeaders, filter);
            }

        }

    }


}
