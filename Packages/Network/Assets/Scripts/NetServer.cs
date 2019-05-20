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

        private struct PacketProcessor
        {
            public Thread m_Thread;
            public ConcurrentQueue<UdpReceiveResult> m_Queue;
        }

        private UdpClient m_Socket = null;
        private int m_Port = 0;
        private volatile int m_State = (int)ServerState.Shutdown;
        private Task<UdpReceiveResult> m_ActiveTask = null;
        private PacketProcessor[] m_PacketProcessors = null;
        private List<INetConnection> m_Connections = new List<INetConnection>();
        private readonly object m_ConnectionLock = new object();
        private int m_MaxConnections = 100;

        public ServerState State
        {
            get { ServerState state = (ServerState)m_State; Thread.MemoryBarrier(); return state; }
            private set { m_State = (int)value; Thread.MemoryBarrier(); }
        }

        private ConcurrentQueue<UdpReceiveResult> GetQueue(Protocol protocol)
        {
            return m_PacketProcessors[(int)protocol].m_Queue;
        }


        public void Host(int port)
        {
            if(State != ServerState.Shutdown)
            {
                throw new InvalidOperationException("NetServer cannot host as it is not in 'Shutdown' state");
            }

            m_PacketProcessors = new PacketProcessor[NUM_PROTOCOLS];
            m_PacketProcessors[(int)Protocol.FileTransfer] = new PacketProcessor() { m_Thread = new Thread(ProcessFileTransfers), m_Queue = new ConcurrentQueue<UdpReceiveResult>() };
            m_PacketProcessors[(int)Protocol.RemoteMethod] = new PacketProcessor() { m_Thread = new Thread(ProcessRemoteMethods), m_Queue = new ConcurrentQueue<UdpReceiveResult>() };
            m_PacketProcessors[(int)Protocol.Replication] = new PacketProcessor() { m_Thread = new Thread(ProcessReplications), m_Queue = new ConcurrentQueue<UdpReceiveResult>() };
            m_PacketProcessors[(int)Protocol.WebRequest] = new PacketProcessor() { m_Thread = new Thread(ProcessWebRequests), m_Queue = new ConcurrentQueue<UdpReceiveResult>() };
            m_PacketProcessors[(int)Protocol.Connect] = new PacketProcessor() { m_Thread = new Thread(ProcessConnections), m_Queue = new ConcurrentQueue<UdpReceiveResult>() };
            m_PacketProcessors[(int)Protocol.Disconnect] = new PacketProcessor() { m_Thread = new Thread(ProcessDisconnect), m_Queue = new ConcurrentQueue<UdpReceiveResult>() };

            m_Port = port;
            m_Socket = new UdpClient(port);
            State = ServerState.Running;
            BeginReceive();
            for (int i = 0; i < m_PacketProcessors.Length; ++i)
            {
                Thread thread = m_PacketProcessors[i].m_Thread;
                if (thread != null)
                {
                    thread.Name = "Packet_Processor";
                    thread.Start();
                }
            }
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

            m_Socket.Close();
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
            if(udpResult.Buffer.Length > NetStream.HEADER_SIZE)
            {
                Protocol protocol = (Protocol)udpResult.Buffer[NetStream.HEADER_SIZE];
                if(protocol == Protocol.Shutdown && State == ServerState.WaitingForSocket)
                {
                    Log.Debug("Shutdown message received!");
                    State = ServerState.ShuttingDown;
                    return;
                }
                else if(protocol != Protocol.None)
                {
                    var queue = GetQueue(protocol);
                    if(queue != null)
                    {
                        queue.Enqueue(udpResult);
                    }
                    else
                    {
                        string address = udpResult.RemoteEndPoint.ToString();
                        Log.Debug($"Ignoring message from protocol {protocol} from {address}");
                    }
                }
            }

            // Receive more data:
            BeginReceive();
        }

        private void ProcessFileTransfers()
        {
            Log.Debug("Start processing file transfer packets...");
            while(State == ServerState.Running || State == ServerState.WaitingForSocket)
            {

            }
            Log.Debug("Stop processing file transfer packets...");
        }

        private void ProcessWebRequests()
        {
            Log.Debug("Start processing file transfer packets...");
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {

            }
            Log.Debug("Stop processing web request packets...");
        }

        private void ProcessRemoteMethods()
        {
            Log.Debug("Start processing remote method packets...");

            ConcurrentQueue<UdpReceiveResult> queue = GetQueue(Protocol.RemoteMethod);
            while(State == ServerState.Running || State == ServerState.WaitingForSocket)
            {
                UdpReceiveResult result;
                if(queue.TryDequeue(out result))
                {
                    byte[] buffer = result.Buffer;

                    RemoteMethodPacket packet = new RemoteMethodPacket();
                    if(packet.Read(buffer))
                    {
                        // todo: Dispatch to replicator

                        // todo: Handle network transport flags
                    }
                }
            }
            Log.Debug("Stop processing remote method packets...");
        }

        private void ProcessReplications()
        {
            Log.Debug("Start processing replication packets...");
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {

            }
            Log.Debug("Stop processing replication packets...");
        }

        private void ProcessConnections()
        {
            Log.Debug("Start processing connnect packets...");
            ConcurrentQueue<UdpReceiveResult> queue = GetQueue(Protocol.Connect);
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {
                UdpReceiveResult result;
                if(queue.TryDequeue(out result))
                {
                    ConnectPacket packet = new ConnectPacket();
                    if(packet.Read(result.Buffer))
                    {
                        Log.Debug($"Accepting connection {packet.Identifier}");
                        INetConnection connection = CreateConnection(result.RemoteEndPoint, packet.Identifier);

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
                            int bytesSent = m_Socket.Send(bytes, bytes.Length, result.RemoteEndPoint);
                            if(bytes.Length != bytesSent)
                            {
                                Log.Error($"Failed to send packet. Protocol={ack.ProtocolType}, Target={result.RemoteEndPoint.ToString()}, Expected={bytes.Length}, Sent={bytesSent}");
                            }
                        }
                    }
                    else
                    {
                        OnDiscardCorruptPacket(Protocol.Disconnect, result.Buffer, result.RemoteEndPoint);
                    }
                }
            }
            Log.Debug("Stop processing connnect packets...");
        }

        private void ProcessDisconnect()
        {
            Log.Debug("Start processing disconnect packets...");
            ConcurrentQueue<UdpReceiveResult> queue = GetQueue(Protocol.Disconnect);
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {
                UdpReceiveResult result;
                if(queue.TryDequeue(out result))
                {
                    DisconnectPacket packet = new DisconnectPacket();
                    if (packet.Read(result.Buffer))
                    {
                        INetConnection connection = CloseConnection(packet.ConnectionUID);
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
                            int bytesSent = m_Socket.Send(bytes, bytes.Length, result.RemoteEndPoint);
                            if(bytes.Length != bytesSent)
                            {
                                Log.Error($"Failed to send packet. Protocol={ack.ProtocolType}");
                            }
                        }
                    }
                    else
                    {
                        OnDiscardCorruptPacket(Protocol.Disconnect, result.Buffer, result.RemoteEndPoint);
                    }
                }
            }
            Log.Debug("Stop processing disconnect packets...");
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
