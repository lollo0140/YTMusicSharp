using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtility;
using YoutubeMusic;



string path = "D:\\COSE PRODUTTIVE\\CODING\\repos\\repos\\.NET\\LOLLOMUSICX";
JsonNode headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new YTMusicSharp(
    workspacePath: path,
    youtubHeaders: (JsonObject)headers
);






System.Console.WriteLine(await ytClient.GetYTAudioById("ijpFh42YQ3w"));
