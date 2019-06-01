using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Game.Util;

namespace Game.Networking
{
    /// <summary>
    /// A virtual implementation of a NetServer that is supposed to be used for debugging
    /// </summary>
    public class VirtualNetServer : IVirtualNode
    {
        private static readonly int NUM_PROTOCOLS = Enum.GetValues(typeof(Protocol)).Length;
        private struct VirtualReceiveResult
        {
            public IVirtualNode Sender;
            public byte[] Buffer;
        }
        protected struct Stats
        {
            public int BadProtocolPackets;

            public static Stats Default
            {
                get
                {
                    return new Stats()
                    {
                        BadProtocolPackets = 0
                    };
                }
            }
        }

        private volatile int m_State = (int)ServerState.Shutdown;
        protected VirtualNetwork m_Socket = null;
        protected List<INetConnection> m_Connections = new List<INetConnection>();
        private readonly object m_ConnectionLock = new object();
        private int m_MaxConnections = 100;
        private PacketProcessor<VirtualReceiveResult> m_PacketProcessor = new PacketProcessor<VirtualReceiveResult>();

        // Stats:
        protected Stats m_Stats = Stats.Default;

        public ServerState State
        {
            get { ServerState state = (ServerState)m_State; Thread.MemoryBarrier(); return state; }
            private set { m_State = (int)value; Thread.MemoryBarrier(); }
        }

        public string VirtualAddress { get; private set; }

        public void Host(int port)
        {
            throw new NotImplementedException("This method will never be implemented as a VirtualNetServer, use VirtualHost instead.");
        }

        public void VirtualHost(VirtualNetwork network, string address)
        {
            if(State != ServerState.Shutdown)
            {
                throw new InvalidOperationException("VirtualNetServer cannot host as it is not in 'Shutdown' state");
            }
            VirtualAddress = address;

            m_PacketProcessor = new PacketProcessor<VirtualReceiveResult>();
            m_PacketProcessor.Start(new Action<VirtualReceiveResult>[]
            {
                null, // None
                null, // FileTransfer
                null, // WebRequest
                null, // RemoteMethod
                null, // Replication
                null, // Shutdown
                CreateProtocolRoute<ConnectPacket>(ProcessConnectPacket, Protocol.Connect),          // Connect
                CreateProtocolRoute<DisconnectPacket>(ProcessDisconnectPacket, Protocol.Disconnect), // Disconnect
                null  // Acknowledgement
            });

            State = ServerState.Running;
            network.Connect(this);
            m_Socket = network;
        }

        public void Close(ShutdownType shutdownType)
        {
            if (State != ServerState.Running)
            {
                throw new InvalidOperationException("VirtualNetServer cannot close as it is not 'Running'");
            }
            State = ServerState.WaitingForSocket;

            // Create shutdown message
            NetStream ns = new NetStream();
            byte msg = (byte)Protocol.Shutdown;
            ns.Open();
            ns.Serialize(ref msg);
            byte[] msgBytes = ns.Close();

            // Send the message to ourself to unblock the task
            if (!m_Socket.Send(VirtualAddress, this, msgBytes))
            {
                throw new InvalidOperationException("Failed to send shutdown message to self.");
            }

            // Wait for the message to be received then disconnect
            while (State != ServerState.ShuttingDown) { }
            m_PacketProcessor.Stop();
            m_Socket.Disconnect(this);
            VirtualAddress = string.Empty;
            m_Socket = null;
            State = ServerState.Shutdown;
        }

        public void OnReceive(IVirtualNode sender, byte[] data)
        {
            if(sender == null)
            {
                return;
            }

            if(State == ServerState.ShuttingDown)
            {
                return;
            }

            if(data.Length > NetStream.HEADER_SIZE)
            {
                Protocol protocol = (Protocol)data[NetStream.HEADER_SIZE];
                if(protocol == Protocol.Shutdown && State == ServerState.WaitingForSocket)
                {
                    State = ServerState.ShuttingDown;
                    return;
                }
                else
                {
                    if(!m_PacketProcessor.Enqueue(protocol, new VirtualReceiveResult() { Sender = sender, Buffer = data }))
                    {
                        Log.Debug($"Ignoring message from protocol {protocol} from {sender.VirtualAddress}");
                        Interlocked.Increment(ref m_Stats.BadProtocolPackets);
                    }
                }
            }
        }

