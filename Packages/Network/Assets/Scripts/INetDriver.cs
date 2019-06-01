namespace Game.Networking
{
    public interface INetDriver
    {
        /// <summary>
        /// Returns true if the NetDriver is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the NetDriver up as a server creating a socket that 
        /// will send/recv data on the 'port'
        /// 
        /// Calling this function while the NetDriver is running will result 
        /// in an InvalidOperationException
        /// </summary>
        void Host(int port);
        /// <summary>
        /// Starts the NetDriver up as a client creating a socket that
        /// attempts to connect to the specified 'ipAddress' and 'port'
        /// 
        /// Calling this function while the NetDriver is running will result 
        /// in an InvalidOperationException
        /// </summary>
        void Connect(string ipAddress, int port);
        /// <summary>
        /// Closes the connection on the socket using the behavior defined 
        /// by the 'shutdownType'
        /// 
        /// Calling this function while the NetDriver is running will result 
        /// in an InvalidOperationException
        /// </summary>
        void Close(ShutdownType shutdownType);

        INetClient GetClient();
        INetServer GetServer();


        // void ReplicateObject(IReplicated replicatedObject);
        // void DestroyObject(IReplicated replicatedObject);
    }
}
