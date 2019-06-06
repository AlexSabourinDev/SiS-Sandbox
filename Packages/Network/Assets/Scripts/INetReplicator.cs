namespace Game.Networking
{
    public interface INetReplicator
    {
        void ProcessPacket(RemoteMethodPacket packet);
        void Allocate(IReplicated obj);
        void Free(IReplicated obj);
    }
}
