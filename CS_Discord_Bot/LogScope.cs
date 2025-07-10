using CS_Discord_Bot.Enums;
using System.Runtime.CompilerServices;

namespace CS_Discord_Bot
{
    public class LogScope : IDisposable
    {
        private readonly ConsoleColor? storaged_color;
        private string caller;
        public LogScope([CallerMemberName] string caller = "")
        {
            //storaged_color = Logger.outline_color;
            Logger.depth++;
            //Logger.AddLog($"+Scope {caller}", LogLevel.ERROR).Wait();
            this.caller = caller;
        }
        public LogScope(string msg, [CallerMemberName] string caller = "")
        {
            Logger.AddLog(msg, msgType: LogLevel.INFO).Wait();
            Logger.depth++;
            //Logger.AddLog($"+Scope {caller}", LogLevel.ERROR).Wait();
            this.caller = caller;
        }
        public LogScope(string msg, ConsoleColor color, [CallerMemberName] string caller = "")
        {
            storaged_color = Logger.outline_color;
            Logger.outline_color = color;
            Logger.AddLog(msg, msgType: LogLevel.INFO).Wait();
            Logger.depth++;
            //Logger.AddLog($"+Scope {caller}", LogLevel.ERROR).Wait();
            this.caller = caller;
        }

        public void Dispose()
        {
            Logger.depth = Logger.depth - 1 < 0 ? 0 : Logger.depth - 1;
            if (Logger.depth == 0)
            {
                Logger.outline_color = Logger._DEFAULT_OUTLINE_COLOR;
            }
            else if (storaged_color != null)
                Logger.outline_color = storaged_color.Value;
            //Logger.AddLog($"-Scope {caller}", LogLevel.ERROR).Wait();
        }
    }
}

