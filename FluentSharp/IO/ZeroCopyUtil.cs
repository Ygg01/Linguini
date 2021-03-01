#nullable enable

using System;

namespace FluentSharp.IO
{
    public static class ZeroCopyUtil
    {
        private const int CharLength = 1;
        private static ReadOnlyMemory<char> _eof = new(new char[] {'\0'});
        public static ReadOnlySpan<char> ReadCharFromMemory(this ReadOnlyMemory<char> memory, int pos)
        {
            if (pos + CharLength > memory.Length)
            {
                return _eof.Span;
            }
            
            return memory.Slice(pos,CharLength).Span;
        }

        public static bool Equals(this char lhs, ReadOnlySpan<char> rhs)
        {
            if (rhs.Length != 1)
            {
                throw new ArgumentException("Expected single character span");
            }
            var chr = rhs[0];
            return lhs.Equals(chr);
        }
    }
}