        private Action<VirtualReceiveResult> CreateProtocolRoute<PacketT>(Action<PacketT, IVirtualNode> callback, Protocol protocol) where PacketT : IProtocolPacket, new()
        {
            return (VirtualReceiveResult result) =>
            {
                PacketT packet = new PacketT();
                if (packet.Read(result.Buffer))
                {
                    callback(packet, result.Sender);
                }
                else
                {
                    OnDiscardCorruptPacket(protocol, result.Buffer, result.Sender);
                }
            };
        }

        protected INetConnection CreateConnection(IVirtualNode endPoint, string identifier)
        {
            lock (m_ConnectionLock)
            {
                if (m_Connections.Count >= m_MaxConnections)
                {
                    return null;
                }

                INetConnection connection = new VirtualNetConnection(endPoint, identifier, Guid.NewGuid().ToByteArray());
                m_Connections.Add(connection);

                Log.Debug($"Register connection: ID={identifier}, UID={new Guid(connection.UID).ToString()}, IP={endPoint.VirtualAddress}");
                return connection;
            }
        }

        protected INetConnection CloseConnection(byte[] uid)
        {
            INetConnection connection = null;
            lock (m_ConnectionLock)
            {
                for (int i = 0; i < m_Connections.Count; ++i)
                {

                    // Compare:
                    bool equal = true;
                    for (int k = 0; k < m_Connections[i].UID.Length; ++k)
                    {
                        if (m_Connections[i].UID[k] != uid[k])
                        {
                            equal = false;
                            break;
                        }
                    }

                    if (equal)
                    {
                        connection = m_Connections[i];
                        m_Connections.RemoveAt(i);
                    }
                }
            }
            if (connection != null)
            {
                Log.Debug($"Closing connection: ID={connection.Identifier}, UID={new Guid(connection.UID).ToString()}, IP={connection.EndPointString}");
            }

            return connection;
        }

        private void OnDiscardCorruptPacket(Protocol protocol, byte[] bytes, IVirtualNode endPoint)
        {
            Log.Debug($"Discarding corrupt packet. Protocol={protocol}, Bytes={bytes.Length}, IP={endPoint.VirtualAddress}");
        }

        protected virtual void ProcessConnectPacket(ConnectPacket packet, IVirtualNode sender)
        {
            INetConnection connection = CreateConnection(sender, packet.Identifier);
            if((packet.Flags & ProtocolFlags.Reliable) > 0)
            {
                ConnectAckPacket connectAck = new ConnectAckPacket() { UID = connection.UID };
                NetStream ns = new NetStream();
                ns.Open();
                connectAck.NetSerialize(ns);
                byte[] connectAckData = ns.Close();

                AcknowledgePacket ack = new AcknowledgePacket()
                {
                    ProtocolType = Protocol.Acknowledgement,
                    Flags = ProtocolFlags.None,
                    Crc32 = 0,
                    UID = packet.UID,
                    Data = connectAckData
                };
                byte[] bytes = ack.Write();
                if(!m_Socket.Send(sender.VirtualAddress, this, bytes))
                {
                    Log.Error($"Failed to send packet. Protocol={ack.ProtocolType}, Target={sender.VirtualAddress}");
                }
            }
        }

        private void ProcessDisconnectPacket(DisconnectPacket packet, IVirtualNode sender)
        {
            if(packet.ConnectionUID.Length == 16)
            {
                CloseConnection(packet.ConnectionUID);
            }
            if((packet.Flags & ProtocolFlags.Reliable) > 0)
            {
                AcknowledgePacket ack = new AcknowledgePacket()
                {
                    ProtocolType = Protocol.Acknowledgement,
                    Flags = ProtocolFlags.None,
                    Crc32 = 0,
                    UID = packet.UID,
                    Data = new byte[0]
                };

                byte[] bytes = ack.Write();
                if(!m_Socket.Send(sender.VirtualAddress, this, bytes))
                {
                    Log.Error($"Failed to send packet. Protocol={ack.ProtocolType}, Target={sender.VirtualAddress}");
                }
            }
        }
    }
}
