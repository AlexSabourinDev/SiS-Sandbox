using System.Collections.Generic;

namespace Game.Networking
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
}
