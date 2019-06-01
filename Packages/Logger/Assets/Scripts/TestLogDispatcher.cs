using System.Collections.Concurrent;
using UnityEngine;

namespace Game.Util
{
    public class TestLogDispatcher : IAsyncLogHandler
    {
        public struct Message
        {
            public LogLevel LogLevel;
            public string LogMessage;
        }
        private ConcurrentQueue<Message> m_Items = new ConcurrentQueue<Message>();

        public TestLogDispatcher()
        {
            LogConfig.LoggingLevel = LogLevel.Debug;
            Log.LogHandler = this;
        }

        public void Output(LogLevel logLevel, string formattedMessage)
        {
            m_Items.Enqueue(new Message() { LogLevel = logLevel, LogMessage = formattedMessage });
        }

        public void Update()
        {
            Message msg;
            if(m_Items.TryDequeue(out msg))
            {
                switch(msg.LogLevel)
                {
                    case LogLevel.Debug:
                    case LogLevel.Info:
                        {
                            Debug.Log(msg.LogMessage);
                        } break;
                    case LogLevel.Warning:
                        {
                            Debug.LogWarning(msg.LogMessage);
                        } break;
                    case LogLevel.Error:
                        {
                            Debug.LogError(msg.LogMessage);
                        } break;
                }
            }
        }
    }
}
