namespace Game.Networking
{
    public interface IVirtualNode
    {
        string VirtualAddress { get; }

        void OnReceive(IVirtualNode sender, byte[] data);
    }
}
