namespace Game.Networking
{
    public enum ServerState
    {
        Shutdown,
        Running,
        WaitingForSocket,
        ShuttingDown
    }
}