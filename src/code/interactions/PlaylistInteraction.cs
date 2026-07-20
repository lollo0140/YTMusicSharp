using System.Text.Json.Nodes;
using DebugUtility;

namespace YoutubeMusic;

class PlaylistInteraction
{


    internal static async Task EditPlaylist(string playlistId, JsonObject? headers,
    string? title = null,
    string? description = null,
    PrivacyStatus? privacyStatus = null)
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
        JsonArray actions = [];


        payload["playlistId"] = playlistId;



        if (title != null)
        {
            JsonObject nameAction = [];

            nameAction["action"] = "ACTION_SET_PLAYLIST_NAME";
            nameAction["playlistName"] = title;

            actions.Add(nameAction);
        }

        if (description != null)
        {
            JsonObject descAction = [];

            descAction["action"] = "ACTION_SET_PLAYLIST_DESCRIPTION";
            descAction["playlistDescription"] = description;

            actions.Add(descAction);
        }

        if (privacyStatus != null)
        {
            JsonObject descAction = [];

            string privState;

            switch (privacyStatus)
            {
                case PrivacyStatus.UNLISTED:
                    privState = "UNLISTED";
                    break;

                case PrivacyStatus.PUBLIC:
                    privState = "PUBLIC";
                    break;

                case PrivacyStatus.PRIVATE:
                    privState = "PRIVATE";
                    break;

                default:
                    privState = "UNLISTED";
                    break;
            }

            descAction["action"] = "ACTION_SET_PLAYLIST_PRIVACY";
            descAction["playlistPrivacy"] = privState;

            actions.Add(descAction);

        }

        actions.Add(new JsonObject
        {
            ["action"] = "ACTION_SET_ALLOW_ITEM_VOTE",
            ["itemVotePermission"] = 3
        });


        payload["actions"] = actions;


        await Requester.PostRequest(endpointUrl: "browse/edit_playlist", cookies: C, payload: payload);

    }


    internal static async Task DeletePlaylist(string playlistId, JsonObject? headers)
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

        payload["playlistId"] = playlistId;


        await Requester.PostRequest(endpointUrl: "playlist/delete", cookies: C, payload: payload);

    }

    internal static async Task CreatePlaylist(JsonObject? headers,
    string? title = null,
    string? description = null,
    PrivacyStatus? privacyStatus = null)
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

        var payload = new JsonObject
        {
            ["params"] = "KAA%3D"
        };




        if (privacyStatus != null)
        {
            payload["privacyStatus"] = privacyStatus.ToString();
        }
        else
        {
            payload["privacyStatus"] = "UNLISTED";
        }

        if (title != null)
        {
            payload["title"] = title;
        }
        else
        {
            payload["title"] = "YoutubeMusic Playlist";
        }

        if (description != null)
        {
            payload["description"] = description;
        }
        else
        {
            payload["description"] = "Playlist created using YTMusicSharp library";
        }

        await Requester.PostRequest(endpointUrl: "playlist/create", cookies: C, payload: payload);
    }

    internal static async Task SetPlaylistCollaborators(string playlistId, bool collaborate, JsonObject? headers)
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




        JsonArray Ac = [];


        if (collaborate)
        {
            Ac = new JsonArray
            {
                new JsonObject{
                    ["action"] = "ACTION_CREATE_COLLABORATION_INVITE_LINK"
                }
            };
        }
        else
        {
            Ac = new JsonArray
            {
                new JsonObject{
                    ["action"] = "ACTION_SET_CLOSED_TO_CONTRIBUTIONS",
                    ["closedToContributions"] = true
                }
            };
        }


        var payload = new JsonObject
        {
            ["playlistId"] = playlistId,
            ["actions"] = Ac
        };


        await Requester.PostRequest(endpointUrl: "playlist/delete", cookies: C, payload: payload);

    }

}
