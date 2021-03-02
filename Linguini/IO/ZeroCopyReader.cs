using System;

namespace Linguini.IO
{
    public class ZeroCopyReader
    {
        private readonly ReadOnlyMemory<char> _unconsumedData;
        private int _position;

        public ZeroCopyReader(string text) : this(text.AsMemory())
        {
        }

        public ZeroCopyReader(ReadOnlyMemory<char> memory)
        {
            _unconsumedData = memory;
            _position = 0;
        }

        public int Position => _position;
        public bool IsNotEof => _position >= _unconsumedData.Length;
        public bool IsEof => !IsNotEof;

        public ReadOnlySpan<char> PeekCharSpan()
        {
            return _unconsumedData.ReadCharSpan(_position);
        }

        public ReadOnlySpan<char> PeekCharSpan(int offset)
        {
            return _unconsumedData.ReadCharSpan(_position + offset);
        }

        public ReadOnlySpan<char> GetCharSpan()
        {
            var chr = PeekCharSpan();
            _position += 1;
            return chr;
        }

        public int SkipBlankBlock()
        {
            var count = 0;
            while (true)
            {
                var start = _position;
                SkipBlankInline();
                if (!SkipEol())
                {
                    _position = start;
                    break;
                }

                count += 1;
            }

            return count;
        }

        private bool SkipEol()
        {
            if ('\n'.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
                return true;
            }

            if ('\r'.EqualsSpans(PeekCharSpan())
                && '\n'.EqualsSpans(PeekCharSpan(1)))
            {
                _position += 2;
                return true;
            }

            return false;
        }

        private int SkipBlankInline()
        {
            var start = _position;
            while (' '.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
            }

            return _position - start;
        }

    }
}
