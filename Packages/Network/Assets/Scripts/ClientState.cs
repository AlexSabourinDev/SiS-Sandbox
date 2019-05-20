namespace Game.Networking
{
    public enum ClientState
    {
        Shutdown,
        Running,
        WaitingForSocket,
        ShuttingDown
    }
}