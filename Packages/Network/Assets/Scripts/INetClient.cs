namespace Game.Networking
{
    public interface INetClient
    {
        /// <summary>
        /// Attempts to establish a connection with a server using the provided 
        /// 'ipAddress' and 'port'.
        /// </summary>
        void Connect(string identifier, string ipAddress, int port);
        /// <summary>
        /// Closes the connection with the server using the behavior determined by 'shutdownType'
        /// </summary>
        /// <param name="shutdownType"></param>
        void Close(ShutdownType shutdownType);
        /// <summary>
        /// Returns the state of the client.
        /// </summary>
        ClientState State { get; }
        /// <summary>
        /// Sends a 'heartbeat' packet to the server to let them know we're still around
        /// this way they won't timeout the connection.
        /// </summary>
        void SendHeartbeat();
    }
}
