using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using NUnit.Framework;
using Game.Util;

namespace Game.Networking
{
    public class ClientServerUnitTests
    {
        public class Dispatcher : IAsyncLogHandler
        {
            public struct Message
            {
                public LogLevel LogLevel;
                public string LogMessage;
            }

            private ConcurrentQueue<Message> Items = new ConcurrentQueue<Message>();

            public Dispatcher()
            {
                LogConfig.LoggingLevel = LogLevel.Debug;
                Log.LogHandler = this;
            }

            public void Output(LogLevel logLevel, string formattedMessage)
            {
                Items.Enqueue(new Message() { LogLevel = logLevel, LogMessage = formattedMessage });
            }

            public void Update()
            {
                Message msg;
                if(Items.TryDequeue(out msg))
                {
                    switch(msg.LogLevel)
                    {
                        case LogLevel.Debug:
                        case LogLevel.Info:
                            {
                                UnityEngine.Debug.Log(msg.LogMessage);
                                break;
                            }
                        case LogLevel.Warning:
                            {
                                UnityEngine.Debug.LogWarning(msg.LogMessage);
                                break;
                            }
                        case LogLevel.Error:
                            {
                                UnityEngine.Debug.LogError(msg.LogMessage);
                                break;
                            }
                    }
                }
            }
        }

        [Test]
        public void BasicTest()
        {
            Dispatcher dispatcher = new Dispatcher();

            VirtualNetwork network = new VirtualNetwork();

            VirtualNetServer server = new VirtualNetServer();
            server.VirtualHost(network, "127.0.0.1:27000");

            VirtualNetClient client = new VirtualNetClient();
            client.VirtualConnect(network, server.VirtualAddress, "127.0.0.1:64332");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while(watch.ElapsedMilliseconds < 2500)
            {
                dispatcher.Update();
            }
            client.Close(ShutdownType.Notify);

            watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < 2500)
            {
                dispatcher.Update();
            }
            server.Close(ShutdownType.Immediate);

            watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < 100)
            {
                dispatcher.Update();
            }
            watch = new Stopwatch();

            Log.LogHandler = null;
        }

        
    }
}
