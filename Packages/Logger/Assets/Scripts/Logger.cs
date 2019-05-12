using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

// warning CS0162: Unreachable code detected : Don't care for unreachable code in this file, stuff may become unreachable due to config. In the future we may want a runtime config vs baking into exe.
#pragma warning disable 0162

namespace Game.Util
{
    public class Logger
    {
        private LogFile m_File = null;
        public IAsyncLogHandler Handler { get; set; } = null;

        public Logger()
        {
            if (LogConfig.OutputFile && !string.IsNullOrEmpty(LogConfig.LogDirectory))
            {
                m_File = new LogFile(LogConfig.LogDirectory, LogConfig.AppendLogFile, LogConfig.ClearOldLogFiles);
            }
        }

        private string LogFormat(string message, LogLevel logLevel, int lineNumber, string filename, DateTime timeStamp)
        {
            StringBuilder sb = new StringBuilder();

            // DateTime formatting [YYYY-MM-DD--HH:MM:SS:mm]
            if (LogConfig.FormatDate || LogConfig.FormatTime)
            {
                sb.Append('[');
            }
            if (LogConfig.FormatDate)
            {
                sb.Append(timeStamp.Year).Append('-')
                    .Append(timeStamp.Month).Append('-')
                    .Append(timeStamp.Day);

                if (LogConfig.FormatTime)
                {
                    sb.Append("--");
                }
            }

            if (LogConfig.FormatTime)
            {
                sb.Append(timeStamp.Hour).Append(':')
                    .Append(timeStamp.Minute).Append(':')
                    .Append(timeStamp.Second).Append(':')
                    .Append(timeStamp.Millisecond);
            }
            if (LogConfig.FormatDate || LogConfig.FormatTime)
            {
                sb.Append(']');
            }

            if (LogConfig.FormatFile)
            {
                sb.Append('[').Append(filename).Append(':').Append(lineNumber).Append(']');
            }

            if (sb.Length != 0)
            {
                sb.Append("   ");
            }
            sb.Append('[').Append(logLevel).Append(']')
                .Append(' ').Append(message);

            if (!string.IsNullOrEmpty(message) && message[message.Length - 1] != '\n')
            {
                sb.Append('\n');
            }

            return sb.ToString();
        }

        public async Task WriteAsync(string message, LogLevel logLevel, int lineNumber, string filename)
        {
            if (logLevel < LogConfig.LoggingLevel)
            {
                return;
            }

            DateTime timeStamp = DateTime.Now;

            await Task.Run(async () =>
            {
                int index = filename.LastIndexOf('\\');
                string localFile = index >= 0 ? filename.Substring(index) : filename;
                string formattedString = LogFormat(message, logLevel, lineNumber, localFile, timeStamp);

                if (LogConfig.OutputFile && m_File != null)
                {
                    await m_File.WriteAsync(formattedString);
                }
                if (LogConfig.OutputStd)
                {
                    Console.Write(formattedString);
                }
                if (LogConfig.OutputEditor)
                {
                    Debug.Write(formattedString);
                }
                if(Handler != null)
                {
                    Handler.Output(logLevel, formattedString);
                }
            });
        }

    }
}
