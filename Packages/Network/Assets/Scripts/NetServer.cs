using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Game.Util;

namespace Game.Networking
{
    public class NetServer : INetServer
    {
        private static readonly int NUM_PROTOCOLS = Enum.GetValues(typeof(Protocol)).Length;
        private struct ReceiveResult
        {
            public IPEndPoint Sender { get; set; }
            public byte[] Buffer { get; set; }
        }

        private UdpClient m_Socket = null;
        private int m_Port = 0;
        private volatile int m_State = (int)ServerState.Shutdown;
        private Task<UdpReceiveResult> m_ActiveTask = null;
        private List<INetConnection> m_Connections = new List<INetConnection>();
        private readonly object m_ConnectionLock = new object();
        private int m_MaxConnections = 100;
        private PacketProcessor<ReceiveResult> m_Processor = null;

        public ServerState State
        {
            get { ServerState state = (ServerState)m_State; Thread.MemoryBarrier(); return state; }
            private set { m_State = (int)value; Thread.MemoryBarrier(); }
        }

        public void Host(int port)
        {
            if(State != ServerState.Shutdown)
            {
                throw new InvalidOperationException("NetServer cannot host as it is not in 'Shutdown' state");
            }

            m_Processor = new PacketProcessor<ReceiveResult>();
            m_Processor.Start(new Action<ReceiveResult>[]
            {
                null, // None
                null, // FileTransfer
                null, // WebRequest
                null, // RemoteMethod
                null, // Replication
                null, // Shutdown
                CreateProtocolRoute<ConnectPacket>(ProcessConnectPacket, Protocol.Connect), // Connect
                CreateProtocolRoute<DisconnectPacket>(ProcessDisconnectPacket, Protocol.Disconnect), // Disconnect
                null  // Acknowledgement
            });

            m_Port = port;
            m_Socket = new UdpClient(port);
            State = ServerState.Running;
            BeginReceive();
        }

        public void Close(ShutdownType shutdownType)
        {
            if(State != ServerState.Running)
            {
                throw new InvalidOperationException("NetServer cannot close as it is not 'Running'");
            }

            State = ServerState.WaitingForSocket;

            // Create shutdown message
            NetStream ns = new NetStream();
            byte msg = (byte)Protocol.Shutdown;
            ns.Open();
            ns.Serialize(ref msg);
            byte[] msgBytes = ns.Close();

            // Connect as client and pump a shutdown message.
            UdpClient shutdownSocket = new UdpClient("127.0.0.1", m_Port);
            int result = shutdownSocket.Send(msgBytes, msgBytes.Length);

            while(State != ServerState.ShuttingDown)
            {

            }
            m_Processor.Stop();
            m_Processor = null;
            m_Socket.Close();
            m_Connections.Clear();
            m_Socket = null;
            m_Port = 0;
            m_ActiveTask = null;
            // todo: Send/wait depending on ShutdownType
        }


        private void BeginReceive()
        {
            if(State == ServerState.Running)
            {
                m_ActiveTask = m_Socket.ReceiveAsync();
                m_ActiveTask.ContinueWith(OnReceive);
            }
        }
        private void OnReceive(Task<UdpReceiveResult> result)
        {
            if(State == ServerState.ShuttingDown || result.IsCanceled)
            {
                return;
            }

            // Queue data for processing:
            UdpReceiveResult udpResult = result.Result;
            if(udpResult.RemoteEndPoint == null)
            {
                return;
            }

            if(udpResult.Buffer.Length > NetStream.HEADER_SIZE)
            {
                Protocol protocol = (Protocol)udpResult.Buffer[NetStream.HEADER_SIZE];
                if (protocol == Protocol.Shutdown && State == ServerState.WaitingForSocket)
                {
                    State = ServerState.ShuttingDown;
                    return;
                }
                else
                {
                    if(!m_Processor.Enqueue(protocol, new ReceiveResult() { Buffer = udpResult.Buffer, Sender = udpResult.RemoteEndPoint }))
                    {
                        Log.Debug($"Ignoring message from protocol {protocol} from {udpResult.RemoteEndPoint}");
                    }
                }
            }

            // Receive more data:
            BeginReceive();
        }

