using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtility;
using YoutubeMusic;

Console.Clear();
JsonNode headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new((JsonObject)headers);


var a = await ytClient.GetLyrics("c section", "lucy bedroque");
