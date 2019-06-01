namespace Game.Networking
{
    public interface INetServer
    {
        /// <summary>
        /// Returns the state of the server.
        /// </summary>
        ServerState State { get; }
        /// <summary>
        /// Returns how many connections the server is handling
        /// </summary>
        int ConnectionCount { get; }
        /// <summary>
        /// Controls how long a client must remain in contact with the server before they
        /// are timed out.
        /// </summary>
        long TimeoutMilliseconds { get; set; }
        /// <summary>
        /// Attempts to open a socket to send/recv data on a 'port'
        /// </summary>
        void Host(int port);
        /// <summary>
        /// Closes the socket using the behavior based on the 'shutdownType'
        /// </summary>
        void Close(ShutdownType shutdownType);

        void Update();
    }
}
