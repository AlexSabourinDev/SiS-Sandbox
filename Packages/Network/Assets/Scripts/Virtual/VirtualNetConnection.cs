using System.Net;

namespace Game.Networking
{
    public class VirtualNetConnection : INetConnection
    {
        public IPEndPoint EndPoint { get { return null; } }
        public string EndPointString { get; private set; } = string.Empty;
        public string Identifier { get; private set; } = string.Empty;
        public byte[] UID { get; private set; } = new byte[0];
        public long TickMilliseconds { get { return 100000000000; } }
        public void Tick() { }

        public VirtualNetConnection()
        {

        }

        public VirtualNetConnection(IVirtualNode node, string identifier, byte[] uid)
        {
            EndPointString = node.VirtualAddress;
            Identifier = identifier;
            UID = uid;
        }
    }
}
