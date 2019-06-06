namespace Game.Networking
{
    public enum Protocol
    {
        None,
        FileTransfer,
        WebRequest,
        RemoteMethod,
        Replication,
        Shutdown,
        Connect,
        Disconnect,
        Acknowledgement
    }
}