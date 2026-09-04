using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtility;
using YoutubeMusic;



string path = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "LOLLOMUSICX"
);
JsonNode headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new((JsonObject)headers);


await ytClient.DownloadVideoById("xdQIlVFqwVs", "./");
