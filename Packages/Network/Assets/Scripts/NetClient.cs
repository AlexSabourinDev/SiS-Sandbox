using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Game.Util;

namespace Game.Networking
{
    public class NetClient : INetClient
    {
        private UdpClient m_Socket = null;
        private volatile int m_State = (int)ClientState.Shutdown;
        private Task<UdpReceiveResult> m_ActiveTask = null;
        private uint m_UID = 0;
        private ConcurrentDictionary<uint, byte[]> m_ReliablePackets = new ConcurrentDictionary<uint, byte[]>();

        private string m_ConnectionIdentifier = string.Empty;
        private byte[] m_ConnectionUID = null;

        public ClientState State
        {
            get { ClientState state = (ClientState)m_State; Thread.MemoryBarrier(); return state; }
            private set { m_State = (int)value; Thread.MemoryBarrier(); }
        }

        public void Connect(string identifier, string ipAddress, int port)
        {
            if(State != ClientState.Shutdown)
            {
                throw new InvalidOperationException("NetClient cannot host as it is not in 'Shutdown' state.");
            }

            State = ClientState.Running;
            m_Socket = new UdpClient(ipAddress, port);
            m_ConnectionIdentifier = identifier;
            BeginReceive();

            // Send Message: await response:
            ConnectPacket packet = new ConnectPacket()
            {
                ProtocolType = Protocol.Connect,
                Flags = ProtocolFlags.Reliable,
                Crc32 = 0,
                Identifier = m_ConnectionIdentifier,
                UID = m_UID++
            };

            byte[] bytes = packet.Write();
            SendPacket(packet.UID, bytes);
        }

        public void Close(ShutdownType shutdownType)
        {
            if(shutdownType == ShutdownType.Notify)
            {
                DisconnectPacket packet = new DisconnectPacket()
                {
                    ProtocolType = Protocol.Disconnect,
                    Flags = ProtocolFlags.None,
                    Crc32 = 0,
                    UID = m_UID++,
                    ConnectionUID = m_ConnectionUID
                };

                byte[] bytes = packet.Write();
                SendPacket(packet.UID, bytes);
            }

            State = ClientState.ShuttingDown;
            m_Socket.Close();
        }

        private void BeginReceive()
        {
            if(State == ClientState.Running)
            {
                m_ActiveTask = m_Socket.ReceiveAsync();
                m_ActiveTask.ContinueWith(OnReceive);
            }
        }

        private void OnReceive(Task<UdpReceiveResult> result)
        {
            if(State == ClientState.ShuttingDown || result.IsCanceled)
            {
                return;
            }

            // Send(x, guid) : 
            // Wait(guid, timeout)

            UdpReceiveResult udpResult = result.Result;
            if(udpResult.Buffer.Length > NetStream.HEADER_SIZE)
            {
                Protocol protocol = (Protocol)udpResult.Buffer[NetStream.HEADER_SIZE];
                if(protocol == Protocol.Shutdown && State == ClientState.WaitingForSocket)
                {
                    State = ClientState.ShuttingDown;
                    return;
                }
                else if(protocol != Protocol.None)
                {
                    if(protocol == Protocol.Acknowledgement)
                    {
                        AcknowledgePacket packet = new AcknowledgePacket();
                        if(packet.Read(udpResult.Buffer))
                        {
                            CompletePacket(packet);
                        }
                    }
                }
            }

            BeginReceive();
        }

        private void SendPacket(uint uid, byte[] data)
        {
            ProtocolFlags flags = (ProtocolFlags)data[NetStream.HEADER_SIZE + 1];

            if((flags & ProtocolFlags.Reliable) > 0)
            {
                if (!m_ReliablePackets.TryAdd(uid, data))
                {
                    throw new InvalidOperationException("Failed to keep reliable packet, could not push item into dictionary");
                }
            }

            int statusCode = m_Socket.Send(data, data.Length);
            Log.Debug($"Send with StatusCode={statusCode}");
            
        }

        private void CompletePacket(AcknowledgePacket packet)
        {
            byte[] data;
            if(m_ReliablePackets.TryRemove(packet.UID, out data))
            {
                Protocol protocol = (Protocol)data[NetStream.HEADER_SIZE];
                ConnectAckPacket packetData = new ConnectAckPacket();
                NetStream ns = new NetStream();
                ns.Open(packet.Data);
                packetData.NetSerialize(ns);
                ns.Close();

                m_ConnectionUID = packetData.UID;

                bool allZero = true;
                for(int i = 0; i < m_ConnectionUID.Length; ++i)
                {
                    if(m_ConnectionUID[i] != 0)
                    {
                        allZero = false;
                        break;
                    }
                }

                if(allZero)
                {
                    Log.Debug($"Connection rejected!");
                }
            }
        }
        
    }
}
