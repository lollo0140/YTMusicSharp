using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using DebugUtility;

namespace YoutubeMusic
{

    internal class Playlist
    {

        internal static async Task<JsonObject> FetchUserLikePlaylist(JsonObject? headers)
        {
            return await FetchPlaylistData("VLLM", headers);
        }

        internal static async Task<JsonObject> FetchUserEpisodePlaylist(JsonObject? headers)
        {
            return await FetchPlaylistData("VLSE", headers);
        }



        internal static async Task<JsonObject> FetchPlaylistData(string BrowseId, JsonObject? headers)
        {
            JsonObject? C;

            if (headers != null)
            {
                C = headers;
            }
            else
            {
                C = new JsonObject();
            }

            var payload = new JsonObject();

            payload["browseId"] = BrowseId;
            JsonObject result = await Requester.PostRequest(endpointUrl: "browse", cookies: C, payload: payload);





            JsonObject parsedPlaylist = new JsonObject();

            if (result != null)
            {
                parsedPlaylist["data"] = ParsePlaylistMetaData(result, BrowseId);
            }

            //PARSING elements ---------------------------------

            JsonArray musicResponsiveListItemRenderers = [];

            JsonArray? items = (JsonArray?)result?["contents"]?["twoColumnBrowseResultsRenderer"]?["secondaryContents"]?["sectionListRenderer"]?["contents"]?[0]?["musicPlaylistShelfRenderer"]?["contents"];


            if (items != null)
            {

                string? CT = FindContinuationToken(items);

                if (CT != null)
                {
                    for (int i = 0; i < items?.Count - 1; i++)
                    {
                        musicResponsiveListItemRenderers.Add(items?[i]?.DeepClone());
                    }
                }
                else
                {
                    for (int i = 0; i < items?.Count; i++)
                    {
                        musicResponsiveListItemRenderers.Add(items?[i]?.DeepClone());
                    }
                }


                while (CT != null)
                {
                    // System.Console.WriteLine(CT);

                    JsonArray? newTraks = await BrowseContinuation(CT, headers);

                    if (newTraks != null && newTraks.Count > 1)
                    {
                        CT = FindContinuationToken(newTraks);

                        if (CT != null)
                        {
                            for (int i = 0; i < newTraks?.Count - 1; i++)
                            {
                                musicResponsiveListItemRenderers.Add(newTraks?[i]?.DeepClone());
                            }
                        }
                        else
                        {
                            for (int i = 0; i < newTraks?.Count; i++)
                            {
                                musicResponsiveListItemRenderers.Add(newTraks?[i]?.DeepClone());
                            }
                        }
                    }
                    else
                    {
                        CT = null;
                    }
                }


            }

            JsonArray parsedTraks = [];

            foreach (JsonObject? item in musicResponsiveListItemRenderers)
            {

                var track = (JsonObject?)item?["musicResponsiveListItemRenderer"];


                if (track != null)
                {
                    parsedTraks.Add(Parsing.ParseMusicResponsiveListItemRenderer(track));
                }

            }

            parsedPlaylist["items"] = parsedTraks;

            return parsedPlaylist;

        }

