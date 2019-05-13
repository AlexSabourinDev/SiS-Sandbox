using System;
using System.Collections.Generic;

namespace Game.Networking
{
    /// <summary>
    /// A class that facilitates reading/writing to a 'stream' of data in a binary format
    /// 
    /// Buffer Encoding:
    /// Bit Buffer Length : 4 bytes
    ///       Byte Buffer : Buffer.Length - (4 + Bit Buffer Length)
    ///        Bit Buffer : Bit Buffer Length
    ///     
    /// Example: A netstream that encoded 28 bytes and 37 bits ( ceil(37 / 8) : 5 bytes )
    ///   Stream Length: 37 bytes
    ///   Stream Buffer: [....|............................|.....]
    ///                    ^                ^                 ^---- Bit Buffer
    ///                    |                +---------------------- Byte Buffer
    ///                    +--------------------------------------- Bit Buffer Length
    ///           
    /// todo: Currently we assume byte order to be little endian, there needs to be checks for big endian and a way to convert 
    ///       to little endian. (Since big endian isn't as popular anymore and I don't believe windows supports it i'll defer doing 
    ///       that till later.)
    /// </summary>
    public class NetStream
    {
        private const string READ_EXCEPTION_MSG = "The NetStream cannot read bytes off the end of the byte-buffer.";

        // Value for the 'last' index of a bitmask which has the length of a byte.
        protected const int BIT_CURSOR_END = 7;
        // Value for the 'one before first' index of the bit mask.
        protected const int BIT_CURSOR_REND = -1;
        // Bitmask table we when reading/writing bits to the bit buffer.
        private readonly int[] BIT_MASK = new int[]
                {
                    0x00, // NULL_WRITE
                    0x01, //   00000001
                    0x03, //   00000011
                    0x07, //   00000111
                    0x0F, //   00001111
                    0x1F, //   00011111
                    0x3F, //   00111111
                    0x7F, //   01111111
                    0xFF  // FULL WRITE
                };

        // A container of bytes written or 'parsed' and read from.
        protected List<byte> m_ByteBuffer = new List<byte>();
        protected int m_ByteBufferReadCursor = 0;

        // A container of bits written or 'parsed' and read from
        protected List<byte> m_BitBuffer = new List<byte>();
        // The current bitmask being read/written to.
        protected byte m_WorkingBits = 0;
        // The cursor in the 'BitBuffer' that we read the next 'Working Byte' from
        protected int m_BitBufferReadCursor = 0;
        // The cursor in the 'Working Byte' for which we read the next bit(s) from.
        protected int m_BitCursor = 0;

        // Just a state that is set when the stream is 'opened'
        protected bool m_IsReading = false;


        /// <summary>
        /// Returns the state of the stream on whether or not it is reading data from the stream or writing data to the stream.
        /// </summary>
        public bool IsReading { get { return m_IsReading; } private set { m_IsReading = value; } }
        /// <summary>
        /// Returns the bytes that would be returned when the stream closes
        /// </summary>
        public int Count { get { return m_ByteBuffer.Count + m_BitBuffer.Count + (m_BitCursor == BIT_CURSOR_END ? 0 : 1) + 4; } }


        /// <summary>
        /// Opens the stream for reading or writing (that depends on the argument 'data')
        /// 
        /// If the 'data' is null then the stream is opened for write
        /// otherwise its opened for read.
        /// </summary>
        /// <param name="data">Data that will be written to objects calling any of the Serialize functions</param>
        public void Open(byte[] data = null)
        {
            if (data == null)
            {
                m_ByteBuffer = new List<byte>();
                m_ByteBuffer.Capacity = 1024;

                m_BitBuffer = new List<byte>();
                m_BitBuffer.Capacity = 8;
            }
            else
            {
                m_ByteBuffer = new List<byte>();
                m_BitBuffer = new List<byte>();
                if (data.Length >= 4)
                {
                    int bitBufferLength = data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];
                    int byteBufferLength = data.Length - (4 + bitBufferLength);
                    m_ByteBuffer.Capacity = byteBufferLength;
                    m_BitBuffer.Capacity = bitBufferLength;

                    int cursor = 4;
                    for (int i = 0; i < byteBufferLength; ++i)
                    {
                        m_ByteBuffer.Add(data[cursor++]);
                    }
                    for (int i = 0; i < bitBufferLength; ++i)
                    {
                        m_BitBuffer.Add(data[cursor++]);
                    }
                }
            }

