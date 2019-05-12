using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Game.Util
{
    /// <summary>
    /// Static interface for log, creates a logger on demand (thread safe) then outputs redirects all output to it.
    /// </summary>
    public static class Log
    {
        // Acquired on demand
        private static object s_Mutex = new object();
        private static Logger s_Logger = null;
        private static Logger GetLogger()
        {
            if (s_Logger == null)
            {
                lock (s_Mutex)
                {
                    if (s_Logger == null)
                    {
                        s_Logger = new Logger();
                    }
                }
            }
            return s_Logger;
        }
        
        public static IAsyncLogHandler LogHandler
        {
            get { return GetLogger().Handler; }
            set { GetLogger().Handler = value; }
        }

        private static void Write(string message, LogLevel logLevel, int lineNumber, string filename)
        {
            Task task = GetLogger().WriteAsync(message, logLevel, lineNumber, filename);
        }

        public static void Debug(string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filename = "")
        {
            Write(message, LogLevel.Debug, lineNumber, filename);
        }

        public static void Info(string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filename = "")
        {
            Write(message, LogLevel.Info, lineNumber, filename);
        }

        public static void Warning(string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filename = "")
        {
            Write(message, LogLevel.Warning, lineNumber, filename);
        }

        public static void Error(string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filename = "")
        {
            Write(message, LogLevel.Error, lineNumber, filename);
        }
    }
}
