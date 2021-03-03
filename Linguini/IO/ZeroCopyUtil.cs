#nullable enable

using System;
using System.Runtime.InteropServices;

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

        public static bool EqualsSpans(this char chr, ReadOnlySpan<char> chrSpan)
        {
            if (chrSpan.Length > CharLength)
            {
                throw new ArgumentException("Expected single character charSpan");
            }

            return chrSpan.Length != 0 && IsEqual(chrSpan, chr);
        }

        public static bool EqualsSpans(this char? lhs, ReadOnlySpan<char> chrSpan)
        {
            if (lhs == null)
            {
                return chrSpan == Eof.Span;
            }

            return EqualsSpans((char) lhs, chrSpan);
        }
        
        private static bool IsEqual(this ReadOnlySpan<char> charSpan, char c1)
        {
            if (charSpan.Length != CharLength)
            {
                throw new ArgumentException("Character span must be exactly one `char` wide");
            }
            return MemoryMarshal.GetReference(charSpan) == c1;
        }
        
        public static bool IsOneOf(this ReadOnlySpan<char> charSpan, char c1, char c2)
        {
            if (charSpan.Length != CharLength)
            {
                throw new ArgumentException("Character span must be exactly one `char` wide");
            }
            var x = MemoryMarshal.GetReference(charSpan);
            return x == c1 || x == c2;
        }

        public static bool IsAlphaNumeric(this ReadOnlySpan<char> charSpan)
        {
            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z')
                   || IsInside(c, '0', '9');
        }

        private static bool IsInside(char c, char min, char max) => (uint) (c - min) <= (uint) (max - min);
        
    }
}
