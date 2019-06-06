namespace Game.Networking
{
    public interface IReplicatedArgument
    {
        void NetSerialize(NetStream ns);
    }
}
