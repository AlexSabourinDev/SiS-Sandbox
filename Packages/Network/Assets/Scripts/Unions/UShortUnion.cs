using System.Runtime.InteropServices;

namespace Game.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UShortUnion
    {
        [FieldOffset(0)]
        public ushort Value;
        [FieldOffset(0)]
        public byte B0;
        [FieldOffset(1)]
        public byte B1;
    }
}
