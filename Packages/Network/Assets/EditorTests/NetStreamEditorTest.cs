using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace Game.Networking
{
    public class NetStreamEditorTest
    {
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

            public void FatSerialize(MemoryStream ms, BinaryFormatter fmt)
            {
                fmt.Serialize(ms, U8);
                fmt.Serialize(ms, S8);
                fmt.Serialize(ms, S16Char);
                fmt.Serialize(ms, S16Short);
                fmt.Serialize(ms, U16Short);
                fmt.Serialize(ms, S32);
                fmt.Serialize(ms, U32);
                fmt.Serialize(ms, S64);
                fmt.Serialize(ms, U64);
                fmt.Serialize(ms, F32);
                fmt.Serialize(ms, D64);

                fmt.Serialize(ms, U8_6);
                fmt.Serialize(ms, S8_5);
                fmt.Serialize(ms, S16_15);
                fmt.Serialize(ms, U16_12);
                fmt.Serialize(ms, S32_31);
                fmt.Serialize(ms, U32_28);
                fmt.Serialize(ms, S64_55);
                fmt.Serialize(ms, U64_40);
                fmt.Serialize(ms, F32_18);
                fmt.Serialize(ms, D64_24);
            }

            public void FatSerialize(BinaryWriter bw, MemoryStream ms)
            {
                bw.Write(U8);
                bw.Write(S8);
                bw.Write(S16Char);
                bw.Write(S16Short);
                bw.Write(U16Short);
                bw.Write(S32);
                bw.Write(U32);
                bw.Write(S64);
                bw.Write(U64);
                bw.Write(F32);
                bw.Write(D64);

                bw.Write(U8_6);
                bw.Write(S8_5);
                bw.Write(S16_15);
                bw.Write(U16_12);
                bw.Write(S32_31);
                bw.Write(U32_28);
                bw.Write(S64_55);
                bw.Write(U64_40);
                bw.Write(F32_18);
                bw.Write(D64_24);
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

        [Test]
        public void BugFixReadExactBufferSize()
        {
            uint byte4 = 0xADCAD123;
            ulong byte8 = 0xDEADCAED13374004;

            MockNetStream ns = new MockNetStream();
            ns.Open();
            ns.Serialize(ref byte4);
            ns.Serialize(ref byte8);
            byte[] bytes = ns.Close();

            byte4 = 0;
            byte8 = 0;

            ns.Open(bytes);
            Exception thrownException = null;
            try
            {
                ns.Serialize(ref byte4);
                ns.Serialize(ref byte8);
            }
            catch(Exception exception)
            {
                thrownException = exception;
            }
            ns.Close();

            Assert.AreEqual(null, thrownException);
            Assert.AreEqual(0xADCAD123, byte4);
            Assert.AreEqual(0xDEADCAED13374004, byte8);
        }

        [Test]
        public void SerializeString()
        {
            string message = "~I am a big jam donut~";

            NetStream ns = new NetStream();
            ns.Open();
            ns.Serialize(ref message);
            byte[] bytes = ns.Close();

            string result = string.Empty;
            ns.Open(bytes);
            ns.Serialize(ref result);
            ns.Close();

            Assert.AreEqual(message, result);

        }

        [Test]
        public void CompareMemoryStream()
        {
            const int OBJECT_COUNT = 24;

            long formatterMemoryUsage = 0;
            long writerMemoryUsage = 0;
            long netStreamMemoryUsage = 0;

            long before = 0;
            long after = 0;

            StubObject stub = new StubObject();
            stub.PopulateCorrect();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            before = GC.GetTotalMemory(false);
            byte[] formatterBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter fmt = new BinaryFormatter();
                for(int i = 0; i < OBJECT_COUNT; ++i)
                {
                    stub.FatSerialize(ms, fmt);
                }
                ms.Flush();
                formatterBytes = ms.ToArray();
            }
            after = GC.GetTotalMemory(false);
            formatterMemoryUsage = after - before;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            before = GC.GetTotalMemory(false);
            byte[] writerBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    for(int i = 0; i < OBJECT_COUNT; ++i)
                    {
                        stub.FatSerialize(bw, ms);
                    }
                    ms.Flush();
                    writerBytes = ms.ToArray();
                }
            }
            after = GC.GetTotalMemory(false);
            writerMemoryUsage = after - before;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            before = GC.GetTotalMemory(false);
            byte[] bytes = null;
            NetStream ns = new NetStream();
            ns.Open(null);
            for(int i = 0; i < OBJECT_COUNT; ++i)
            {
                stub.NetSerialize(ns);
            }
            bytes = ns.Close();
            after = GC.GetTotalMemory(false);
            netStreamMemoryUsage = after - before;

            // note: The memory usage stat pretty much means nothing when running in unity, would need a much simpler environment where objects aren't being allocated
            //       on multiple threads.
            Debug.Log($"[CompareMemoryStream] Disregard MemoryUsage if ran from unity as we have no guarantee how much memory is being used in that environment.");
            Debug.Log($"[CompareMemoryStream] BinaryFormatter: BufferLength={formatterBytes.Length}, MemoryUsage={formatterMemoryUsage}");
            Debug.Log($"[CompareMemoryStream]    BinaryWriter: BufferLength={writerBytes.Length}, MemoryUsage={writerMemoryUsage}");
            Debug.Log($"[CompareMemoryStream]       NetStream: BufferLength={bytes.Length}, MemoryUsage={netStreamMemoryUsage}");
            Assert.IsTrue(bytes.Length <= formatterBytes.Length, "NetStream is worse than BinaryFormatter!");
            Assert.IsTrue(bytes.Length <= writerBytes.Length, "NetStream is worse than BinaryWriter!");

        }
    }
}
