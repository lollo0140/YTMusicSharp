# YTMusicSharp

A C# library for interacting with the YouTube Music API. This project provides a simple and efficient way to fetch home sections, search for music, manage playlists, and more.

## Features

- **Home Data**: Fetch personalized home sections and recommendations.
- **Search**: Search for songs, albums, artists, and playlists.
- **Library Management**: Access and manage your music library.
- **Content Parsing**: Detailed parsing for Albums, Artists, and Playlists.
- **Continuation Support**: Handle paginated results seamlessly.

## Getting Started

### Prerequisites

- .NET 10.0 or higher.
- A valid `cookies.json` for authentication (optional but recommended for personalized data).

### Usage

```csharp
using YoutubeMusic;

string workspacePath = "your/workspace/path";
JsonObject headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTmusicApi ytClient = new YTmusicApi(
    workspacePath: workspacePath,
    youtubHeaders: headers
);

// Fetch home sections
JsonObject homeData = await ytClient.BrowseEndpoint.FetchHomeSections();
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.
