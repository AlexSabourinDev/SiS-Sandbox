using System.Diagnostics;
using System.Net;

namespace Game.Networking
{
    public class NetConnection : INetConnection
    {
        private IPEndPoint  m_EndPoint = null;
        private string      m_Identifier = "";
        private byte[]      m_UID = null;
        private Stopwatch   m_Stopwatch = new Stopwatch();

        public IPEndPoint EndPoint { get { return m_EndPoint; } }
        public string EndPointString { get { return m_EndPoint != null ? m_EndPoint.ToString() : string.Empty; } }
        public string Identifier { get { return m_Identifier; } }
        public byte[] UID { get { return m_UID; } }
        public long TickMilliseconds { get { return m_Stopwatch.ElapsedMilliseconds; } }
        public void Tick() { m_Stopwatch.Restart(); }

        public NetConnection()
        {
            m_Stopwatch.Start();
        }

        public NetConnection(IPEndPoint endPoint, string identifier, byte[] uid)
        {
            m_EndPoint = endPoint;
            m_Identifier = identifier;
            m_UID = uid;
            m_Stopwatch.Start();
        }
    }
}
