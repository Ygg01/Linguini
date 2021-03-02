#nullable enable

using System;

namespace Linguini.IO
{
    public static class ZeroCopyUtil
    {
        private const int CharLength = 1;
        public static ReadOnlyMemory<char> Eof = ReadOnlyMemory<char>.Empty;

        public static ReadOnlySpan<char> ReadCharSpan(this ReadOnlyMemory<char> memory, int pos)
        {
            memory.TryReadCharSpan(pos, out var span);
            return span;
        }

        public static bool TryReadCharSpan(this ReadOnlyMemory<char> memory, int pos, out ReadOnlySpan<char> span)
        {
            span = Eof.Span;
            if (pos + CharLength > memory.Length)
            {
                return false;
            }

            span = memory.Slice(pos, CharLength).Span;
            return true;
        }

        public static bool EqualsSpans(this char lhs, ReadOnlySpan<char> rhs)
        {
            if (rhs.Length != 1)
            {
                throw new ArgumentException("Expected single character span");
            }

            var chr = rhs[0];
            return lhs.Equals(chr);
        }

        public static bool EqualsSpans(this char? lhs, ReadOnlySpan<char> rhs)
        {
            if (lhs == null)
            {
                return rhs == Eof.Span;
            }

            return EqualsSpans((char) lhs, rhs);
        }
    }
}
