using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    internal class Requester
    {

        public static string? HL = null;
        public static string? GL = null;

        internal static void SetLanguage(string? hl, string? gl)
        {

            if (hl != null)
            {
                HL = hl;
            }

            if (gl != null)
            {
                GL = gl;
            }

        }

        internal static JsonNode GetRequestContext()
        {
            JsonNode context = new JsonObject();
            JsonNode client = new JsonObject();



            if (HL != null)
            {
                client["hl"] = HL;
            }

            if (GL != null)
            {
                client["gl"] = GL;
            }



            client["clientName"] = "WEB_REMIX";
            client["clientVersion"] = "1.20240313.01.00";
            context["client"] = client;

            return context;

        }

        private static string ParseCookieString(JsonObject cookies)
        {
            return string.Join("; ", cookies.Select(item => $"{item.Key}={item.Value}"));
        }

        private static string GenHash(string sapisid)
        {

            var timeStamp = System.Math.Floor((double)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            const string origin = "https://music.youtube.com";


            string data = $"{timeStamp} {sapisid} {origin}";

            var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(data))).ToLower();

            return $"{timeStamp}_{hash}";

        }

        internal static JsonNode GenerateHeaders(JsonObject cookies)
        {

            string sapisid = cookies?["SAPISID"]?.GetValue<string>() ?? string.Empty;

            string cookieString = ParseCookieString(cookies!);
            string hashData = GenHash(sapisid);
            const string origin = "https://music.youtube.com";

            JsonNode requestheaders = new JsonObject();

            requestheaders["Cookie"] = cookieString;
            requestheaders["Authorization"] = $"SAPISIDHASH {hashData}";
            requestheaders["Content-Type"] = "application/json";
            requestheaders["X-Goog-AuthUser"] = "0";
            requestheaders["x-origin"] = origin;
            requestheaders["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

            return requestheaders;


        }

        internal static async Task<JsonObject> PostRequest(string endpointUrl, JsonObject cookies, JsonNode payload, bool noAuth = false)
        {
            using var client = new HttpClient();
            var headers = GenerateHeaders(cookies);


            payload["context"] = GetRequestContext();



            string URL = "https://music.youtube.com/youtubei/v1/";

            var request = new HttpRequestMessage(HttpMethod.Post, URL + endpointUrl);
            request.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");

            if (!noAuth)
            {
                
                foreach (var header in headers.AsObject())
                {
                    if (header.Key == "Content-Type") continue;

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString());
                }
            }



            try
            {
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (JsonObject)JsonNode.Parse(result)!;
                }
                else
                {
                    return new JsonObject();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione: {ex.Message}");

                return new JsonObject();
            }

        }

        internal static async Task<JsonObject> GetRequest(string endpointUrl, JsonObject cookies, JsonNode payload)
        {
            using var client = new HttpClient();
            var headers = GenerateHeaders(cookies);


            payload["context"] = GetRequestContext();



            string URL = "https://music.youtube.com/youtubei/v1/";

            var request = new HttpRequestMessage(HttpMethod.Get, URL + endpointUrl);
            request.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");



            foreach (var header in headers.AsObject())
            {
                // Alcuni header vanno nel contenuto, altri nel messaggio
                if (header.Key == "Content-Type") continue;

                request.Headers.TryAddWithoutValidation(header.Key, header.Value?.ToString());
            }



            try
            {
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (JsonObject)JsonNode.Parse(result)!;
                }
                else
                {
                    return new JsonObject();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione: {ex.Message}");

                return new JsonObject();
            }
        }


    }
}
