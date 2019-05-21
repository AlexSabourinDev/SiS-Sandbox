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
            public IVirtualNode Sender { get; set; }
            public byte[] Buffer { get; set; }
        }
        private struct PacketProcessor
        {
            public Thread m_Thread;
            public ConcurrentQueue<VirtualReceiveResult> m_Queue;
        }

        private volatile int m_State = (int)ServerState.Shutdown;
        private VirtualNetwork m_Socket = null;
        private PacketProcessor[] m_PacketProcessors = null;
        private List<INetConnection> m_Connections = new List<INetConnection>();
        private readonly object m_ConnectionLock = new object();
        private int m_MaxConnections = 100;


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

            m_PacketProcessors = new PacketProcessor[NUM_PROTOCOLS];
            m_PacketProcessors[(int)Protocol.FileTransfer] = new PacketProcessor() { m_Thread = new Thread(ProcessFileTransfers), m_Queue = new ConcurrentQueue<VirtualReceiveResult>() };
            m_PacketProcessors[(int)Protocol.RemoteMethod] = new PacketProcessor() { m_Thread = new Thread(ProcessRemoteMethods), m_Queue = new ConcurrentQueue<VirtualReceiveResult>() };
            m_PacketProcessors[(int)Protocol.Replication] = new PacketProcessor() { m_Thread = new Thread(ProcessReplications), m_Queue = new ConcurrentQueue<VirtualReceiveResult>() };
            m_PacketProcessors[(int)Protocol.WebRequest] = new PacketProcessor() { m_Thread = new Thread(ProcessWebRequests), m_Queue = new ConcurrentQueue<VirtualReceiveResult>() };
            m_PacketProcessors[(int)Protocol.Connect] = new PacketProcessor() { m_Thread = new Thread(ProcessConnections), m_Queue = new ConcurrentQueue<VirtualReceiveResult>() };
            m_PacketProcessors[(int)Protocol.Disconnect] = new PacketProcessor() { m_Thread = new Thread(ProcessDisconnect), m_Queue = new ConcurrentQueue<VirtualReceiveResult>() };

            State = ServerState.Running;
            network.Connect(this);
            m_Socket = network;
            for(int i = 0; i < m_PacketProcessors.Length; ++i)
            {
                Thread thread = m_PacketProcessors[i].m_Thread;
                if(thread != null)
                {
                    thread.Name = "Packet_Processor";
                    thread.Start();
                }
            }
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

            for(int i = 0; i < m_PacketProcessors.Length; ++i)
            {
                if(m_PacketProcessors[i].m_Thread != null)
                {
                    m_PacketProcessors[i].m_Thread.Join();
                }
            }

            m_Socket.Disconnect(this);
            VirtualAddress = string.Empty;
            m_Socket = null;
        }

        public void OnReceive(IVirtualNode sender, byte[] data)
        {
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
                else if(protocol != Protocol.None && sender != null)
                {
                    var queue = GetQueue(protocol);
                    if(queue != null)
                    {
                        queue.Enqueue(new VirtualReceiveResult() { Sender = sender, Buffer = data });
                    }
                    else
                    {
                        Log.Debug($"Ignoring message from protocol {protocol} from {sender.VirtualAddress}");
                    }
                }
            }
        }

        private void ProcessFileTransfers()
        {
            Log.Debug("Start processing file transfer packets...");
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
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

            ConcurrentQueue<VirtualReceiveResult> queue = GetQueue(Protocol.RemoteMethod);
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {
                
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
            ConcurrentQueue<VirtualReceiveResult> queue = GetQueue(Protocol.Connect);
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {
                VirtualReceiveResult result;
                if(queue.TryDequeue(out result))
                {
                    ConnectPacket packet = new ConnectPacket();
                    if(packet.Read(result.Buffer))
                    {
                        ProcessConnectPacket(packet, result.Sender);
                    }
                    else
                    {
                        OnDiscardCorruptPacket(Protocol.Connect, result.Buffer, result.Sender);
                    }
                }
            }
            Log.Debug("Stop processing connnect packets...");
        }

        private void ProcessDisconnect()
        {
            Log.Debug("Start processing disconnect packets...");
            ConcurrentQueue<VirtualReceiveResult> queue = GetQueue(Protocol.Disconnect);
            while (State == ServerState.Running || State == ServerState.WaitingForSocket)
            {
                VirtualReceiveResult result;
                if(queue.TryDequeue(out result))
                {
                    DisconnectPacket packet = new DisconnectPacket();
                    if(packet.Read(result.Buffer))
                    {
                        ProcessDisconnectPacket(packet, result.Sender);
                    }
                    else
                    {
                        OnDiscardCorruptPacket(Protocol.Connect, result.Buffer, result.Sender);
                    }
                }
            }
            Log.Debug("Stop processing disconnect packets...");
        }

        private ConcurrentQueue<VirtualReceiveResult> GetQueue(Protocol protocol)
        {
            return m_PacketProcessors[(int)protocol].m_Queue;
        }

        private INetConnection CreateConnection(IVirtualNode endPoint, string identifier)
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

        private INetConnection CloseConnection(byte[] uid)
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

        private void ProcessConnectPacket(ConnectPacket packet, IVirtualNode sender)
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
                if(!m_Socket.Send(sender.VirtualAddress, this, bytes))
                {
                    Log.Error($"Failed to send packet. Protocol={ack.ProtocolType}, Target={sender.VirtualAddress}");
                }
            }
        }
    }
}
