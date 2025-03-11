
namespace GPT3_Interactor
{
    public enum LogLevel
    {
        INFO,
        ERROR,
        WARNING,
    }
    public static class Logs
    {
        private static readonly object _logLock = new object();
        public static int LoggingLevel { get; set; } = 3; // 0 - no logging, 1 - only error, 2 - warnings, 3 - all
 
        
        public static async Task AddLog(string message, LogLevel msgType = LogLevel.INFO)
        {
            await Task.Yield();
            string messageType = "u";
            ConsoleColor messageColor = ConsoleColor.White;
            int logLevel = 1;

            switch (msgType)
            {
                case LogLevel.ERROR:
                    messageType = "[ERR]";
                    messageColor = ConsoleColor.Red;
                    logLevel = 1;
                    break;
                case LogLevel.INFO:
                    messageType = "[INF]";
                    messageColor = ConsoleColor.Gray;
                    logLevel = 3;
                    break;
                case LogLevel.WARNING:
                    messageType = "[WAR]";
                    messageColor = ConsoleColor.Yellow;
                    logLevel = 2;
                    break;
                default:
                    messageType = "[UNK]";
                    messageColor = ConsoleColor.White;
                    logLevel = 1;
                    break;
            }

            if (LoggingLevel >= logLevel)
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                lock (_logLock)
                {
                    Console.ForegroundColor = messageColor;
                    Console.WriteLine($"[{time}]{messageType} {message}");
                    Console.ResetColor();
                }
            }
        }
       
    }
}