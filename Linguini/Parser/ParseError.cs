using System;
using System.Text;
using Linguini.Ast;

namespace Linguini.Parser
{
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
            var sb = new StringBuilder().AppendFormat("Expected a token starting with  \"{0}\"", chr);

            return new(
                ErrorType.ExpectedToken,
                sb.ToString(),
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError ExpectedCharRange(string range, int pos)
        {
            var sb = new StringBuilder().AppendFormat("Expected one of \"{0}\"", range);

            return new(
                ErrorType.ExpectedCharRange,
                sb.ToString(),
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError ExpectedMessageField(string entryId, int start, int end)
        {
            var sb = new StringBuilder().AppendFormat("Expected a message field for \"{0}\"", entryId);

            return new(
                ErrorType.ExpectedMessageField,
                sb.ToString(),
                new Range(start, end),
                null
            );
        }

        public static ParseError MissingValue(Identifier id, int pos)
        {
            return new(
                ErrorType.MissingValue,
                "Expected a value",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError UnbalancedClosingBrace(int pos)
        {
            return new(
                ErrorType.UnbalancedClosingBrace,
                "Unbalanced closing brace",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError? TermAttributeAsPlaceable(int pos)
        {
            return new(
                ErrorType.TermAttributeAsPlaceable,
                "Term attributes can't be used as a selector",
                new Range(pos, pos + 1),
                null
            );
        }
    }
}
