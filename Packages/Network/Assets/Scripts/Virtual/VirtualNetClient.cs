using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Game.Util;

namespace Game.Networking
{
    public class VirtualNetClient : IVirtualNode
    {
        private struct ReliablePacket
        {
            public uint UID;
            public byte[] PacketData;
            public Action<AcknowledgePacket, ReliablePacket> Callback;
        }
        protected struct Stats
        {
            public int BadProtocolPackets;
            public int CorruptAckPackets;

            public static Stats Default
            {
                get
                {
                    return new Stats()
                    {
                        BadProtocolPackets = 0,
                        CorruptAckPackets = 0
                    };
                }
            }
        }

        private VirtualNetwork m_Socket = null;
        private volatile int m_State = (int)ClientState.Shutdown;
        private string m_ConnectionIdentifier = string.Empty;
        private byte[] m_ConnectionUID = null;
        private uint m_UID = 0;
        private string m_Server = string.Empty;
        private ConcurrentDictionary<uint, ReliablePacket> m_ReliablePackets = new ConcurrentDictionary<uint, ReliablePacket>();
        protected Stats m_Stats = Stats.Default;


        public ClientState State
        {
            get { ClientState state = (ClientState)m_State; Thread.MemoryBarrier(); return state; }
            private set { m_State = (int)value; Thread.MemoryBarrier(); }
        }
        public string VirtualAddress { get; private set; }

        public void VirtualConnect(VirtualNetwork network, string server, string address)
        {
            if (State != ClientState.Shutdown)
            {
                throw new InvalidOperationException("VirtualNetClient cannot host as it is not in 'Shutdown' state.");
            }

            State = ClientState.Connecting;

            m_ConnectionIdentifier = address;
            m_Server = server;
            VirtualAddress = address;
            m_Socket = network;
            m_Socket.Connect(this);

            // Begin Connection
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

            if(shutdownType == ShutdownType.NotifyAndWait && State == ClientState.Connected)
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
                m_Socket.Disconnect(this);
            }
            else
            {
                State = ClientState.WaitingForSocket;
                
                // Create shutdown message
                NetStream ns = new NetStream();
                byte msg = (byte)Protocol.Shutdown;
                ns.Open();
                ns.Serialize(ref msg);
                byte[] msgBytes = ns.Close();

                // Emulate unblocking a socket by sending the Shutdown message to ourself
                m_Socket.Send(VirtualAddress, this, msgBytes);
                while (State != ClientState.ShuttingDown) { }

                m_Socket.Disconnect(this);
                State = ClientState.Shutdown;
            }
        }

        public void OnReceive(IVirtualNode sender, byte[] data)
        {
            if(State == ClientState.ShuttingDown)
            {
                return;
            }

            if(data.Length > NetStream.HEADER_SIZE)
            {
                Protocol protocol = (Protocol)data[NetStream.HEADER_SIZE];
                if (protocol == Protocol.Shutdown && State == ClientState.WaitingForSocket)
                {
                    State = ClientState.ShuttingDown;
                    return;
                }
                else if(protocol == Protocol.Acknowledgement)
                {
                    AcknowledgePacket packet = new AcknowledgePacket();
                    if (packet.Read(data))
                    {
                        CompletePacket(packet);
                    }
                    else
                    {
                        Interlocked.Increment(ref m_Stats.CorruptAckPackets);
                    }
                }
                else
                {
                    Interlocked.Increment(ref m_Stats.BadProtocolPackets);
                }
            }
        }

        private void SendPacket(uint uid, byte[] data, Action<AcknowledgePacket, ReliablePacket> acknowledgementCallback = null)
        {
            ProtocolFlags flags = (ProtocolFlags)data[NetStream.HEADER_SIZE + 1];

            if ((flags & ProtocolFlags.Reliable) > 0)
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

            if(!m_Socket.Send(m_Server, this, data))
            {
                Log.Warning("Failed to send packet.");
            }
        }

        private void CompletePacket(AcknowledgePacket packet)
        {
            ReliablePacket reliablePacket;
            if (m_ReliablePackets.TryRemove(packet.UID, out reliablePacket))
            {
                if (reliablePacket.Callback != null)
                {
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
                State = ClientState.Shutdown;
            }
            else
            {
                State = ClientState.Connected;
            }
        }
    }
}
