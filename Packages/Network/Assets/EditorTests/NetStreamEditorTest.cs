using System.Collections.Generic;
using NUnit.Framework;

namespace Game.Networking
{
    public class NetStreamEditorTest
    {
        public class MockNetStream : NetStream
        {
            public List<byte> ByteBuffer { get { return m_ByteBuffer; } }
            public int ByteBufferReadCursor { get { return m_ByteBufferReadCursor; } }
            public List<byte> BitBuffer { get { return m_BitBuffer; } }
            public byte WorkingBits { get { return m_WorkingBits; } }
            public int BitBufferReadCursor { get { return m_BitBufferReadCursor; } }
            public int BitCursor { get { return m_BitCursor; } }

            public int WorkingBitLength { get { return BitCursor < 0 ? 0 : CursorEnd - BitCursor; } }

            public int CursorEnd { get { return BIT_CURSOR_END; } }
            public int CursorRend { get { return BIT_CURSOR_REND; } }
        }

        public struct StubObject
        {
            public byte   U8;
            public sbyte  S8;
            public char   S16Char;
            public short  S16Short;
            public ushort U16Short;
            public int    S32;
            public uint   U32;
            public long   S64;
            public ulong  U64;
            public float  F32;
            public double D64;

            public byte     U8_6;
            public sbyte    S8_5;
            public short    S16_15;
            public ushort   U16_12;
            public int      S32_31;
            public uint     U32_28;
            public long     S64_55;
            public ulong    U64_40;
            public float    F32_18;
            public double   D64_24;

            public void PopulateDefault()
            {
                U8 = 0x6A;
                S8 = -48;
                S16Char = '\u07F7';
                S16Short = -16820;
                U16Short = 0xF33D;
                S32 = -2354920;
                U32 = 0xF33DB4DD;
                S64 = -8421919010202;
                U64 = 0xF33DB4DDAE66014C;
                F32 = 2.234981060028076171875f;
                D64 = 2.234981060028076171875;

                                                   // --- Overflow ----  | --- Clamped --- |
                U8_6 = 0x6A;                       //               0x6A              0x2A
                S8_5 = -48;                        //               0xD0              0x10
                S16_15 = -16820;                   //             0xBE4C            0x3E4C 
                U16_12 = 0xF33D;                   //             0xF33D             0x33D
                S32_31 = -2354920;                 //         0xFFDC1118        0x7FDC1118
                U32_28 = 0xF33DB4DD;               //         0xF33DB4DD         0x33DB4DD
                S64_55 = -8421919010202;           // 0x6652821E57F8FFFF  0xFFF8571E825266 -> 0x7FF8571E825266
                U64_40 = 0xF33DB4DDAE66014C;       // 0xF33DB4DDAE66014C      0xDDAE66014C
                F32_18 = 2.234981060028076171875f; //         0x400F09EE           0x309EE
                D64_24 = 2.234981060028076171875;  // 0x4001E13DC0000000               0x0
            }

            public void PopulateCorrect()
            {
                U8 = 0x6A;
                S8 = -48;
                S16Char = '\u07F7';
                S16Short = -16820;
                U16Short = 0xF33D;
                S32 = -2354920;
                U32 = 0xF33DB4DD;
                S64 = -8421919010202;
                U64 = 0xF33DB4DDAE66014C;
                F32 = 2.234981060028076171875f;
                D64 = 2.234981060028076171875;


                U8_6 = 0x2A;
                S8_5 = 0x10;
                S16_15 = 0x3E4C;
                U16_12 = 0x33D;
                S32_31 = 0x7FDC1118;
                U32_28 = 0x33DB4DD;
                S64_55 = 0x7FF8571E825266;      
                U64_40 = 0xDDAE66014C;
                F32_18 = FloatUnion.ToFloat(0x309EE);
                D64_24 = DoubleUnion.ToDouble(0x0);
            }

