namespace Game.Networking
{
    public class MockVirtualNetServer : VirtualNetServer
    {
        public int BadProocolPackets { get { return m_Stats.BadProtocolPackets; } }


        public enum ProcessConnectBehavior
        {
            // Use the normal connection behavior
            Normal,
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
                case ProcessConnectBehavior.IgnoreAck:
                    {
                        CreateConnection(sender, packet.Identifier);
                    } break;
                case ProcessConnectBehavior.Ignore:
                    {
                        
                    } break;
            }
        }
    }
}
