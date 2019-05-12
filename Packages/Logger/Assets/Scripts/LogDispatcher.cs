using System.Collections.Generic;
using UnityEngine;

namespace Game.Util
{
    /// <summary>
    /// This script will auto-magically inject a gameobject with this behavior attached
    /// and will synchronize log output to a unity thread so we can output log messages
    /// to the unity console. 
    /// </summary>
    public class LogDispatcher : MonoBehaviour, IAsyncLogHandler
    {
        private struct LogMessage
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; }
        }

        private static GameObject Instance { get; set; }
        private readonly object  m_Lock = new object();
        private List<LogMessage> m_Messages = new List<LogMessage>();

        public void Output(LogLevel logLevel, string formattedMessage)
        {
            if(!LogConfig.OutputUnity)
            {
                return;
            }

            // todo: May use concurrent queue in future
            lock(m_Lock)
            {
                m_Messages.Add(new LogMessage() { Level = logLevel, Message = formattedMessage });
            }
        }

        void Start()
        {
            Log.LogHandler = this;
        }

        void OnDestroy()
        {
            if(this == Log.LogHandler as LogDispatcher)
            {
                Log.LogHandler = null;
            }
        }

        void Update()
        {
            LogMessage[] messages = null;

            lock(m_Lock)
            {
                if(m_Messages.Count > 0)
                {
                    messages = m_Messages.ToArray();
                    m_Messages.Clear();
                }
            }

            if(messages != null)
            {
                for(int i = 0; i < messages.Length; ++i)
                {
                    switch(messages[i].Level)
                    {
                        case LogLevel.Debug:
                        case LogLevel.Info:
                            Debug.Log(messages[i].Message);
                            break;
                        case LogLevel.Warning:
                            Debug.LogWarning(messages[i].Message);
                            break;
                        case LogLevel.Error:
                        default:
                            Debug.LogError(messages[i].Message);
                            break;
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnLoad()
        {
            if (!Instance)
            {
                // In the event that the script gets recompiled and we lose our static data,
                // try and fetch from the scene.
                LogDispatcher dispatcher = FindObjectOfType<LogDispatcher>();
                if(dispatcher != null)
                {
                    Instance = dispatcher.gameObject;
                    return;
                }

                GameObject gameObject = new GameObject("[Game.Logger]-LogDispatcher");
                DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<LogDispatcher>();
            }
        }
    }

}

