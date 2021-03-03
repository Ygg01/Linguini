using System;
using System.Text;

namespace Linguini.Parser
{
    public enum ErrorType
    {
        ExpectedChar,
    }

    public class ParseError
    {
        public ErrorType Kind { get; private set; }
        public string? Message { get; private set; }
        public Range Position { get; private set; }
        public Range? Slice { get; set; }

        private ParseError(ErrorType kind, string? message, Range position, Range? slice)
        {
            Kind = kind;
            Message = message;
            Position = position;
            Slice = slice;
        }

        public static ParseError ExpectedToken(char chr, int pos)
        {
            var sb = new StringBuilder("Expected char \"").Append(chr).Append("\" .");

            return new ParseError(
                ErrorType.ExpectedChar,
                sb.ToString(),
                new Range(pos, pos + 1),
                null
            );
        }
    }
}
