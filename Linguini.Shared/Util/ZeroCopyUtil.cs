using System;

namespace Linguini.Shared.Util
{
    /// <summary>
    /// Utils used for Zero copy parsing.
    /// </summary>
    public static class ZeroCopyUtil
    {
        /// <summary>
        /// Tries to read character from a given position.
        /// </summary>
        /// <param name="memory"><see cref="ReadOnlyMemory{T}"/> of <see cref="char"/> that will be read</param>
        /// <param name="pos">position at which the read will be read will be attempted</param>
        /// <param name="c"><c>out</c> parameter, that is <c>defualt</c> when <c>false</c>; and otherwise the returned character</param>
        /// <returns><c>true</c> if the character is in memory; otherwise, <c>false</c>.</returns>
        public static bool TryReadChar(this ReadOnlyMemory<char> memory, int pos, out char c)
        {
            if (pos >= memory.Length)
            {
                c = default;
                return false;
            }

            c = memory.Span[pos];
            return true;
        }

        /// <summary>
        /// Determines if the specified character is an ASCII alphabetic character.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is an ASCII alphabetic character (<c>'a'</c>-<c>'z'</c> or <c>'A'</c>-<c>'Z'</c>); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAsciiAlphabetic(this char c)
        {
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z');
        }

        /// <summary>
        /// Determines if the specified character is an ASCII for hexadecimal digit.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is an ASCII hexadecimal digit (<c>'a'</c>-<c>'z'</c>, <c>'A'</c>-<c>'Z'</c> or <c>'0'</c>-<c>'9'</c>); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAsciiHexdigit(this char c)
        {
            return IsInside(c, '0', '9')
                   || IsInside(c, 'a', 'f')
                   || IsInside(c, 'A', 'F');
        }

        /// <summary>
        /// Determines if the specified character is ASCII uppercase character.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is ASCII uppercase character (<c>'A'</c>-<c>'Z'</c>); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAsciiUppercase(this char c)
        {
            return IsInside(c, 'A', 'Z');
        }

        /// <summary>
        /// Determines if the specified character is an ASCII number character.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is an ASCII alphabetic character (<c>'0'</c>-<c>'9'</c>); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAsciiDigit(this char c)
        {
            return IsInside(c, '0', '9');
        }

        /// <summary>
        /// Determines if the specified character is a valid ASCII number start.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is an ASCII alphabetic character (<c>'0'</c>-<c>'9'</c> or <c>'-'</c>); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumberStart(this char c)
        {
            return IsInside(c, '0', '9') || c == '-';
        }

        /// <summary>
        /// Determines if the specified character is a valid Fluent callee.
        /// </summary>
        /// <param name="charSpan">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is <see cref="IsAsciiUppercase">ASCII uppercase</see>,
        /// or <see cref="IsAsciiDigit">ASCII digit</see> or one of <c>'_'</c> or <c>'-'</c>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCallee(this ReadOnlySpan<char> charSpan)
        {
            var isCallee = true;
            foreach (var c in charSpan)
            {
                if (!(c.IsAsciiUppercase() || c.IsAsciiDigit() || c.IsOneOf('_', '-')))
                {
                    isCallee = false;
                    break;
                }
            }

            return isCallee;
        }

        /// <summary>
        /// Determines if the specified character is a valid Fluent identifier.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the character is <see cref="IsAsciiUppercase">ASCII uppercase</see>, ASCII lowercase,
        /// an <see cref="IsAsciiDigit">ASCII digit</see> or one of <c>'_'</c> or <c>'-'</c>; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIdentifier(this char c)
        {
            return IsInside(c, 'a', 'z')
                   || IsInside(c, 'A', 'Z')
                   || IsInside(c, '0', '9')
                   || c == '-' || c == '_';
        }

        /// <summary>
        /// Determines whether the character matches one of the two specified characters.
        /// </summary>
        /// <param name="c">The character to compare.</param>
        /// <param name="c1">The first character to compare against.</param>
        /// <param name="c2">The second character to compare against.</param>
        /// <returns><c>true</c> if the character matches either <paramref name="c1"/> or <paramref name="c2"/>; otherwise, <c>false</c>.</returns>
        public static bool IsOneOf(this char c, char c1, char c2)
        {
            return c == c1 || c == c2;
        }

        /// <summary>
        /// Determines whether the character matches one of the three specified characters.
        /// </summary>
        /// <param name="c">The character to compare.</param>
        /// <param name="c1">The first character to compare against.</param>
        /// <param name="c2">The second character to compare against.</param>
        /// <param name="c3">The third character to compare against.</param>
        /// <returns><c>true</c> if the character matches either <paramref name="c1"/> or <paramref name="c2"/> or <paramref name="c3"/>; otherwise, <c>false</c>.</returns>
        public static bool IsOneOf(this char c, char c1, char c2, char c3)
        {
            return c == c1 || c == c2 || c == c3;
        }

        /// <summary>
        /// Determines whether the character matches one of the three specified characters.
        /// </summary>
        /// <param name="c">The character to compare.</param>
        /// <param name="c1">The first character to compare against.</param>
        /// <param name="c2">The second character to compare against.</param>
        /// <param name="c3">The third character to compare against.</param>
        /// <param name="c4">The fourth character to compare against.</param>
        /// <returns><c>true</c> if the character matches either <paramref name="c1"/> or <paramref name="c2"/> or <paramref name="c3"/> or <paramref name="c4"/>; otherwise, <c>false</c>.</returns>
        public static bool IsOneOf(this char c, char c1, char c2, char c3, char c4)
        {
            return c == c1 || c == c2 || c == c3 || c == c4;
        }

#if NETSTANDARD2_1
        // Polyfill for netstandard 2.1 until dotnet backports MemoryExtension
        /// <summary>
        /// Removes trailing whitespace characters from the memory segment.
        /// Acts as a polyfill for the TrimEnd method for environments where it's unavailable.
        /// </summary>
        /// <param name="memory">A <see cref="ReadOnlyMemory{T}"/> of <see cref="char"/> representing the memory segment to be trimmed.</param>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> of <see cref="char"/> with trailing whitespace removed.</returns>
        public static ReadOnlyMemory<char> TrimEndPolyFill(this ReadOnlyMemory<char> memory)
            => memory.Slice(0, FindLastWhitespace(memory.Span));

        private static int FindLastWhitespace(ReadOnlySpan<char> span)
        {
            int end = span.Length - 1;
            for (; end >= 0; end--)
            {
                if (!char.IsWhiteSpace(span[end]))
                {
                    break;
                }
            }

            return end + 1;
        }
#endif

        private static bool IsInside(char c, char min, char max) => (uint) (c - min) <= (uint) (max - min);
    }
}
