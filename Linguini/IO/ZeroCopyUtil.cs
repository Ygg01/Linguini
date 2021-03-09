#nullable enable

using System;
using System.Runtime.InteropServices;

namespace Linguini.IO
{
    public static class ZeroCopyUtil
    {
        private const int CharLength = 1;
        public static readonly ReadOnlyMemory<char> Eof = ReadOnlyMemory<char>.Empty;

        public static ReadOnlySpan<char> PeakCharAt(this ReadOnlyMemory<char> memory, int pos)
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
                return false;
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
                return false;
            }

            return MemoryMarshal.GetReference(charSpan) == c1;
        }

        public static bool IsAsciiAlphabetic(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z');
        }
        
        public static bool IsAsciiHexdigit(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, '0', '9')
                   || IsInside(c, 'a', 'f')
                   || IsInside(c, 'A', 'F');
        }
        
        public static bool IsAsciiUppercase(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, 'A', 'Z');
        }
        
        public static bool IsAsciiDigit(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, '0', '9');
        }
        
        public static bool IsNumberStart(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, '0', '9') || c == '-';
        }
        
        public static bool IsCallee(this ReadOnlyMemory<char> charSpan)
        {
            bool isCallee = true;
            for (int i = 0; i < charSpan.Length -1; i++)
            {
                var c = charSpan.Slice(i, i+1).Span;
                if (!(c.IsAsciiUppercase()|| c.IsAsciiDigit() || c.IsOneOf('_', '-')))
                {
                    isCallee = false;
                    break;
                }
            }

            return isCallee;
        }


        public static bool IsIdentifier(this ReadOnlySpan<char> charSpan)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var c = MemoryMarshal.GetReference(charSpan);
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z')
                   || IsInside(c, '0', '9')
                   || c == '-' || c == '_';
        }

        public static bool IsOneOf(this ReadOnlySpan<char> charSpan, char c1, char c2)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var x = MemoryMarshal.GetReference(charSpan);
            return x == c1 || x == c2;
        }

        public static bool IsOneOf(this ReadOnlySpan<char> charSpan, char c1, char c2, char c3)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var x = MemoryMarshal.GetReference(charSpan);
            return x == c1 || x == c2 || x == c3;
        }

        public static bool IsOneOf(this ReadOnlySpan<char> charSpan, char c1, char c2, char c3, char c4)
        {
            if (charSpan.Length != CharLength)
            {
                return false;
            }

            var x = MemoryMarshal.GetReference(charSpan);
            return x == c1 || x == c2 || x == c3 || x == c4;
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
