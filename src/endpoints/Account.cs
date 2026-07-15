using System.Text.Json.Nodes;

namespace YoutubeMusic
{
    public class Account
    {
        private readonly JsonObject? youtubHeaders;
        private readonly YTMusicSharp yt;
        internal Account(JsonObject headers, YTMusicSharp yT)
        {
            yt = yT;
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

}