            // Set Mode:
            IsReading = data != null;
            m_ByteBufferReadCursor = 0;
            m_BitBufferReadCursor = 0;
            m_WorkingBits = 0;
            if (IsReading)
            {
                m_BitCursor = BIT_CURSOR_REND;
            }
            else
            {
                m_ByteBuffer.Capacity = 1024;
                m_BitBuffer.Capacity = 8;
                m_BitCursor = BIT_CURSOR_END;
            }
        }

        /// <summary>
        /// Closes the stream resetting it's internal state.
        /// 
        /// If the stream was opened for write then the 'working bits' are flushed
        /// into the bit buffer and the function returns an encoded array of bytes.
        /// </summary>
        /// <returns>The bytes written if opened for write otherwise null</returns>
        public byte[] Close()
        {
            byte[] result = null;
            if (!IsReading)
            {
                FlushBits();
                int bitBufferLength = m_BitBuffer.Count;
                result = new byte[m_ByteBuffer.Count + bitBufferLength + 4];
                result[0] = (byte)((bitBufferLength >> 24) & 0xFF);
                result[1] = (byte)((bitBufferLength >> 16) & 0xFF);
                result[2] = (byte)((bitBufferLength >> 8) & 0xFF);
                result[3] = (byte)(bitBufferLength & 0xFF);

                int cursor = 4;
                for (int i = 0; i < m_ByteBuffer.Count; ++i)
                {
                    result[cursor++] = m_ByteBuffer[i];
                }
                for (int i = 0; i < m_BitBuffer.Count; ++i)
                {
                    result[cursor++] = m_BitBuffer[i];
                }
            }

            m_ByteBuffer = null;
            m_ByteBufferReadCursor = -1;
            m_BitBuffer = null;
            m_WorkingBits = 0;
            m_BitCursor = BIT_CURSOR_REND;
            m_BitBufferReadCursor = -1;
            IsReading = false;
            return result;
        }

        /// <summary>
        /// Serializes a boolean value as a single bit.
        /// </summary>
        /// <param name="value">The value to be read/written</param>
        public void Serialize(ref bool value)
        {
            if (IsReading)
            {
                if (m_BitCursor < 0)
                {
                    FetchBits();
                }
                byte mask = (byte)(1 << m_BitCursor);
                byte bit = (byte)(m_WorkingBits & mask);
                value = bit > 0 ? true : false;
                --m_BitCursor;
            }
            else
            {
                byte bit = (byte)((value ? 1 : 0) << m_BitCursor);
                m_WorkingBits |= bit;
                --m_BitCursor;
                if (m_BitCursor < 0)
                {
                    FlushBits();
                }
            }
        }

        /// <summary>
        /// Reads/Writes a single byte to the stream.
        /// </summary>
        /// <param name="value">The value to be read/written</param>
        public void Serialize(ref byte value)
        {
            if (IsReading)
            {
                if (m_ByteBufferReadCursor >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                value = m_ByteBuffer[m_ByteBufferReadCursor++];
            }
            else
            {
                m_ByteBuffer.Add(value);
            }
        }

        /// <summary>
        /// Reads/Writes a fixed number of bits of the value to the stream.
        /// 
        /// note:
        ///     If bits >= sizeof(value) then the data is serialized as if called Serialize
        /// </summary>
        /// <param name="value">The value to be read/written</param>
        /// <param name="bits">The number of bits to serialize</param>
        public void SerializeBits(ref byte value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }

            if (bits >= 8)
            {
                Serialize(ref value);
                return;
            }

            if (IsReading)
            {
                if (m_BitCursor < 0)
                {
                    FetchBits();
                }

                int readableBits = m_BitCursor + 1;
                if (readableBits >= bits)
                {
                    int shift = readableBits - (int)bits;
                    value = (byte)((m_WorkingBits >> shift) & BIT_MASK[bits]);
                    m_BitCursor -= (int)bits;
                }
                else
                {
                    // Read One:
                    value = (byte)(m_WorkingBits & BIT_MASK[readableBits]);
                    FetchBits();
                    // Read Remainder:
                    int remainderBits = (int)bits - readableBits;
                    int shift = (m_BitCursor + 1) - remainderBits;
                    value = (byte)((value << remainderBits) | ((m_WorkingBits >> shift) & BIT_MASK[remainderBits]));
                    m_BitCursor -= (int)remainderBits;
                }
            }
            else
            {
                int writeableBits = m_BitCursor + 1;
                if (writeableBits >= bits)
                {
                    int shift = writeableBits > bits ? (m_BitCursor - (int)bits) + 1 : 0;
                    byte bitmask = (byte)((value & BIT_MASK[bits]) << shift);
                    m_WorkingBits |= bitmask;
                    m_BitCursor -= (int)bits;
                    if (m_BitCursor < 0)
                    {
                        FlushBits();
                    }
                }
                else
                {
                    // Write One:
                    int remainderBits = (int)bits - writeableBits;
                    byte bitmask = (byte)((value >> remainderBits) & BIT_MASK[writeableBits]);
                    m_WorkingBits |= bitmask;
                    FlushBits();

                    // Write Two: 'Remainder'
                    bitmask = (byte)((value & BIT_MASK[remainderBits]) << ((m_BitCursor - (int)remainderBits)) + 1);
                    m_WorkingBits |= bitmask;
                    m_BitCursor -= remainderBits;
                }
            }
        }
        
