using System.Net;

namespace Game.Networking
{
    public interface INetConnection
    {
        IPEndPoint EndPoint { get; }
        string Identifier { get; }
        byte[] UID { get; }
    }
}
