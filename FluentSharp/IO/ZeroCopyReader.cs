using System;

namespace FluentSharp.IO
{
    public class ZeroCopyReader
    {
        private ReadOnlyMemory<char> _unconsumedData;
        private int _currentPosition;

        public ZeroCopyReader(string text) : this(text.AsMemory())
        {
        }

        public ZeroCopyReader(ReadOnlyMemory<char> memory)
        {
            _unconsumedData = memory;
            _currentPosition = 0;
        }
        
        public ReadOnlySpan<char> PeekChar()
        {
            return _unconsumedData.ReadCharFromMemory(_currentPosition);
        }
        
        public ReadOnlySpan<char> PeekChar(int offset)
        {
            return _unconsumedData.ReadCharFromMemory(_currentPosition + offset);
        }

        public ReadOnlySpan<char> GetChar()
        {
            var chr = PeekChar();
            _currentPosition += 1;
            return chr;
        }

        public int SkipBlankBlock()
        {
            var count = 0;
            while (true)
            {
                var start = _currentPosition;
                SkipBlankInline();
                if (!SkipEol())
                {
                    _currentPosition = start;
                    break;
                }

                count += 1;
            }

            return count;
        }

        private bool SkipEol()
        {
            if ('\n'.Equals(PeekChar()))
            {
                _currentPosition += 1;
                return true;
            }

            if ('\r'.Equals(PeekChar())
                && '\n'.Equals(PeekChar(1)))
            {
                _currentPosition += 2;
                return true;
            }

            return false;
        }

        private int SkipBlankInline()
        {
            var start = _currentPosition;
            while (' '.Equals(PeekChar()))
            {
                _currentPosition += 1;
            }

            return _currentPosition - start;
        }

        public bool IsNotEof()
        {
            return _currentPosition >= _unconsumedData.Length;
        }
    }
}
