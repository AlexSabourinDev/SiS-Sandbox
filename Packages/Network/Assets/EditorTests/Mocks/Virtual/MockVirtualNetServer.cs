﻿using System.Text;
using Game.Util;

namespace Game.Networking
{
    public class MockVirtualNetServer : VirtualNetServer
    {
        public int BadProtocolPackets { get { return m_Stats.BadProtocolPackets; } }


        public enum ProcessConnectBehavior
        {
            // Use the normal connection behavior
            Normal,
            // Create the connection but respond with a corrupted acknowledgement
            CorruptAck,
            // Create the connection on the server but don't acknowledge the client
            IgnoreAck,
            // Do nothing
            Ignore
        }
        public ProcessConnectBehavior m_ProcessConnectBehavior = ProcessConnectBehavior.Normal;
        protected override void ProcessConnectPacket(ConnectPacket packet, IVirtualNode sender)
        {
            switch(m_ProcessConnectBehavior)
            {
                case ProcessConnectBehavior.Normal:
                    {
                        base.ProcessConnectPacket(packet, sender);
                    } break;
                case ProcessConnectBehavior.CorruptAck:
                    {
                        INetConnection connection = CreateConnection(sender, packet.Identifier);
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

                            // Simulate either data corruption or hacker tampering with data and not recalculating the crc32.
                            byte[] bytes = ack.Write();
                            bytes[27]++;

                            if (!m_Socket.Send(sender.VirtualAddress, this, bytes))
                            {
                                Log.Error($"Failed to send packet. Protocol={ack.ProtocolType}, Target={sender.VirtualAddress}");
                            }
                        }
                    } break;
                case ProcessConnectBehavior.IgnoreAck:
                    {
                        CreateConnection(sender, packet.Identifier);
                    } break;
                case ProcessConnectBehavior.Ignore:
                    {
                        
                    } break;
            }
        }


        public bool HasConnection(string identifier)
        {
            return m_Connections.Exists((INetConnection connection) =>
            {
                return connection.Identifier == identifier;
            });
        }

        public bool HasConnection(byte[] uid)
        {
            return m_Connections.Exists((INetConnection connection) =>
            {
                if(connection.UID.Length != uid.Length)
                {
                    return false;
                }

                bool allEqual = true;
                for(int i = 0; i < uid.Length; ++i)
                {
                    if(uid[i] != connection.UID[i])
                    {
                        allEqual = false;
                        break;
                    }
                }
                return allEqual;
            });
        }

        public string GetConnectionUID(string identifier)
        {
            var item = m_Connections.Find((INetConnection connection) =>
            {
                return connection.Identifier == identifier;
            });
            if(item == null)
            {
                return string.Empty;
            }
            return Encoding.ASCII.GetString(item.UID);
        }
    }
}