        private Action<ReceiveResult> CreateProtocolRoute<PacketT>(Action<PacketT, IPEndPoint> callback, Protocol protocol) where PacketT : IProtocolPacket, new()
        {
            return (ReceiveResult result) =>
            {
                PacketT packet = new PacketT();
                if(packet.Read(result.Buffer))
                {
                    Log.Debug($"Processing packet of length {result.Buffer.Length} with protocol {protocol} from {result.Sender.ToString()}");
                    callback(packet, result.Sender);
                }
                else
                {
                    OnDiscardCorruptPacket(protocol, result.Buffer, result.Sender);
                }
            };
        }

        private void SendPacket(byte[] data, IPEndPoint who, Protocol protocol)
        {
            int bytesSent = m_Socket.Send(data, data.Length, who);
            if (bytesSent != data.Length)
            {
                Log.Error($"Failed to send packet. Protocol={protocol}, Target={who}, Expected={data.Length}, Sent={bytesSent}");
            }
        }

        private void ProcessConnectPacket(ConnectPacket packet, IPEndPoint sender)
        {
            INetConnection connection = CreateConnection(sender, packet.Identifier);
            // todo: Error Handling, what if we cannot create a connection?

            if ((packet.Flags & ProtocolFlags.Reliable) > 0)
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
                SendPacket(bytes, sender, ack.ProtocolType);
            }
        }

        

        private void ProcessDisconnectPacket(DisconnectPacket packet, IPEndPoint sender)
        {
            INetConnection connection = CloseConnection(packet.ConnectionUID);
            // todo: Error Handling, what if we are closing an already closed connection?

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
                SendPacket(bytes, sender, ack.ProtocolType);
            }
        }

        private INetConnection CreateConnection(IPEndPoint endPoint, string identifier)
        {
            lock(m_ConnectionLock)
            {
                if(m_Connections.Count >= m_MaxConnections)
                {
                    return null;
                }

                NetConnection connection = new NetConnection(endPoint, identifier, Guid.NewGuid().ToByteArray());
                m_Connections.Add(connection);

                Log.Debug($"Register connection: ID={identifier}, UID={new Guid(connection.UID).ToString()}, IP={endPoint.ToString()}");
                return connection;
            }
        }

        private INetConnection CloseConnection(byte[] uid)
        {
            INetConnection connection = null;
            lock (m_ConnectionLock)
            {
                for(int i = 0; i < m_Connections.Count; ++i)
                {
            
                    // Compare:
                    bool equal = true;
                    for(int k = 0; k < m_Connections[i].UID.Length; ++k)
                    {
                        if(m_Connections[i].UID[k] != uid[k])
                        {
                            equal = false;
                            break;
                        }
                    }
            
                    if(equal)
                    {
                        connection = m_Connections[i];
                        m_Connections.RemoveAt(i);
                    }
                }
            }
            if(connection != null)
            {
                Log.Debug($"Closing connection: ID={connection.Identifier}, UID={new Guid(connection.UID).ToString()}, IP={connection.EndPoint.ToString()}");
            }

            return connection;
        }

        private void OnDiscardCorruptPacket(Protocol protocol, byte[] bytes, IPEndPoint endPoint)
        {
            Log.Debug($"Discarding corrupt packet. Protocol={protocol}, Bytes={bytes.Length}, IP={endPoint.ToString()}");
        }

        // Process Data Flow:
        // Read Protocol:
        // Read NetFlags: (Reliable|ReliableOrder|Unreliable)
        // Read Crc32:
        //    Drop If Crc32 is not valid
        // Dispatch to Protocol Handler
        //
        //
        // Remote Method:
        //   Read 'ObjectID'
        //   Read 'Priority'
        //     ReplicatedObject o = NetReplicator->Find(ObjectID)
        //     o.Enqueue<Packet>(x)


        // NetReplicator.OnUpdate
        //      foreach(object o in obj)
        //          o.Callback.Invoke(o.Instance, packet)

    }
}
