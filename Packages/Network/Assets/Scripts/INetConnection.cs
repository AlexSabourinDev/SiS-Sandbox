using System.Net;

namespace Game.Networking
{
    public interface INetConnection
    {
        IPEndPoint EndPoint { get; }
        string EndPointString { get; }
        string Identifier { get; }
        byte[] UID { get; }
    }
}