        public void Serialize(ref sbyte value)
        {
            var union = new SByteUnion();
            if(IsReading)
            {
                Serialize(ref union.B0);
                value = union.Value;
            }
            else
            {
                union.Value = value;
                Serialize(ref union.B0);
            }
        }

        public void Serialize(ref char value)
        {
            var union = new CharUnion();
            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(char)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(char);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
            }
        }

        public void Serialize(ref short value)
        {
            var union = new ShortUnion();
            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(short)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(short);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
            }
        }

        public void Serialize(ref ushort value)
        {
            var union = new UShortUnion();
            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(ushort)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(ushort);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
            }
        }

        /// <summary>
        /// Serializes a byte value
        /// </summary>
        /// <param name="value">The value to be read/written</param>
        public void Serialize(ref int value)
        {
            var union = new IntUnion();

            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(int)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                union.B2 = m_ByteBuffer[m_ByteBufferReadCursor + 2];
                union.B3 = m_ByteBuffer[m_ByteBufferReadCursor + 3];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(int);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
                m_ByteBuffer.Add(union.B2);
                m_ByteBuffer.Add(union.B3);
            }
        }

        public void Serialize(ref uint value)
        {
            var union = new UIntUnion();

            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(uint)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                union.B2 = m_ByteBuffer[m_ByteBufferReadCursor + 2];
                union.B3 = m_ByteBuffer[m_ByteBufferReadCursor + 3];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(uint);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
                m_ByteBuffer.Add(union.B2);
                m_ByteBuffer.Add(union.B3);
            }
        }

        public void Serialize(ref long value)
        {
            var union = new LongUnion();

            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(long)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                union.B2 = m_ByteBuffer[m_ByteBufferReadCursor + 2];
                union.B3 = m_ByteBuffer[m_ByteBufferReadCursor + 3];
                union.B4 = m_ByteBuffer[m_ByteBufferReadCursor + 4];
                union.B5 = m_ByteBuffer[m_ByteBufferReadCursor + 5];
                union.B6 = m_ByteBuffer[m_ByteBufferReadCursor + 6];
                union.B7 = m_ByteBuffer[m_ByteBufferReadCursor + 7];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(long);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
                m_ByteBuffer.Add(union.B2);
                m_ByteBuffer.Add(union.B3);
                m_ByteBuffer.Add(union.B4);
                m_ByteBuffer.Add(union.B5);
                m_ByteBuffer.Add(union.B6);
                m_ByteBuffer.Add(union.B7);
            }
        }

        public void Serialize(ref ulong value)
        {
            var union = new ULongUnion();

            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(ulong)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                union.B2 = m_ByteBuffer[m_ByteBufferReadCursor + 2];
                union.B3 = m_ByteBuffer[m_ByteBufferReadCursor + 3];
                union.B4 = m_ByteBuffer[m_ByteBufferReadCursor + 4];
                union.B5 = m_ByteBuffer[m_ByteBufferReadCursor + 5];
                union.B6 = m_ByteBuffer[m_ByteBufferReadCursor + 6];
                union.B7 = m_ByteBuffer[m_ByteBufferReadCursor + 7];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(ulong);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
                m_ByteBuffer.Add(union.B2);
                m_ByteBuffer.Add(union.B3);
                m_ByteBuffer.Add(union.B4);
                m_ByteBuffer.Add(union.B5);
                m_ByteBuffer.Add(union.B6);
                m_ByteBuffer.Add(union.B7);
            }
        }

        public void Serialize(ref float value)
        {
            var union = new FloatUnion();

            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(float)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                union.B2 = m_ByteBuffer[m_ByteBufferReadCursor + 2];
                union.B3 = m_ByteBuffer[m_ByteBufferReadCursor + 3];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(float);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
                m_ByteBuffer.Add(union.B2);
                m_ByteBuffer.Add(union.B3);
            }
        }

        public void Serialize(ref double value)
        {
            var union = new DoubleUnion();

            if (IsReading)
            {
                if ((m_ByteBufferReadCursor + sizeof(double)) >= m_ByteBuffer.Count)
                {
                    throw new IndexOutOfRangeException(READ_EXCEPTION_MSG);
                }

                union.B0 = m_ByteBuffer[m_ByteBufferReadCursor + 0];
                union.B1 = m_ByteBuffer[m_ByteBufferReadCursor + 1];
                union.B2 = m_ByteBuffer[m_ByteBufferReadCursor + 2];
                union.B3 = m_ByteBuffer[m_ByteBufferReadCursor + 3];
                union.B4 = m_ByteBuffer[m_ByteBufferReadCursor + 4];
                union.B5 = m_ByteBuffer[m_ByteBufferReadCursor + 5];
                union.B6 = m_ByteBuffer[m_ByteBufferReadCursor + 6];
                union.B7 = m_ByteBuffer[m_ByteBufferReadCursor + 7];
                value = union.Value;

                m_ByteBufferReadCursor += sizeof(double);
            }
            else
            {
                union.Value = value;
                m_ByteBuffer.Add(union.B0);
                m_ByteBuffer.Add(union.B1);
                m_ByteBuffer.Add(union.B2);
                m_ByteBuffer.Add(union.B3);
                m_ByteBuffer.Add(union.B4);
                m_ByteBuffer.Add(union.B5);
                m_ByteBuffer.Add(union.B6);
                m_ByteBuffer.Add(union.B7);
            }
        }

        public void SerializeBits(ref sbyte value, uint bits)
        {
            if(bits == 0)
            {
                return;
            }
            if(bits >= 8)
            {
                Serialize(ref value);
                return;
            }
            var union = new SByteUnion() { Value = value };
            SerializeBits(ref union.B0, bits);
            value = union.Value;
        }

        public void SerializeBits(ref short value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 16)
            {
                Serialize(ref value);
                return;
            }
            var union = new ShortUnion() { Value = value };
            if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        public void SerializeBits(ref ushort value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 16)
            {
                Serialize(ref value);
                return;
            }
            var union = new UShortUnion() { Value = value };
            if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        public void SerializeBits(ref int value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 32)
            {
                Serialize(ref value);
                return;
            }
            var union = new IntUnion() { Value = value };
            if(bits > 24)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                SerializeBits(ref union.B3, GetRemainderBits(32, bits));
            }
            else if(bits > 16)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                SerializeBits(ref union.B2, GetRemainderBits(24, bits));
            }
            else if(bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        public void SerializeBits(ref uint value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 32)
            {
                Serialize(ref value);
                return;
            }
            var union = new UIntUnion() { Value = value };
            if (bits > 24)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                SerializeBits(ref union.B3, GetRemainderBits(32, bits));
            }
            else if (bits > 16)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                SerializeBits(ref union.B2, GetRemainderBits(24, bits));
            }
            else if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        public void SerializeBits(ref long value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 64)
            {
                Serialize(ref value);
                return;
            }
            var union = new LongUnion() { Value = value };
            if(bits > 56)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                Serialize(ref union.B5);
                Serialize(ref union.B6);
                SerializeBits(ref union.B7, GetRemainderBits(64, bits));
            }
            else if(bits > 48)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                Serialize(ref union.B5);
                SerializeBits(ref union.B6, GetRemainderBits(56, bits));
            }
            else if(bits > 40)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                SerializeBits(ref union.B5, GetRemainderBits(48, bits));
            }
            else if(bits > 32)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                SerializeBits(ref union.B4, GetRemainderBits(40, bits));
            }
            if (bits > 24)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                SerializeBits(ref union.B3, GetRemainderBits(32, bits));
            }
            else if (bits > 16)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                SerializeBits(ref union.B2, GetRemainderBits(24, bits));
            }
            else if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        public void SerializeBits(ref ulong value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 64)
            {
                Serialize(ref value);
                return;
            }
            var union = new ULongUnion() { Value = value };
            if (bits > 56)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                Serialize(ref union.B5);
                Serialize(ref union.B6);
                SerializeBits(ref union.B7, GetRemainderBits(64, bits));
            }
            else if (bits > 48)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                Serialize(ref union.B5);
                SerializeBits(ref union.B6, GetRemainderBits(56, bits));
            }
            else if (bits > 40)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                SerializeBits(ref union.B5, GetRemainderBits(48, bits));
            }
            else if (bits > 32)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                SerializeBits(ref union.B4, GetRemainderBits(40, bits));
            }
            if (bits > 24)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                SerializeBits(ref union.B3, GetRemainderBits(32, bits));
            }
            else if (bits > 16)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                SerializeBits(ref union.B2, GetRemainderBits(24, bits));
            }
            else if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        public void SerializeBits(ref float value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 32)
            {
                Serialize(ref value);
                return;
            }
            var union = new FloatUnion() { Value = value };
            if (bits > 24)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                SerializeBits(ref union.B3, GetRemainderBits(32, bits));
            }
            else if (bits > 16)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                SerializeBits(ref union.B2, GetRemainderBits(24, bits));
            }
            else if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bits"></param>
        public void SerializeBits(ref double value, uint bits)
        {
            if (bits == 0)
            {
                return;
            }
            if (bits >= 64)
            {
                Serialize(ref value);
                return;
            }
            var union = new DoubleUnion() { Value = value };
            if (bits > 56)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                Serialize(ref union.B5);
                Serialize(ref union.B6);
                SerializeBits(ref union.B7, GetRemainderBits(64, bits));
            }
            else if (bits > 48)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                Serialize(ref union.B5);
                SerializeBits(ref union.B6, GetRemainderBits(56, bits));
            }
            else if (bits > 40)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                Serialize(ref union.B4);
                SerializeBits(ref union.B5, GetRemainderBits(48, bits));
            }
            else if (bits > 32)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                Serialize(ref union.B3);
                SerializeBits(ref union.B4, GetRemainderBits(40, bits));
            }
            if (bits > 24)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                Serialize(ref union.B2);
                SerializeBits(ref union.B3, GetRemainderBits(32, bits));
            }
            else if (bits > 16)
            {
                Serialize(ref union.B0);
                Serialize(ref union.B1);
                SerializeBits(ref union.B2, GetRemainderBits(24, bits));
            }
            else if (bits > 8)
            {
                Serialize(ref union.B0);
                SerializeBits(ref union.B1, GetRemainderBits(16, bits));
            }
            else
            {
                SerializeBits(ref union.B0, bits);
            }
            value = union.Value;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalBits"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        private uint GetRemainderBits(uint totalBits, uint bits)
        {
            return 8 - (totalBits - bits);
        }

        /// <summary>
        /// Pops the next bitfield off the bit buffer.
        /// </summary>
        private void FetchBits()
        {
            if (m_BitBufferReadCursor >= m_BitBuffer.Count)
            {
                return; // todo: Throw exception?
            }
            m_WorkingBits = m_BitBuffer[m_BitBufferReadCursor++];
            m_BitCursor = BIT_CURSOR_END;
        }

        /// <summary>
        /// Pushes the current bitfield into the bit buffer.
        /// </summary>
        private void FlushBits()
        {
            if (IsReading)
            {
                return;
            }
            m_BitBuffer.Add(m_WorkingBits);
            m_BitCursor = BIT_CURSOR_END;
            m_WorkingBits = 0;
        }


    }
}
