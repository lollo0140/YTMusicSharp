using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtility;
using YoutubeMusic;



string path = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "LOLLOMUSICX"
);
JsonNode headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new YTMusicSharp(
    workspacePath: path,
    youtubHeaders: (JsonObject)headers
);


await ytClient.InteractionsEndpoint.RemoveVideoFromPlaylist("8rIa8GMkex4", "289F4A46DF0A30D2", "VLPLe6IBg_InZhI");
