using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using NUnit.Framework;
using Game.Util;

namespace Game.Networking
{
    public class ClientServerUnitTests
    {
        [Test]
        public void BasicTest()
        {
            TestLogDispatcher dispatcher = new TestLogDispatcher();

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
            // Mock:
            // 
        }

        [Test]
        public void ConnectTest()
        {
            // TestLogDispatcher dispatcher = new TestLogDispatcher();

            VirtualNetwork network = new VirtualNetwork();
            MockVirtualNetServer server = new MockVirtualNetServer();
            server.VirtualHost(network, "127.0.0.1:27000");
            TestUtil.Wait(() => { return server.State == ServerState.Running; });
            Assert.AreEqual(ServerState.Running, server.State);

            VirtualNetClient client = new VirtualNetClient();
            client.VirtualConnect(network, server.VirtualAddress, "127.0.0.1:64332");
            TestUtil.Wait(() => { return client.State == ClientState.Connected; });
            Assert.AreEqual(ClientState.Connected, client.State);

            client.Close(ShutdownType.Notify);
            TestUtil.Wait(() => { return client.State == ClientState.Shutdown; });
            Assert.AreEqual(ClientState.Shutdown, client.State);

            server.Close(ShutdownType.Immediate);
            Assert.AreEqual(ServerState.Shutdown, server.State);
        }
        
    }
}
