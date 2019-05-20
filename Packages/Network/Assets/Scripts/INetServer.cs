namespace Game.Networking
{
    public interface INetServer
    {
        /// <summary>
        /// Returns the state of the server.
        /// </summary>
        ServerState State { get; }
        /// <summary>
        /// Attempts to open a socket to send/recv data on a 'port'
        /// </summary>
        void Host(int port);
        /// <summary>
        /// Closes the socket using the behavior based on the 'shutdownType'
        /// </summary>
        void Close(ShutdownType shutdownType);
    }
}
