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
            string parsedString = input.Replace(" ", "-");
            return await YTsearch.GetSearchSuggestions(parsedString, youtubHeaders);
        }

        public async Task<JsonObject> GenericSearch(string query)
        {
            string parsedString = query.Replace(" ", "-");
            return await YTsearch.Search(parsedString, youtubHeaders);
        }

        public async Task<JsonObject> SpecificSearch(string query, ContentType contentType)
        {

            if (contentType == ContentType.Text)
            {
                return [];
            }
            string parsedString = query.Replace(" ", "-");
            return await YTsearch.SpecificSearch(parsedString, youtubHeaders, contentType: contentType);

        }


    }
}
