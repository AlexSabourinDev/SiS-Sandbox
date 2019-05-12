using System.Runtime.InteropServices;

namespace Game.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SByteUnion
    {
        [FieldOffset(0)]
        public sbyte Value;
        [FieldOffset(0)]
        public byte B0;
    }
}