            public void NetSerialize(NetStream ns)
            {
                ns.Serialize(ref U8);
                ns.Serialize(ref S8);
                ns.Serialize(ref S16Char);
                ns.Serialize(ref S16Short);
                ns.Serialize(ref U16Short);
                ns.Serialize(ref S32);
                ns.Serialize(ref U32);
                ns.Serialize(ref S64);
                ns.Serialize(ref U64);
                ns.Serialize(ref F32);
                ns.Serialize(ref D64);

                ns.SerializeBits(ref U8_6, 6);
                ns.SerializeBits(ref S8_5, 5);
                ns.SerializeBits(ref S16_15, 15);
                ns.SerializeBits(ref U16_12, 12);
                ns.SerializeBits(ref S32_31, 31);
                ns.SerializeBits(ref U32_28, 28);
                ns.SerializeBits(ref S64_55, 55);
                ns.SerializeBits(ref U64_40, 40);
                ns.SerializeBits(ref F32_18, 18);
                ns.SerializeBits(ref D64_24, 24);
            }
        }

        

        [Test]
        public void SerializeWriteBits()
        {
            byte mask = 0xAA;
            byte value = 0xAB;
            /////// Writing less or equal bits
            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 2);

                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 1);             // changes
                Assert.AreEqual(3, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA0, ns.WorkingBits);      // changes
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 2);

                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 2);
                Assert.AreEqual(4, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xB0, ns.WorkingBits);      // changes
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 2);

                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 3);
                Assert.AreEqual(5, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x98, ns.WorkingBits);      // changes
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 2);

                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 4);
                Assert.AreEqual(6, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xAC, ns.WorkingBits);      // changes
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 2);

                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 5);
                Assert.AreEqual(7, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x96, ns.WorkingBits);      // changes
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 2);

                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 6);
                Assert.AreEqual(0, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(1, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xAB, ns.BitBuffer[0]);      // changes
                Assert.AreEqual(0, ns.WorkingBits);
                Assert.AreEqual(0xAB, value);
            }

            /////// Writing more bits than available.
            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 4);

                Assert.AreEqual(4, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA0, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 5);
                Assert.AreEqual(1, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(1, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA5, ns.BitBuffer[0]);      // changes
                Assert.AreEqual(0x80, ns.WorkingBits);
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 4);

                Assert.AreEqual(4, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA0, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 6);
                Assert.AreEqual(2, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(1, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xAA, ns.BitBuffer[0]);      // changes
                Assert.AreEqual(0xC0, ns.WorkingBits);
                Assert.AreEqual(0xAB, value);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 4);

                Assert.AreEqual(4, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA0, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 7);
                Assert.AreEqual(3, ns.WorkingBitLength);    // changes
                Assert.AreEqual(0, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(1, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA5, ns.BitBuffer[0]);      // changes
                Assert.AreEqual(0x60, ns.WorkingBits);
                Assert.AreEqual(0xAB, value);
            }

            /////// Writing bits but easier to write a byte...
            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                Assert.IsFalse(ns.IsReading);

                ns.SerializeBits(ref mask, 4);

                Assert.AreEqual(4, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA0, ns.WorkingBits);
                Assert.AreEqual(0xAA, mask);

                ns.SerializeBits(ref value, 8);
                Assert.AreEqual(4, ns.WorkingBitLength);    // changes
                Assert.AreEqual(1, ns.ByteBuffer.Count);    // changes
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(0, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xAB, ns.ByteBuffer[0]);      // changes
                Assert.AreEqual(0xA0, ns.WorkingBits);
                Assert.AreEqual(0xAB, value);
            }
        }

        [Test]
        public void SerializeReadBits()
        {
            byte[] sourceSimple = null;  // 10101101
            byte[] sourceOverflow = null;// 10101001 11

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                byte b0 = 0xA;
                byte b1 = 0xD;
                ns.SerializeBits(ref b0, 4);
                ns.SerializeBits(ref b1, 4);
                sourceSimple = ns.Close();
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open();
                byte b0 = 0xA;
                byte b1 = 0x27;
                ns.SerializeBits(ref b0, 4);
                ns.SerializeBits(ref b1, 6);
                sourceOverflow = ns.Close();
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open(sourceSimple);
                Assert.IsTrue(ns.IsReading);
                Assert.AreEqual(0, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.IsTrue(1 == ns.BitBuffer.Count || (2 == ns.BitBuffer.Count && ns.BitBuffer[1] == 0));
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xAD, ns.BitBuffer[0]);

                byte b0 = 0;
                byte b1 = 0;
                ns.SerializeBits(ref b0, 4);
                Assert.AreEqual(4, ns.WorkingBitLength);

                ns.SerializeBits(ref b1, 4);
                Assert.AreEqual(0, ns.WorkingBitLength);
                Assert.AreEqual(0xA, b0);
                Assert.AreEqual(0xD, b1);
            }

            {
                MockNetStream ns = new MockNetStream();
                ns.Open(sourceOverflow);
                Assert.IsTrue(ns.IsReading);
                Assert.AreEqual(0, ns.WorkingBitLength);
                Assert.AreEqual(0, ns.ByteBuffer.Count);
                Assert.AreEqual(0, ns.ByteBufferReadCursor);
                Assert.AreEqual(2, ns.BitBuffer.Count);
                Assert.AreEqual(0, ns.BitBufferReadCursor);
                Assert.AreEqual(0xA9, ns.BitBuffer[0]);
                Assert.AreEqual(0xC0, ns.BitBuffer[1]);

                byte b0 = 0;
                byte b1 = 0;
                ns.SerializeBits(ref b0, 4);
                Assert.AreEqual(4, ns.WorkingBitLength);

                ns.SerializeBits(ref b1, 6);
                Assert.AreEqual(2, ns.WorkingBitLength);
                Assert.AreEqual(0xA, b0);
                Assert.AreEqual(0x27, b1);
            }
        }

        [Test]
        public void SerializeTest()
        {
            // Create out 'source data'
            StubObject input = new StubObject();
            input.PopulateDefault();
             

            // Write to bytes...
            MockNetStream ns = new MockNetStream();
            ns.Open();
            Assert.IsTrue(!ns.IsReading);
            input.NetSerialize(ns);
            byte[] data = ns.Close();

            // Read bytes back into StubObject
            ns.Open(data);
            Assert.IsTrue(ns.IsReading);
            StubObject output = new StubObject();
            output.NetSerialize(ns);
            ns.Close();

            // Compare with expected values (ie the SerializeBit vars should've been truncated)
            StubObject real = new StubObject();
            real.PopulateCorrect();

            Assert.AreEqual(real.U8, output.U8);
            Assert.AreEqual(real.S8, output.S8);
            Assert.AreEqual(real.S16Char, output.S16Char);
            Assert.AreEqual(real.S16Short, output.S16Short);
            Assert.AreEqual(real.U16Short, output.U16Short);
            Assert.AreEqual(real.S32, output.S32);
            Assert.AreEqual(real.U32, output.U32);
            Assert.AreEqual(real.S64, output.S64);
            Assert.AreEqual(real.U64, output.U64);
            Assert.AreEqual(real.F32, output.F32);
            Assert.AreEqual(real.D64, output.D64);

            Assert.AreEqual(real.U8_6, output.U8_6);
            Assert.AreEqual(real.S8_5, output.S8_5);
            Assert.AreEqual(real.S16_15, output.S16_15);
            Assert.AreEqual(real.U16_12, output.U16_12);
            Assert.AreEqual(real.S32_31, output.S32_31);
            Assert.AreEqual(real.U32_28, output.U32_28);
            Assert.AreEqual(real.S64_55, output.S64_55);
            Assert.AreEqual(real.U64_40, output.U64_40);
            Assert.AreEqual(real.F32_18, output.F32_18);
            Assert.AreEqual(real.D64_24, output.D64_24);
        }
    }
}
