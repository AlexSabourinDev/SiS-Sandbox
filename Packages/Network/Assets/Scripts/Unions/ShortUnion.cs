using System.Runtime.InteropServices;

namespace Game.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ShortUnion
    {
        [FieldOffset(0)]
        public short Value;
        [FieldOffset(0)]
        public byte B0;
        [FieldOffset(1)]
        public byte B1;
    }
}
