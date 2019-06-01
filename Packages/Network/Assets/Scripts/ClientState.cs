namespace Game.Networking
{
    public enum ClientState
    {
        Shutdown,
        Connecting,
        Connected,
        Running,
        WaitingForSocket,
        ShuttingDown
    }
}