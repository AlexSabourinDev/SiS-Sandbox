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
        private struct ReliablePacket
        {
            public uint UID;
            public byte[] PacketData;
            public Action<AcknowledgePacket, ReliablePacket> Callback;
        }

        private UdpClient m_Socket = null;
        private volatile int m_State = (int)ClientState.Shutdown;
        private Task<UdpReceiveResult> m_ActiveTask = null;
        private uint m_UID = 0;
        private ConcurrentDictionary<uint, ReliablePacket> m_ReliablePackets = new ConcurrentDictionary<uint, ReliablePacket>();

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

            State = ClientState.Connecting;
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
            SendPacket(packet.UID, bytes, OnConnectAcknowledged);
        }

        public void Close(ShutdownType shutdownType)
        {
            if(shutdownType == ShutdownType.Notify && State == ClientState.Connected)
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

            if (shutdownType == ShutdownType.NotifyAndWait && State == ClientState.Connected)
            {
                State = ClientState.WaitingForSocket;
                DisconnectPacket packet = new DisconnectPacket()
                {
                    ProtocolType = Protocol.Disconnect,
                    Flags = ProtocolFlags.Reliable,
                    Crc32 = 0,
                    UID = m_UID++,
                    ConnectionUID = m_ConnectionUID
                };
                byte[] bytes = packet.Write();
                SendPacket(packet.UID, bytes,
                    (AcknowledgePacket ackPacket, ReliablePacket reliablePacket) =>
                    {
                        State = ClientState.ShuttingDown;
                    });

                while (State != ClientState.ShuttingDown) { }
                m_Socket.Close();
                State = ClientState.Shutdown;
            }
            else
            {
                State = ClientState.WaitingForSocket;

                m_Socket.Close();
                State = ClientState.Shutdown;
            }
        }

        private void BeginReceive()
        {
            if (State == ClientState.Connecting || State == ClientState.Connected || State == ClientState.WaitingForSocket)
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
            if(udpResult.RemoteEndPoint == null)
            {
                return;
            }

            if(udpResult.Buffer.Length > NetStream.HEADER_SIZE)
            {
                Protocol protocol = (Protocol)udpResult.Buffer[NetStream.HEADER_SIZE];
                if(protocol == Protocol.Shutdown && State == ClientState.WaitingForSocket)
                {
                    State = ClientState.ShuttingDown;
                    return;
                }
                else if(protocol == Protocol.Acknowledgement)
                {
                    AcknowledgePacket packet = new AcknowledgePacket();
                    if (packet.Read(udpResult.Buffer))
                    {
                        CompletePacket(packet);
                    }
                    else
                    {
                        Log.Debug($"Ignoring corrupt packet with protocol {protocol} from {udpResult.RemoteEndPoint}");
                    }
                }
                else
                {
                    Log.Debug($"Ignoring bad protocol packet {protocol} from {udpResult.RemoteEndPoint}");
                }
            }

            BeginReceive();
        }

        private void SendPacket(uint uid, byte[] data, Action<AcknowledgePacket, ReliablePacket> acknowledgementCallback = null)
        {
            ProtocolFlags flags = (ProtocolFlags)data[NetStream.HEADER_SIZE + 1];

            if((flags & ProtocolFlags.Reliable) > 0)
            {
                ReliablePacket packet = new ReliablePacket()
                {
                    UID = uid,
                    PacketData = data,
                    Callback = acknowledgementCallback
                };
                if (!m_ReliablePackets.TryAdd(uid, packet))
                {
                    throw new InvalidOperationException("Failed to keep reliable packet, could not push item into dictionary");
                }
            }
            else if(acknowledgementCallback != null)
            {
                Log.Warning("Acknowledgement Callback argument was given but the packet is not reliable so it will never be called.");
            }

            int bytesSent = m_Socket.Send(data, data.Length);
            if(bytesSent != data.Length)
            {
                Log.Warning($"Failed to send full packet data. Exepected Bytes={data.Length}, Sent Bytes={bytesSent}");
            }
        }

        private void CompletePacket(AcknowledgePacket packet)
        {
            ReliablePacket reliablePacket;
            if(m_ReliablePackets.TryRemove(packet.UID, out reliablePacket))
            {
                if(reliablePacket.Callback != null)
                {
                    Protocol protocol = (Protocol)reliablePacket.PacketData[NetStream.HEADER_SIZE];
                    Log.Debug($"Processing ack for UID {packet.UID} for protocol {protocol}");
                    reliablePacket.Callback(packet, reliablePacket);
                }
            }
        }

        private void OnConnectAcknowledged(AcknowledgePacket packet, ReliablePacket reliablePacket)
        {
            Protocol protocol = (Protocol)reliablePacket.PacketData[NetStream.HEADER_SIZE];
            if(protocol != Protocol.Connect)
            {
                Log.Error($"OnConnectAcknowledged cannot process protocol {protocol} from packet UID {reliablePacket.UID}");
                return;
            }

            ConnectAckPacket packetData = new ConnectAckPacket();
            NetStream ns = new NetStream();
            ns.Open(packet.Data);
            packetData.NetSerialize(ns);
            ns.Close();

            m_ConnectionUID = packetData.UID;

            bool allZero = true;
            for (int i = 0; i < m_ConnectionUID.Length; ++i)
            {
                if (m_ConnectionUID[i] != 0)
                {
                    allZero = false;
                    break;
                }
            }

            if (allZero)
            {
                Log.Debug($"Connection rejected!");
                State = ClientState.ConnectionFailed;
            }
            else
            {
                State = ClientState.Connected;
            }
        }
     
        public void SendHeartbeat()
        {
            byte protocol = (byte)Protocol.None;
            NetStream ns = new NetStream();
            ns.Open();
            ns.Serialize(ref protocol);
            byte[] bytes = ns.Close();
            SendPacket(0, bytes);
        }
    }
}
