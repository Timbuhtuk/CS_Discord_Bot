using CS_Discord_Bot.Models;
using SpotifyAPI.Web;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace CS_Discord_Bot
{
    /// <summary>
    /// Struct which provides methods for video(s) on Youtube using Youtube/Spotify urls
    /// </summary>
    public struct VideoFinder
    {
        static YoutubeClient client = new YoutubeClient();
        static bool SupportSpotify = bool.Parse(Program.app_config["spotify_settings:SPOTIFY_ENABLED"]);

        /// <summary>
        /// Find video on Youtube by name or link to Youtube or Spotify , supports single tracks and playlists
        /// </summary>
        /// <param name="query">Youtube or Spotify link to track or playlist</param>
        /// <returns>Dictionary&lt;Urls,Titles&gt; or null if no searching results</returns>
        public static async Task<Dictionary<string, string>?> Find(string query)
        {
            using LogScope log_scope = new LogScope($"Find called for {query}", ConsoleColor.Green);

            if (string.IsNullOrEmpty(query))
            {
                return null;
            }
            else if (query.Contains("https://www.youtube.com/watch"))
            {
                try
                {

                    var videoId = VideoId.Parse(query);

                    var watch = new Stopwatch();
                    watch.Start();
                    var video = await client.Videos.GetAsync(videoId);
                    watch.Stop();
                    await Logs.AddLog($"get video info took {watch.Elapsed}");

                    var result = new Dictionary<string, string>();
                    result.Add(video.Url, video.Title);

                    return result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://www.youtube.com/playlist"))
            {
                try
                {

                    var playlist_id = PlaylistId.Parse(query);


                    var watch = new Stopwatch();
                    watch.Start();
                    var videos = client.Playlists.GetVideosAsync(playlist_id);
                    watch.Stop();
                    await Logs.AddLog($"get video info took {watch.Elapsed}");

                    var result = new Dictionary<string, string>();
                    foreach (var video in videos.ToListAsync().Result)
                    {
                        result.Add(video.Url, video.Title);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://open.spotify.com/track/"))
            {
                if (!SupportSpotify) return null;
                try { 
                    var config = SpotifyClientConfig.CreateDefault();
                    var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIFY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIFY_CLIENT_SECRET"]);
                    var response = await new OAuthClient(config).RequestToken(request);
                    var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                    var trackId = query.Split('/').Last().Split("?")[0];
                    var track = await spotify.Tracks.Get(trackId);



                    Console.WriteLine($"Track: {track.Name}, Artist: {track.Artists.First().Name}");
                    return Find($"{track.Name} - {track.Artists.First().Name}").Result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://open.spotify.com/playlist/"))
            {
                if (!SupportSpotify) return null;

                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIFY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIFY_CLIENT_SECRET"]);
                var response = await new OAuthClient(config).RequestToken(request);
                var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                var playlistId = query.Split('/').Last().Split("?")[0];
                var playlist = await spotify.Playlists.Get(playlistId);

                Console.WriteLine($"Playlist: {playlist.Name}");

                var result = new Dictionary<string, string>();

                
                if (playlist.Tracks != null && playlist.Tracks.Items != null)
                {
                    foreach (var trackItem in playlist.Tracks.Items)
                    {
                        var track = trackItem.Track as FullTrack;
                        if (track != null)
                        {
                            var find_result = VideoFinder.Find($"{track.Name} - {track.Artists.First().Name}").Result;
                            if (find_result != null)
                            {
                                var KVP = find_result.First();
                                result.Add(KVP.Key, KVP.Value);
                            }

                        }
                    }
                }
                return result;
            }
            {

                try
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    var searchResults = client.Search.GetResultsAsync(query);
                    var video_raw = await searchResults.FirstAsync();

                    var video_id = VideoId.Parse(video_raw.Url);
                    var video = await client.Videos.GetAsync(video_id);
                    watch.Stop();
                    await Logs.AddLog($"get video info took {watch.Elapsed}");

                    var result = new Dictionary<string, string>();
                    result.Add(video.Url, video.Title);

                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
        /// <summary>
        /// Find video on Youtube by name or link to Youtube or Spotify , supports single tracks and playlists
        /// </summary>
        /// <param name="query">Youtube or Spotify link to track or playlist</param>
        /// <param name="return_song">No matter value of param, but on include it triggers different return on function 'Find'</param>
        /// <returns> List&lt;Song&gt; or null if no searching results</returns>
        public static async Task<List<Song>?> Find(string query, bool return_song)
        {
            using LogScope log_scope = new LogScope($"Find called for {query}",ConsoleColor.Green);

            if (string.IsNullOrEmpty(query))
            {
                return null;
            }
            else if (query.Contains("https://www.youtube.com/watch"))
            {
                try
                {

                    var videoId = VideoId.Parse(query);

                    var watch = new Stopwatch();
                    watch.Start();
                    var video = await client.Videos.GetAsync(videoId);
                    watch.Stop();
                    await Logs.AddLog($"get video info took {watch.Elapsed}");

                    var result = new List<Song>();
                    result.Add(new Song() { 
                        Link = video.Url, 
                        Name = video.Title, 
                        AuthorName = video.Author.ChannelTitle , 
                        Duration = video.Duration != null ? video.Duration.Value.TotalSeconds / 60 : 0 }
                    );

                    return result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://www.youtube.com/playlist"))
            {
                try
                {

                    var playlist_id = PlaylistId.Parse(query);


                    var watch = new Stopwatch();
                    watch.Start();
                    var videos = client.Playlists.GetVideosAsync(playlist_id);
                    watch.Stop();
                    await Logs.AddLog($"get video info took {watch.Elapsed}");

                    var result = new List<Song>();
                    foreach (var video in videos.ToListAsync().Result)
                    {
                        result.Add(new Song()
                        {
                            Link = video.Url,
                            Name = video.Title,
                            AuthorName = video.Author.ChannelTitle,
                            Duration = video.Duration != null ? video.Duration.Value.TotalSeconds / 60 : 0
                        }
                        );
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://open.spotify.com/track/"))
            {
                if (!SupportSpotify) return null;
                try { 
                    var config = SpotifyClientConfig.CreateDefault();
                    var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIFY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIFY_CLIENT_SECRET"]);
                    var response = await new OAuthClient(config).RequestToken(request);
                    var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                    var trackId = query.Split('/').Last().Split("?")[0];
                    var track = await spotify.Tracks.Get(trackId);



                    Console.WriteLine($"Track: {track.Name}, Artist: {track.Artists.First().Name}");
                    return Find($"{track.Name} - {track.Artists.First().Name}",true).Result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://open.spotify.com/playlist/"))
            {
                if (!SupportSpotify) return null;

                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIFY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIFY_CLIENT_SECRET"]);
                var response = await new OAuthClient(config).RequestToken(request);
                var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                var playlistId = query.Split('/').Last().Split("?")[0];
                var playlist = await spotify.Playlists.Get(playlistId);

                Console.WriteLine($"Playlist: {playlist.Name}");

                var result = new List<Song>();

                

                foreach (var trackItem in playlist.Tracks.Items)
                {
                    var track = trackItem.Track as FullTrack;
                    if (track != null)
                    {
                        var song = VideoFinder.Find($"{track.Name} - {track.Artists.First().Name}", true).Result.First();
                        result.Add(song);

                    }
                }
                return result;
            }
            {

                try
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    var searchResults = client.Search.GetResultsAsync(query);
                    var video_raw = await searchResults.FirstAsync();

                    var video_id = VideoId.Parse(video_raw.Url);
                    var video = await client.Videos.GetAsync(video_id);
                    watch.Stop();
                    await Logs.AddLog($"get video info took {watch.Elapsed}");

                    var result = new List<Song>();
                    result.Add(new Song()
                    {
                        Link = video.Url,
                        Name = video.Title,
                        AuthorName = video.Author.ChannelTitle,
                        Duration = video.Duration != null ? video.Duration.Value.TotalSeconds / 60 : 0
                    }
                    );

                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
        /// <summary>
        /// Find playlist on Youtube by link to Youtube or Spotify
        /// </summary>
        /// <param name="query">Youtube or Spotify link to track or playlist</param>
        /// <returns>Dictionary&lt;Urls,Titles&gt; or null if no searching results</returns>
        public static async Task<Dictionary<string, string>?> FindPlaylistByLink(string query)
        {
            await Logs.AddLog($"Find called for {query}");
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }
            else if (query.Contains("https://www.youtube.com/playlist"))
            {
                try
                {

                    var playlist_id = PlaylistId.Parse(query);


                    var watch = new Stopwatch();
                    watch.Start();
                    var videos = client.Playlists.GetVideosAsync(playlist_id);
                    watch.Stop();
                    await Logs.AddLog($"get playlist info took {watch.Elapsed}");

                    var result = new Dictionary<string, string>();
                    foreach (var video in videos.ToListAsync().Result)
                    {
                        result.Add(video.Url, video.Title);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://open.spotify.com/playlist/"))
            {
                if (!SupportSpotify) return null;

                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIFY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIFY_CLIENT_SECRET"]);
                var response = await new OAuthClient(config).RequestToken(request);
                var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                var playlistId = query.Split('/').Last().Split("?")[0];
                var playlist = await spotify.Playlists.Get(playlistId);

                Console.WriteLine($"Playlist: {playlist.Name}");

                var result = new Dictionary<string, string>();



                foreach (var trackItem in playlist.Tracks.Items)
                {
                    var track = trackItem.Track as FullTrack;
                    if (track != null)
                    {
                        var KVP = VideoFinder.Find($"{track.Name} - {track.Artists.First().Name}").Result.First();
                        result.Add(KVP.Key, KVP.Value);

                    }
                }
                return result;
            }
            return null;
        }
        /// <summary>
        /// Find playlist on Youtube by link to Youtube or Spotify
        /// </summary>
        /// <param name="query">Youtube or Spotify link to track or playlist</param>
        ///  /// <param name="return_song">No matter value of param, but on include it triggers different return on function 'Find'</param>
        /// <returns>Dictionary&lt;Urls,Titles&gt; or null if no searching results</returns>
        public static async Task<List<Song>?> FindPlaylistByLink(string query, bool return_song)
        {
            await Logs.AddLog($"Find called for {query}");
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }
            else if (query.Contains("https://www.youtube.com/playlist"))
            {
                try
                {

                    var playlist_id = PlaylistId.Parse(query);


                    var watch = new Stopwatch();
                    watch.Start();
                    var videos = client.Playlists.GetVideosAsync(playlist_id);
                    watch.Stop();
                    await Logs.AddLog($"get playlist info took {watch.Elapsed}");

                    var result = new List<Song>();
                    foreach (var video in videos.ToListAsync().Result)
                    {
                        result.Add(
                            new Song()
                            {
                                Name = video.Title,
                                AuthorName = video.Author.Title,
                                Link = video.Url,
                                Duration = video.Duration != null ? video.Duration.Value.TotalSeconds / 60 : 0
                            });
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (query.Contains("https://open.spotify.com/playlist/"))
            {
                if (!SupportSpotify) return null;

                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIFY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIFY_CLIENT_SECRET"]);
                var response = await new OAuthClient(config).RequestToken(request);
                var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                var playlistId = query.Split('/').Last().Split("?")[0];
                var playlist = await spotify.Playlists.Get(playlistId);

                Console.WriteLine($"Playlist: {playlist.Name}");

                var result = new List<Song>();

                foreach (var trackItem in playlist.Tracks.Items)
                {
                    var track = trackItem.Track as FullTrack;
                    if (track != null)
                    {
                        Song? song = VideoFinder.Find($"{track.Name} - {track.Artists.First().Name}", true).Result?.First();
                        if(song != null)
                            result.Add(song);
                    }
                }
                return result;
            }
            return null;
        }
        /// <summary>
        /// Get playlist name by link
        /// </summary>
        /// <param name="URL">Youtube or Spotify link to playlist</param>
        /// <returns>playlist name</returns>
        public static async Task<string> GetPlaylistName(string URL)
        {
            if (string.IsNullOrEmpty(URL))
            {
                return null;
            }
            else if (URL.Contains("https://www.youtube.com/playlist"))
            {
                try
                {

                    var playlist_id = PlaylistId.Parse(URL);


                    var watch = new Stopwatch();
                    watch.Start();
                    var playlist = await client.Playlists.GetAsync(playlist_id);
                    watch.Stop();
                    await Logs.AddLog($"get playlist info took {watch.Elapsed}");

                    return playlist.Title;
                }
                catch (Exception ex)
                {
                    await Logs.AddLog(ex.Message, LogLevel.ERROR);
                    return null;
                }
            }
            else if (URL.Contains("https://open.spotify.com/playlist/"))
            {
                if (!SupportSpotify) return null;

                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(Program.app_config["spotify_settings:SPOTIPY_CLIENT_ID"], Program.app_config["spotify_settings:SPOTIPY_CLIENT_SECRET"]);
                var response = await new OAuthClient(config).RequestToken(request);
                var spotify = new SpotifyClient(config.WithToken(response.AccessToken));

                var playlistId = URL.Split('/').Last().Split("?")[0];
                var playlist = await spotify.Playlists.Get(playlistId);

                Console.WriteLine($"Playlist: {playlist.Name}");
                foreach (var trackItem in playlist.Tracks.Items)
                {
                    var track = trackItem.Track;
                    if (track != null)
                    {
                        Console.WriteLine(track.ToString());
                        //Console.WriteLine($"Track: {track.Name}, Artist: {track.Artists.First().Name}");
                    }
                }
            }
            return null;
        }
    }
}
