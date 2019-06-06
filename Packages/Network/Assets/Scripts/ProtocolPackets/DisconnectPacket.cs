namespace Game.Networking
{
    public struct DisconnectPacket : IProtocolPacket
    {
        public const int Crc32Offset = 6;

        public Protocol ProtocolType;
        public ProtocolFlags Flags;
        public uint Crc32;
        public uint UID;
        public byte[] ConnectionUID;

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

        private void NetSerialize(NetStream ns)
        {
            byte type = (byte)ProtocolType;
            byte flags = (byte)Flags;

            ns.Serialize(ref type);
            ns.Serialize(ref flags);
            ns.Serialize(ref Crc32);
            ns.Serialize(ref UID);
            ns.Serialize(ref ConnectionUID);

            if(ns.IsReading)
            {
                ProtocolType = (Protocol)type;
                Flags = (ProtocolFlags)flags;
            }
        }
    }
}
