using System.Text.Json;
using System.Text.Json.Nodes;
using YoutubeMusic;



string path = "D:\\COSE PRODUTTIVE\\CODING\\repos\\repos\\.NET\\LOLLOMUSICX";
JsonNode headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new YTMusicSharp(
    workspacePath: path,
    youtubHeaders: (JsonObject)headers
);





JsonObject lib = await ytClient.BrowseEndpoint.FetchHomeSections();

string serializeContent = JsonSerializer.Serialize(lib);
File.WriteAllText("./test.json", serializeContent);
