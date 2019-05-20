namespace Game.Networking
{
    public struct ConnectAckPacket
    {
        public byte[] UID;

        public void NetSerialize(NetStream ns)
        {
            ns.Serialize(ref UID);
        }
    }
}
