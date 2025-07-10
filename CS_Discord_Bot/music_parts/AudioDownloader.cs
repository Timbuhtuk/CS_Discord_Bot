using CS_Discord_Bot.Enums;
using CS_Discord_Bot.Models;
using System.Diagnostics;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;

namespace CS_Discord_Bot
{
    /// <summary>
    /// Enum with names of libs for downloading
    /// </summary>
    public enum Provider
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        YoutubeExplode,
        YoutubeDLSharp
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    public struct AudioDownloader
    {
        private static readonly YoutubeDL ytdl = new YoutubeDL();
        private static readonly YoutubeClient youtube = new YoutubeClient();

        private const string download_file_extension = ".mp3";
        private const string storage_file_extension = ".pcm";
        private const string YoutubeDLPath = "appdata\\yt-dlp.exe";

        private const Provider current_working_provider = Provider.YoutubeExplode;

        /// <summary>
        /// Download audio from youtube video using YoutubeExplode
        /// </summary>
        /// <param name="Title">song name for saving</param>
        /// <param name="Url">Url of song audio stream</param>
        /// <param name="path">output directory for downloading .mp3</param>
        /// <param name="provider">determinates witch library use to download the audio</param>
        /// <returns>valid FilePath .mp3</returns>
        public static async Task<string?> Download(string Title, string Url, string path, Provider provider = current_working_provider)
        {
            using (LogScope log_scope = new LogScope($"Download called for {Title}, provider: {provider}", ConsoleColor.Red))
            {
                var pureVideoName = StringToCorrectFileName(Title);
                var outputName = pureVideoName + download_file_extension;
                var output = Path.Combine(path, outputName);

                if (File.Exists(Path.Combine(path, pureVideoName + storage_file_extension)))
                {
                    await Logger.AddLog($"Audio already downloaded and converted");
                    return output;
                }
                if (File.Exists(output))
                {
                    await Logger.AddLog($"Audio already downloaded");
                    output = await FfmpegInteractor.ConvertMp3ToPcm(output);
                    return output;
                }

                if (provider == Provider.YoutubeExplode)
                {
                    try
                    {
                        var video = await youtube.Videos.GetAsync(Url);
                        await Logger.AddLog($"Video found: {video.Title}");

                        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                        var audioStreamInfo = streamManifest.GetAudioOnlyStreams().Where(S => S.Bitrate.BitsPerSecond == streamManifest.GetAudioOnlyStreams().Max(s => s.Bitrate.BitsPerSecond)).First();

                        if (audioStreamInfo == null)
                        {
                            await Logger.AddLog($"No audio stream available", LogLevel.ERROR);
                            return null;
                        }

                        var watch = new Stopwatch();
                        watch.Start();
                        await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, output);
                        watch.Stop();

                        await Logger.AddLog($"Download completed in {watch.Elapsed}");
                        output = await FfmpegInteractor.ConvertMp3ToPcm(output);
                        return output;
                    }
                    catch (Exception ex)
                    {
                        await Logger.AddLog($"Download failed: {ex.Message}", LogLevel.ERROR);
                        return null;
                    }
                }
                if (provider == Provider.YoutubeDLSharp)
                {
                    ytdl.YoutubeDLPath = YoutubeDLPath;

                    var options = new OptionSet
                    {
                        Output = output,
                        Format = "bestaudio"
                    };

                    string url = Url;

                    var watch = new Stopwatch();
                    watch.Start();
                    var result = await ytdl.RunVideoDownload(url, overrideOptions: options);
                    watch.Stop();
                    await Logger.AddLog($"download video taken {watch.Elapsed}");


                    if (result.Success)
                    {
                        await Logger.AddLog("Download completed successfully!");
                        output = await FfmpegInteractor.ConvertMp3ToPcm(output);
                        return output;
                    }
                    else
                    {
                        await Logger.AddLog($"Download failed: {result.ErrorOutput[0]}", LogLevel.ERROR);
                        return null;
                    }

                }
            }
            return null;

        }

        /// <summary>
        /// Download audio from youtube video using YoutubeExplode
        /// </summary>
        /// <param name="song">song object instance</param>
        /// <param name="path">output directory for downloading .mp3</param>
        ///   /// <param name="provider">determinates witch library use to download the audio</param>
        /// <returns>Song object instance with valid FilePath .mp3</returns>
        public static async Task<Song?> Download(Song song, string path, Provider provider = current_working_provider)
        {
            if (song.Link != null)
            {
                var result = await Download(song.Name, song.Link, path, provider);
                if (result == null)
                    return null;

                song.FilePath = result;
                return song;
            }
            else
            {
                await Logger.AddLog("song link was null", LogLevel.ERROR);
                return null;
            }
        }
        /// <summary>
        /// Download audios from youtube video using YoutubeExplode
        /// </summary>
        /// <param name="Url_Title">video urls and titles</param>
        /// <param name="path">output directory for downloading .mp3</param>
        /// <returns>full filenames including path</returns>
        /// <remarks>format video title without InvalidFileNameChars and then save</remarks>
        public static async Task<List<string>> Download(Dictionary<string, string> Url_Title, string path)
        {
            var result = new List<string>();
            foreach (var kvp in Url_Title)
            {
                var out_path = await Download(kvp.Value, kvp.Key, path);
                if (out_path != null)
                    result.Add(out_path);
            }
            return result;
        }
        /// <summary>
        /// Download audios from youtube video using YoutubeExplode
        /// </summary>
        /// <param name="songs">list of song object instances</param>
        /// <param name="path">output directory for downloading .mp3</param>
        /// <returns>song object instances with valid FilePath</returns>
        /// <remarks>format video title without InvalidFileNameChars and then save</remarks>
        public static async Task<List<Song>> Download(List<Song> songs, string path)
        {
            var result = new List<Song>();
            foreach (var song in songs)
            {
                var out_path = await Download(song, path);
                if (out_path != null)
                    result.Add(out_path);
            }
            return result;
        }
        /// <summary>
        /// Convert string to correct filename by deleting invalid file name chars
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>correct filename without invalid file name chars</returns>
        public static string StringToCorrectFileName(string str)
        {
            return $"{string.Concat(str.Split(Path.GetInvalidFileNameChars()))}";
        }
    }
}

