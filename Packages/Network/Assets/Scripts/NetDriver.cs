using System;

namespace Game.Networking
{
    public class NetDriver : INetDriver
    {
        private INetClient m_Client = null;
        private INetServer m_Server = null;

        public bool IsRunning
        {
            get
            {
                return m_Client != null ? 
                    m_Client.State == ClientState.Running :
                    m_Server != null && m_Server.State == ServerState.Running;
            }
        }

        public void Close(ShutdownType shutdownType)
        {
            if(!IsRunning)
            {
                throw new InvalidOperationException("NetDriver cannot close as it's not running!");
            }

            if(m_Client != null)
            {
                m_Client.Close(shutdownType);
            }

            if(m_Server != null)
            {
                m_Server.Close(shutdownType);
            }
        }

        public void Connect(string ipAddress, int port)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("NetDriver cannot connect to server. It is already running, a call to Close must first be made.");
            }

            m_Client = new NetClient();
            m_Client.Connect("Test Client", ipAddress, port);
        }

        public void Host(int port)
        {
            if(IsRunning)
            {
                throw new InvalidOperationException("NetDriver cannot host a server. It is already running, a call to Close must first be made.");
            }
            m_Server = new NetServer();
            m_Server.Host(port);
        }

        public void ReplicateObject(IReplicated replicatedObject)
        {

        }

        public void DestroyObject(IReplicated replicatedObject)
        {
            
        }

        // todo: Need more arguments such as ProtocolFlags, and Who we are sending the remote method call to
        public void CallRemoteMethod(IReplicated target, string methodName, byte[] data)
        {


            // Create Packet:
            RemoteMethodPacket packet = new RemoteMethodPacket()
            {
                ProtocolType = Protocol.RemoteMethod,
                Flags = ProtocolFlags.None,
                ObjectID = target.NetworkID,
                MethodID = target.NetworkType.GetMethodID(methodName),
                Crc32 = 0,
                Data = data
            };

            // Write Data:
            byte[] packetData = packet.Write();

            // Send it!
            
        }
    }
}
