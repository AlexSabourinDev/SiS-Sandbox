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
        public uint ObjectID;
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
            CalcCrc32(bytes);
            return bytes;
        }

        public bool Read(byte[] bytes)
        {
            if(!VerifyCrc32(bytes))
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

        private void CalcCrc32(byte[] data)
        {

            if (data == null || data.Length < (Crc32Offset + NetStream.HEADER_SIZE))
            {
                throw new InvalidOperationException("RemoteMethodPacket cannot calculate Crc32 if the data has not been written yet.");
            }

            Crc32 = Crc32Algorithm.Compute(data, Crc32Offset + NetStream.HEADER_SIZE);
            UIntUnion crc = new UIntUnion() { Value = Crc32 };
            data[NetStream.HEADER_SIZE + 2] = crc.B0;
            data[NetStream.HEADER_SIZE + 3] = crc.B1;
            data[NetStream.HEADER_SIZE + 4] = crc.B2;
            data[NetStream.HEADER_SIZE + 5] = crc.B3;
        }

        private bool VerifyCrc32(byte[] data)
        {

            if (data == null || data.Length < (Crc32Offset + NetStream.HEADER_SIZE))
            {
                throw new InvalidOperationException("RemoteMethodPacket cannot verify Crc32 if the data has not been written yet.");
            }

            uint crc32 = Crc32Algorithm.Compute(data, Crc32Offset + NetStream.HEADER_SIZE);
            UIntUnion crc32Bytes = new UIntUnion()
            {
                B0 = data[NetStream.HEADER_SIZE + 2],
                B1 = data[NetStream.HEADER_SIZE + 3],
                B2 = data[NetStream.HEADER_SIZE + 4],
                B3 = data[NetStream.HEADER_SIZE + 5]
            };

            return crc32 == crc32Bytes.Value;
        }
    }
}
