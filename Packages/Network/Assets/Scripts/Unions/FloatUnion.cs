using System.Runtime.InteropServices;

namespace Game.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FloatUnion
    {
        [FieldOffset(0)]
        public int Integer;
        [FieldOffset(0)]
        public float Value;
        [FieldOffset(0)]
        public byte B0;
        [FieldOffset(1)]
        public byte B1;
        [FieldOffset(2)]
        public byte B2;
        [FieldOffset(3)]
        public byte B3;

        public static float ToFloat(int value)
        {
            return new FloatUnion() { Integer = value }.Value;
        }

        public static int ToInteger(float value)
        {
            return new FloatUnion() { Value = value }.Integer;
        }
    }
}
