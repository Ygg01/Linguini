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

        public static bool CompareContent(this ReadOnlySpan<char> memory, char other)
        {
            if (memory.Length != 1)
            {
                throw new ArgumentException("Expected single character span");
            }
            var lhs = memory[0];
            return other.Equals(lhs);
        }
    }
}
