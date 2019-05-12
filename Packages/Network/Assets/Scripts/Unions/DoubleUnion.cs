using System.Runtime.InteropServices;

namespace Game.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DoubleUnion
    {
        [FieldOffset(0)]
        private long Integer;
        [FieldOffset(0)]
        public double Value;
        [FieldOffset(0)]
        public byte B0;
        [FieldOffset(1)]
        public byte B1;
        [FieldOffset(2)]
        public byte B2;
        [FieldOffset(3)]
        public byte B3;
        [FieldOffset(4)]
        public byte B4;
        [FieldOffset(5)]
        public byte B5;
        [FieldOffset(6)]
        public byte B6;
        [FieldOffset(7)]
        public byte B7;

        public static double ToDouble(long value)
        {
            return new DoubleUnion() { Integer = value }.Value;
        }

        public static long ToInteger(double value)
        {
            return new DoubleUnion() { Value = value }.Integer;
        }
    }
}
