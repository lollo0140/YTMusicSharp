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

JsonObject headers = JsonNode.Parse(File.ReadAllText("./cookies.json"))!;

YTMusicSharp ytClient = new YTMusicSharp(
    youtubHeaders: headers
);

// Fetch home sections
JsonObject homeData = await ytClient.BrowseEndpoint.FetchHomeSections();
```

## Roadmap

- [ ] **Client Settings**: Customizable configuration for the API client.
- [ ] **Caching System**: Implement a robust caching mechanism for improved performance.
- [ ] **Interactions**:
    like
    - [ ] Like/Unlike songs and albums.
    - [ ] Add/Remove tracks from playlists.
- [ ] **Download Stream**: Support for fetching and downloading audio streams.
- [ ] **Song History**: Track and manage user listening history.

## License

This project is licensed under the MIT License.
