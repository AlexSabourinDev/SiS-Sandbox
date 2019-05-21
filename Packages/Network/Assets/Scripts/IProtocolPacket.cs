namespace Game.Networking
{
    public interface IProtocolPacket
    {
        bool Read(byte[] bytes);
    }
}
