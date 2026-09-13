using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    public class Search
    {
        private readonly JsonObject? youtubHeaders;
        private readonly YTMusicSharp yt;
        internal Search(JsonObject headers, YTMusicSharp yT)
        {
            yt = yT;
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

            if (contentType == ContentType.Text)
            {
                return [];
            }
            return await YTsearch.SpecificSearch(query, youtubHeaders, contentType: contentType);

        }

        public async Task<JsonObject> IncognitoSpecificSearch(string query, ContentType contentType)
        {
            if (contentType == ContentType.Text)
            {
                return [];
            }
            return await YTsearch.SpecificIncognitoSearch(query, contentType);
        }

        public async Task<JsonObject?> SearchMatchingTrack(string title,
        string firstArtistName,
        string albumName = "",
        bool isExplicit = true)
        {
            return await YTsearch.SearchMatchingTrack(title, firstArtistName, albumName, isExplicit);
        }

    }
}
