using System.Text.Json;
using System.Text.Json.Nodes;
using DebugUtility;
using YoutubeMusic;

Console.Clear();
JsonNode headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new((JsonObject)headers);


var A = await ytClient.BrowseEndpoint.FetchAlbumData("MPREb_iyLdAGYsOUR", true);

foreach (var item in A["items"]!.AsArray())
{
    System.Console.WriteLine(item["title"]);
    System.Console.WriteLine(item["album"]!["titleName"]);
    System.Console.WriteLine(item["artists"]);
    System.Console.WriteLine(item["type"]);
    System.Console.WriteLine(item["id"]);

    System.Console.WriteLine("\n\n\n");

}
