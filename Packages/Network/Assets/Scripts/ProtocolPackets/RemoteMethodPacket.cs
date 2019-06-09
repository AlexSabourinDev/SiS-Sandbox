using System;

namespace Game.Networking
{
    public struct RemoteMethodPacket
    {
        public const int Crc32Offset = 6;
        /// <summary>
        /// Protocol of the packet
        /// </summary>
        public Protocol ProtocolType;
        /// <summary>
        /// Flags associated with the packet
        /// </summary>
        public ProtocolFlags Flags;
        /// <summary>
        /// Crc32 for the pack, used for data integrity check.
        /// </summary>
        public uint Crc32;
        /// <summary>
        /// The target object to invoke the method on
        /// </summary>
        public int ObjectID;
        /// <summary>
        /// The target method to invoke
        /// </summary>
        public uint MethodID;
        /// <summary>
        /// Binary data of the argument being passed to the method
        /// </summary>
        public byte[] Data;

        public byte[] Write()
        {
            NetStream ns = new NetStream();
            ns.Open();
            NetSerialize(ns);
            byte[] bytes = ns.Close();
            Crc32 = NetUtil.SignCrc32Packet(bytes);
            return bytes;
        }

        public bool Read(byte[] bytes)
        {
            if(!NetUtil.VerifyCrc32Packet(bytes))
            {
                return false;
            }
            NetStream ns = new NetStream();
            ns.Open(bytes);
            NetSerialize(ns);
            ns.Close();
            return true;
        }

        /// <summary>
        /// Serializes all data related to the packet information
        /// 
        /// note: You must manually serialize the argument
        /// </summary>
        private void NetSerialize(NetStream ns)
        {
            byte type = (byte)ProtocolType;
            byte flags = (byte)Flags;

            ns.Serialize(ref type);
            ns.Serialize(ref flags);
            ns.Serialize(ref Crc32);
            ns.Serialize(ref ObjectID);
            ns.Serialize(ref MethodID);
            ns.Serialize(ref Data);

            if (ns.IsReading)
            {
                ProtocolType = (Protocol)type;
                Flags = (ProtocolFlags)flags;
            }
        }
    }
}
