using System;

namespace FluentSharp.IO
{
    public class ZeroCopyReader
    {
        private ReadOnlyMemory<char> _unconsumedData;

        public ZeroCopyReader(string text) : this(text.AsMemory())
        {
        }

        public ZeroCopyReader(ReadOnlyMemory<char> memory)
        {
            _unconsumedData = memory;
            _currentPosition = 0;
        }

        private int _currentPosition; 

        public ReadOnlySpan<char> PeekChar()
        {
            return _unconsumedData.ReadCharFromMemory(_currentPosition);
        }

        public ReadOnlySpan<char> GetChar()
        {
            var chr = PeekChar();
            _currentPosition += 1;
            return chr;
        }

        public void SkipBlankBlock()
        {
            while (true)
            {
                var start = _currentPosition;
                SkipBlankInline();
                if (!SkipEol())
                {
                    _currentPosition = start;
                    break;
                }
            }
        }

        private bool SkipEol()
        {
            
        }

        private void SkipBlankInline()
        {
            throw new NotImplementedException();
        }
    }
}
