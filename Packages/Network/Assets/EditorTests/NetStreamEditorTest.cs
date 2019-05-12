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
    }
}
