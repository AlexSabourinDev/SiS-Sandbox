namespace Game.Networking
{
    public enum ShutdownType
    {
        /// <summary>
        /// Connected clients/servers will be sent a disconnect message.
        /// The connected socket will wait for the connected client/server
        /// to acknowledge the message.
        /// </summary>
        NotifyAndWait,
        /// <summary>
        /// Connected clients/servers will be sent a disconnect message,
        /// however the socket will not wait for a acknowledgement.
        /// </summary>
        Notify,
        /// <summary>
        /// Connected clients/servers will not be sent any message the socket will be
        /// closed ASAP.
        /// </summary>
        Immediate
    }
}