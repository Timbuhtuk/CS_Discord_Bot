using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CS_Discord_Bot
{
    public struct FfmpegInteractor
    {
        public static async Task<string?> ConvertMp3ToPcm(string file_path,string? output_file_path = null)
        {
            if(!Regex.Match(file_path, @"^[A-Z]:(?:\\{1,2}[^\\/:*?\""<>|]+)*\.mp3$").Success)
                return null;
            if (!File.Exists(file_path))
            {
                await Logs.AddLog("File to convert not found!", LogLevel.ERROR);
                return null;
            }

            output_file_path = output_file_path == null ? file_path.Replace("mp3", "pcm") : output_file_path;

            using LogScope log_scope = new LogScope($"ConvertMp3ToPcm called", ConsoleColor.Red);

            var ffmpegPath = Path.Combine(Environment.CurrentDirectory, "appdata", "ffmpeg.exe");

            if (!File.Exists(ffmpegPath))
            {
                await Logs.AddLog("FFmpeg executable not found!", LogLevel.ERROR);
                return null;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{file_path}\" -f s16le -ar 48000 -ac 2 \"{output_file_path}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var ffmpegProcess = new Process { StartInfo = processStartInfo };

            ffmpegProcess.ErrorDataReceived += (sender, e) =>
            {
                //if (!string.IsNullOrEmpty(e.Data))
                    //Logs.AddLog($"FFMPEG Error: {e.Data}").Wait();
            };

            if (!ffmpegProcess.Start())
            {
                await Logs.AddLog("FFMPEG STARTUP ERROR", LogLevel.ERROR);
                return null;
            }

            ffmpegProcess.BeginErrorReadLine();

            await ffmpegProcess.WaitForExitAsync();

            if (ffmpegProcess.ExitCode != 0)
            {
                await Logs.AddLog($"FFmpeg conversion failed with exit code {ffmpegProcess.ExitCode}",LogLevel.ERROR);
            }

            await Logs.AddLog("FFMPEG - conversion completed");
            return output_file_path;
        }

    }
}
