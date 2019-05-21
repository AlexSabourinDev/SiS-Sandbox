using System.Threading;
using NUnit.Framework;

namespace Game.Networking
{
    public class ClientServerUnitTests
    {
        [Test]
        public void BasicTest()
        {
            VirtualNetwork network = new VirtualNetwork();

            VirtualNetServer server = new VirtualNetServer();
            server.VirtualHost(network, "127.0.0.1:27000");

            VirtualNetClient client = new VirtualNetClient();
            client.VirtualConnect(network, server.VirtualAddress, "127.0.0.1:64332");
            Thread.Sleep(2500);
            client.Close(ShutdownType.NotifyAndWait);
            Thread.Sleep(2500);
            server.Close(ShutdownType.Immediate);
        }
    }
}
