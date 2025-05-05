using Discord;
using System.Runtime.CompilerServices;

namespace CS_Discord_Bot
{
    public enum LogLevel
    {
        INFO,
        ERROR,
        WARNING,
    }
    public struct Logs
    {
        private static readonly object _logLock = new object();
        public static int LoggingLevel { get; set; } = Int16.Parse(Program.app_config["logging"]!); // 0 - no logging, 1 - only error, 2 - warnings, 3 - all
        public static int depth = 0;
        public const ConsoleColor _DEFAULT_OUTLINE_COLOR = ConsoleColor.Cyan;
        public static ConsoleColor outline_color = ConsoleColor.Cyan;

        public static async Task AddLog(
            string message,
            ConsoleColor color,
            int log_level = 1,
            string message_type = "UNK",
            [CallerMemberName] string caller = "",
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            await Task.Yield();


            if (LoggingLevel >= log_level)
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string fileShort = System.IO.Path.GetFileName(file);

                string offset = "";

                for (int q = 0; q < depth; q++)
                    offset += " |";
                if (depth > 0)
                    offset += "->";
                else if (color == ConsoleColor.White)
                    color = outline_color;


                int thread_id = Thread.CurrentThread.ManagedThreadId;

                lock (_logLock)
                {

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{time}]");
                    Console.ForegroundColor = outline_color;
                    Console.Write($"{offset}");
                    Console.ForegroundColor = color;
                    Console.Write($"{message} ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"[{message_type}][{thread_id}][{caller} @ {fileShort}:{line}]");
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }
        public static async Task AddLog(
        string message,
        LogLevel msgType = LogLevel.INFO,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        {
            ConsoleColor color = msgType == LogLevel.ERROR ? ConsoleColor.Red : msgType == LogLevel.WARNING ? ConsoleColor.Yellow : ConsoleColor.White;
            string type = msgType == LogLevel.ERROR ? "ERR" : msgType == LogLevel.WARNING ? "WAR" : "INF";
            int level = msgType == LogLevel.ERROR ? 1 : msgType == LogLevel.WARNING ? 2 : 3;
            await AddLog(message, color, level, type, caller, file, line);
        }

        public static async Task AddLog(LogMessage log)
        {
            await AddLog(log.Exception == null ? log.Message : log.Exception.Message, log.Exception == null ? LogLevel.INFO : LogLevel.ERROR);
        }
    }
    public class LogScope : IDisposable
    {
        private readonly ConsoleColor? storaged_color;
        private string caller;
        public LogScope([CallerMemberName] string caller = "")
        {
            //storaged_color = Logs.outline_color;
            Logs.depth++;
            //Logs.AddLog($"+Scope {caller}", LogLevel.ERROR).Wait();
            this.caller = caller;
        }
        public LogScope(string msg, [CallerMemberName] string caller = "")
        {
            Logs.AddLog(msg, msgType: LogLevel.INFO).Wait();
            Logs.depth++;
            //Logs.AddLog($"+Scope {caller}", LogLevel.ERROR).Wait();
            this.caller = caller;
        }
        public LogScope(string msg, ConsoleColor color, [CallerMemberName] string caller = "")
        {
            storaged_color = Logs.outline_color;
            Logs.outline_color = color;
            Logs.AddLog(msg, msgType: LogLevel.INFO).Wait();
            Logs.depth++;
            //Logs.AddLog($"+Scope {caller}", LogLevel.ERROR).Wait();
            this.caller = caller;
        }

        public void Dispose()
        {
            Logs.depth = Logs.depth - 1 < 0 ? 0 : Logs.depth - 1;
            if (Logs.depth == 0)
            {
                Logs.outline_color = Logs._DEFAULT_OUTLINE_COLOR;
            }
            else if (storaged_color != null)
                Logs.outline_color = storaged_color.Value;
            //Logs.AddLog($"-Scope {caller}", LogLevel.ERROR).Wait();
        }
    }
}

