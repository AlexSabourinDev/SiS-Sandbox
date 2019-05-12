using NUnit.Framework;
using System;

namespace Game.Networking
{
    public class UnionEditorTests
    {
        [Test]
        public void VerifyTypeSizes()
        {
            Assert.AreEqual(1, sizeof(byte));
            Assert.AreEqual(1, sizeof(sbyte));
            Assert.AreEqual(2, sizeof(char));
            Assert.AreEqual(2, sizeof(short));
            Assert.AreEqual(2, sizeof(ushort));
            Assert.AreEqual(4, sizeof(int));
            Assert.AreEqual(4, sizeof(uint));
            Assert.AreEqual(8, sizeof(long));
            Assert.AreEqual(8, sizeof(ulong));
            Assert.AreEqual(4, sizeof(float));
            Assert.AreEqual(8, sizeof(double));
        }

        [Test]
        public void SByteUnionTest()
        {
            var union = new SByteUnion() { Value = -48 };
            Assert.AreEqual(0xD0, union.B0);
            Assert.AreEqual(-48, union.Value);
        }

        [Test]  
        public void CharUnionTest()
        {
            var union = new CharUnion() { Value = '\u07F7' };
            Assert.AreEqual(0xF7, union.B0);
            Assert.AreEqual(0x07, union.B1);
            Assert.AreEqual('\u07F7', union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
        }

        [Test]
        public void ShortUnionTest()
        {
            var union = new ShortUnion() { Value = -16820 };
            Assert.AreEqual(0x4C, union.B0);
            Assert.AreEqual(0xBE, union.B1);
            Assert.AreEqual(-16820, union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
        }

        [Test]
        public void UShortUnionTest()
        {
            var union = new UShortUnion() { Value = 0xF33D };
            Assert.AreEqual(0x3D, union.B0);
            Assert.AreEqual(0xF3, union.B1);
            Assert.AreEqual(0xF33D, union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(2, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
        }

        [Test]
        public void IntUnionTest()
        {
            var union = new IntUnion() { Value = -2354920 }; // 0xffdc1118
            Assert.AreEqual(0x18, union.B0);
            Assert.AreEqual(0x11, union.B1);
            Assert.AreEqual(0xDC, union.B2);
            Assert.AreEqual(0xFF, union.B3);
            Assert.AreEqual(-2354920, union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(4, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
            Assert.AreEqual(union.B2, bytes[2]);
            Assert.AreEqual(union.B3, bytes[3]);
        }

        [Test]
        public void UIntUnionTest()
        {
            var union = new UIntUnion() { Value = 0xF33DB4DD };
            Assert.AreEqual(0xDD, union.B0);
            Assert.AreEqual(0xB4, union.B1);
            Assert.AreEqual(0x3D, union.B2);
            Assert.AreEqual(0xF3, union.B3);
            Assert.AreEqual(0xF33DB4DD, union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(4, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
            Assert.AreEqual(union.B2, bytes[2]);
            Assert.AreEqual(union.B3, bytes[3]);
        }

        [Test]
        public void LongUnionTest()
        {
            var union = new LongUnion() { Value = -8421919010202 };
            Assert.AreEqual(0x66, union.B0);
            Assert.AreEqual(0x52, union.B1);
            Assert.AreEqual(0x82, union.B2);
            Assert.AreEqual(0x1E, union.B3);
            Assert.AreEqual(0x57, union.B4);
            Assert.AreEqual(0xF8, union.B5);
            Assert.AreEqual(0xFF, union.B6);
            Assert.AreEqual(0xFF, union.B7);
            Assert.AreEqual(-8421919010202, union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(8, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
            Assert.AreEqual(union.B2, bytes[2]);
            Assert.AreEqual(union.B3, bytes[3]);
            Assert.AreEqual(union.B4, bytes[4]);
            Assert.AreEqual(union.B5, bytes[5]);
            Assert.AreEqual(union.B6, bytes[6]);
            Assert.AreEqual(union.B7, bytes[7]);
        }

        [Test]
        public void ULongUnionTest()
        {
            var union = new ULongUnion() { Value = 0xF33DB4DDAE66014C };
            Assert.AreEqual(0x4C, union.B0);
            Assert.AreEqual(0x01, union.B1);
            Assert.AreEqual(0x66, union.B2);
            Assert.AreEqual(0xAE, union.B3);
            Assert.AreEqual(0xDD, union.B4);
            Assert.AreEqual(0xB4, union.B5);
            Assert.AreEqual(0x3D, union.B6);
            Assert.AreEqual(0xF3, union.B7);
            Assert.AreEqual(0xF33DB4DDAE66014C, union.Value);

            Assert.IsTrue(BitConverter.IsLittleEndian);
            byte[] bytes = BitConverter.GetBytes(union.Value);
            Assert.AreEqual(8, bytes.Length);
            Assert.AreEqual(union.B0, bytes[0]);
            Assert.AreEqual(union.B1, bytes[1]);
            Assert.AreEqual(union.B2, bytes[2]);
            Assert.AreEqual(union.B3, bytes[3]);
            Assert.AreEqual(union.B4, bytes[4]);
            Assert.AreEqual(union.B5, bytes[5]);
            Assert.AreEqual(union.B6, bytes[6]);
            Assert.AreEqual(union.B7, bytes[7]);
        }

        [Test]
        public void FloatUnionTest()
        {
            Assert.IsTrue(BitConverter.IsLittleEndian);

            byte[] bytes = BitConverter.GetBytes(2.234981060028076171875f);
            var union = new FloatUnion() { Value = 2.234981060028076171875f };
            Assert.AreEqual(0x40, union.B3);
            Assert.AreEqual(0x0F, union.B2);
            Assert.AreEqual(0x09, union.B1);
            Assert.AreEqual(0xEE, union.B0);

            Assert.AreEqual(bytes[0], union.B0);
            Assert.AreEqual(bytes[1], union.B1);
            Assert.AreEqual(bytes[2], union.B2);
            Assert.AreEqual(bytes[3], union.B3);

            union = new FloatUnion() { B3 = 0x40, B2 = 0x0F, B1 = 0x09, B0 = 0xEE };
            Assert.AreEqual(2.234981060028076171875f, union.Value);
        }
        
        [Test]
        public void DoubleUnionTest()
        {
            Assert.IsTrue(BitConverter.IsLittleEndian);

            byte[] bytes = BitConverter.GetBytes(2.234981060028076171875);
            Assert.AreEqual(8, bytes.Length);

            var union = new DoubleUnion() { Value = 2.234981060028076171875 }; // 0x4001E13DC0000000 
            Assert.AreEqual(0x00, union.B0);
            Assert.AreEqual(0x00, union.B1);
            Assert.AreEqual(0x00, union.B2);
            Assert.AreEqual(0xC0, union.B3);
            Assert.AreEqual(0x3D, union.B4);
            Assert.AreEqual(0xE1, union.B5);
            Assert.AreEqual(0x01, union.B6);
            Assert.AreEqual(0x40, union.B7);

            Assert.AreEqual(bytes[0], union.B0);
            Assert.AreEqual(bytes[1], union.B1);
            Assert.AreEqual(bytes[2], union.B2);
            Assert.AreEqual(bytes[3], union.B3);
            Assert.AreEqual(bytes[4], union.B4);
            Assert.AreEqual(bytes[5], union.B5);
            Assert.AreEqual(bytes[6], union.B6);
            Assert.AreEqual(bytes[7], union.B7);

            union = new DoubleUnion() { B0 = 0x00, B1 = 0x00, B2 = 0x00, B3 = 0xC0, B4 = 0x3D, B5 = 0xE1, B6 = 0x01, B7 = 0x40 };
            Assert.AreEqual(2.234981060028076171875, union.Value);
        }
    }
}
