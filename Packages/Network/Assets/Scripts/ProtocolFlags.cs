using System;

namespace Game.Networking
{
    [Flags]
    public enum ProtocolFlags
    {
        None,
        Reliable = 1 << 0,
        Ordered = 1 << 1
    }
}