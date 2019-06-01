﻿namespace Game.Networking
{
    public enum ClientState
    {
        Shutdown,
        Connecting,
        Connected,
        ConnectionFailed,
        Running,
        WaitingForSocket,
        ShuttingDown
    }
}