        private static JsonObject ParsePlaylistMetaData(JsonObject? data, string BrowseId)
        {

            JsonObject ParsedData = new JsonObject();

            string PLId = BrowseId.StartsWith("VL") ? BrowseId.Substring(2) : BrowseId;


            if (data != null)
            {


                JsonObject? headerData = (JsonObject?)data["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]?[0]?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0];


                JsonObject? PlaylistData = (JsonObject?)headerData?["musicEditablePlaylistDetailHeaderRenderer"]?["header"]?["musicResponsiveHeaderRenderer"] ?? (JsonObject?)headerData?["musicResponsiveHeaderRenderer"];


                JsonObject? editFormNode = (JsonObject?)headerData?["musicEditablePlaylistDetailHeaderRenderer"]?["editHeader"]?["musicPlaylistEditHeaderRenderer"];


                //privacy status --------------------


                ParsedData["privacyStatus"] = editFormNode?["privacy"]?.GetValue<string>() ?? "PUBLIC";


                //privacy status --------------------



                //Thumbnails -------------
                JsonArray Thumbnails = [];

                foreach (JsonObject? T in (PlaylistData?["thumbnail"]?["musicThumbnailRenderer"]?["thumbnail"]?["thumbnails"]?.AsArray() ?? []).Cast<JsonObject?>())
                {
                    if (T?["url"] != null)
                    {
                        Thumbnails.Add(T["url"]?.GetValue<string>());
                    }
                }

                ParsedData["thumbnails"] = Thumbnails;
                //Thumbnails -------------



                //buttons ----------------

                JsonArray? buttons = (JsonArray?)PlaylistData?["buttons"];

                ParsedData["canEdit"] = false;
                ParsedData["canDelete"] = false;

                foreach (JsonObject? B in (buttons ?? []).Cast<JsonObject?>())
                {

                    if (B != null)
                    {
                        if (B.ContainsKey("buttonRenderer"))
                        {
                            JsonObject? BaseButtonNode = (JsonObject?)B["buttonRenderer"];

                            ParsedData["canEdit"] = (BaseButtonNode?["targetId"]?.GetValue<string>() ?? "") == "music-edit-playlist-button";
                            ParsedData["saved"] = ParsedData["canEdit"]?.GetValue<bool>() ?? false;

                        }
                        else if (B.ContainsKey("menuRenderer"))
                        {

                            //Menu items -----------------

                            JsonArray? menuItems = (JsonArray?)B?["menuRenderer"]?["items"];


                            foreach (JsonObject menuItem in (menuItems ?? []).Cast<JsonObject>())
                            {
                                string? iconType = menuItem?["menuNavigationItemRenderer"]?["icon"]?["iconType"]?.GetValue<string>() ?? null;

                                // System.Console.WriteLine(iconType);

                                if (iconType != null && iconType == "DELETE")
                                {

                                    ParsedData["canDelete"] = true;
                                    break;

                                }
                                else
                                {
                                    ParsedData["canDelete"] = false;
                                }

                            }

                            //Menu items -----------------

                        }
                        else if (B.ContainsKey("toggleButtonRenderer"))
                        {
                            ParsedData["saved"] = B?["toggleButtonRenderer"]?["isToggled"]?.GetValue<bool>() ?? false;
                        }

                    }




                }


                //buttons ----------------


                //title --------------------------
                ParsedData["title"] = PlaylistData?["title"]?["runs"]?[0]?["text"]?.GetValue<string>();
                //title --------------------------


                //subtitle --------------------------

                JsonArray? subtitleComponents = (JsonArray?)PlaylistData?["subtitle"]?["runs"];
                string subtitle = "";

                foreach (JsonObject? c in (subtitleComponents ?? []).Cast<JsonObject?>())
                {
                    subtitle += c?["text"]?.GetValue<string>() ?? "";
                }

                ParsedData["subtitle"] = subtitle;


                JsonArray? secondSubtitleComponents = (JsonArray?)PlaylistData?["secondSubtitle"]?["runs"];
                string secondSubtitle = "";

                foreach (JsonObject? c in (secondSubtitleComponents ?? []).Cast<JsonObject?>())
                {
                    secondSubtitle += c?["text"]?.GetValue<string>() ?? "";
                }

                ParsedData["secondSubtitle"] = secondSubtitle;

                //subtitle --------------------------




                //description --------------------------

                ParsedData["description"] = PlaylistData?["description"]?["musicDescriptionShelfRenderer"]?["description"]?["runs"]?[0]?["text"]?.GetValue<string>();

                //description --------------------------


                //facepile -----------------------------

                JsonObject? facepileNode = (JsonObject?)PlaylistData?["facepile"]?["avatarStackViewModel"];
                JsonArray? faepiles = (JsonArray?)facepileNode?["avatars"];

                JsonObject parsedFacePiles = new JsonObject();


                JsonArray photosUrl = [];

                foreach (JsonObject profiles in (faepiles ?? []).Cast<JsonObject>())
                {
                    photosUrl.Add(profiles?["avatarViewModel"]?["image"]?["sources"]?[0]?["url"]?.GetValue<string>());
                }

                parsedFacePiles["profileIcons"] = photosUrl;
                parsedFacePiles["text"] = facepileNode?["text"]?["content"]?.GetValue<string>() ?? null;

                ParsedData["facepile"] = parsedFacePiles;

                //facepile -----------------------------

                //share link ---------------------------

                ParsedData["shareLink"] = "https://music.youtube.com/playlist?list=" + PLId;

                ParsedData["playlistId"] = PLId;

                //share link ---------------------------




            }

            return ParsedData;

        }

        private static string? FindContinuationToken(JsonArray? traks)
        {
            string? token = null;


            if (traks != null)
            {

                JsonObject? lastElement = (JsonObject?)traks[traks.Count - 1];

                if (lastElement != null && lastElement.ContainsKey("continuationItemRenderer"))
                {
                    token = lastElement?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.GetValue<string>() ?? null;

                    return token;

                }

            }
            else
            {
                return null;
            }


            return null;
        }

        private static async Task<JsonArray?> BrowseContinuation(string continuation, JsonObject? cookies)
        {

            JsonObject continuationResult = await Continuation.BrowseContinuation(continuation, cookies);

            JsonArray? continuationContent = (JsonArray?)continuationResult?["onResponseReceivedActions"]?[0]?["appendContinuationItemsAction"]?["continuationItems"];

            return continuationContent;

        }

    }

}
