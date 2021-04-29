using System;
using Linguini.Shared.IO;

namespace Linguini.Syntax.IO
{
    public class ZeroCopyReader
    {
        private readonly ReadOnlyMemory<char> _unconsumedData;
        private int _position;

        public ZeroCopyReader(string text) : this(text.AsMemory())
        {
        }

        private ZeroCopyReader(ReadOnlyMemory<char> memory)
        {
            _unconsumedData = memory;
            _position = 0;
        }

        public int Position
        {
            get => _position;
            set => _position = value;
        }

        public bool IsNotEof => _position < _unconsumedData.Length;
        public bool IsEof => !IsNotEof;

        public ReadOnlySpan<char> PeekCharSpan(int offset = 0)
        {
            return _unconsumedData.PeakCharAt(_position + offset);
        }

        public ReadOnlySpan<char> PeekCharSpanAt(int pos)
        {
            return _unconsumedData.PeakCharAt(pos);
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

        public bool SkipEol()
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

        public int SkipBlankInline()
        {
            var start = _position;
            while (' '.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
            }

            return _position - start;
        }

        public bool ReadCharIf(char c)
        {
            if (c.EqualsSpans(PeekCharSpan()))
            {
                _position += 1;
                return true;
            }

            return false;
        }

        public ReadOnlyMemory<char> GetCommentLine()
        {
            var startPosition = _position;

            while (!IsEol())
            {
                _position += 1;
            }

            return _unconsumedData.Slice(startPosition, _position - startPosition);
        }

        private bool IsEol()
        {
            var chr = PeekCharSpan();

            if ('\n'.EqualsSpans(chr)) return true;
            if ('\r'.EqualsSpans(chr)
                && '\n'.EqualsSpans(PeekCharSpan(1)))
            {
                return true;
            }

            return IsEof;
        }

        public ReadOnlyMemory<char> ReadSlice(int start, int end)
        {
            return _unconsumedData.Slice(start, end - start);
        }

        public string ReadSliceToStr(int start, int end)
        {
            return new(_unconsumedData.Slice(start, end - start).Span);
        }

        public void SkipToNextEntry()
        {
            while (_unconsumedData.TryReadCharSpan(_position, out var span))
            {
                var newline = _position == 0
                              || '\n'.EqualsSpans(PeekCharSpan(-1));

                if (newline && (span.IsAsciiAlphabetic() || span.IsOneOf('#', '-')))
                {
                    break;
                }

                _position += 1;
            }
        }

        public bool TryPeekCharSpan(out ReadOnlySpan<char> span)
        {
            return _unconsumedData.TryReadCharSpan(_position, out span);
        }

        public void SkipBlank()
        {
            while (TryPeekCharSpan(out var span))
            {
                if (' '.EqualsSpans(span) || '\n'.EqualsSpans(span))
                {
                    _position += 1;
                }
                else if ('\r'.EqualsSpans(span) && '\n'.EqualsSpans(PeekCharSpan(1)))
                {
                    _position += 2;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
