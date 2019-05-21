using System;

namespace Game.Networking
{
    public static class NetUtil
    {
        public const int Crc32Offset = 6;

        /// <summary>
        /// Normally its safe enough to check if an object is null if it's not a Unity Object.
        /// If it's a unity object the underlying object may be null but the C# reference is kept alive.
        /// Using the implicit boolean operator we can check if the Unity Object is null.
        /// </summary>
        public static bool IsNull(object arg)
        {
            if(arg == null)
            {
                return true;
            }

#if UNITY_ENGINE
            if(arg is UnityEngine.Object)
            {
                // todo: May need additional checks to make sure this is thread-safe.. or even 'Unity Thread Safe'
                return (UnityEngine.Object)arg;
            }
#endif
            return false;
        }

        public static uint SignCrc32Packet(byte[] packetData)
        {
            if (packetData == null || packetData.Length < (Crc32Offset + NetStream.HEADER_SIZE))
            {
                throw new InvalidOperationException("Packet cannot calculate Crc32 if the data has not been written yet.");
            }

            uint crc32 = Crc32Algorithm.Compute(packetData, Crc32Offset + NetStream.HEADER_SIZE);
            UIntUnion crc = new UIntUnion() { Value = crc32 };
            packetData[NetStream.HEADER_SIZE + 2] = crc.B0;
            packetData[NetStream.HEADER_SIZE + 3] = crc.B1;
            packetData[NetStream.HEADER_SIZE + 4] = crc.B2;
            packetData[NetStream.HEADER_SIZE + 5] = crc.B3;
            return crc32;
        }

        public static bool VerifyCrc32Packet(byte[] packetData)
        {
            if (packetData == null || packetData.Length < (Crc32Offset + NetStream.HEADER_SIZE))
            {
                throw new InvalidOperationException("Packet cannot verify Crc32 if the data has not been written yet.");
            }

            uint crc32 = Crc32Algorithm.Compute(packetData, Crc32Offset + NetStream.HEADER_SIZE);
            UIntUnion crc32Bytes = new UIntUnion()
            {
                B0 = packetData[NetStream.HEADER_SIZE + 2],
                B1 = packetData[NetStream.HEADER_SIZE + 3],
                B2 = packetData[NetStream.HEADER_SIZE + 4],
                B3 = packetData[NetStream.HEADER_SIZE + 5]
            };

            return crc32 == crc32Bytes.Value;
        }
    }
}
