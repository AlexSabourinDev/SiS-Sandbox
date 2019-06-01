namespace Game.Networking
{
    public class MockVirtualNetClient : VirtualNetClient
    {
        public int BadProtocolPackets { get { return m_Stats.BadProtocolPackets; } }
        public int CorruptAckPackets { get { return m_Stats.CorruptAckPackets; } }
    }
}
