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

        public static ParseError ExpectedMessageField(ReadOnlyMemory<char> entryId, int start, int end)
        {
            var sb = new StringBuilder()
                .Append(@"Expected a message field for """).Append(entryId).Append('"');

            return new(
                ErrorType.ExpectedMessageField,
                sb.ToString(),
                new Range(start, end),
                null
            );
        }

        public static ParseError MissingValue(int pos)
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

        public static ParseError? ExpectedTermField(Identifier id, int start, int end)
        {
            var sb = new StringBuilder()
                .Append("Expected a term field for \"").Append(id.Name).Append("\"");

            return new(
                kind: ErrorType.ExpectedTermField,
                sb.ToString(),
                new Range(start, end),
                null
            );
        }

        public static ParseError MessageReferenceAsSelector(int pos)
        {
            return new(
                ErrorType.MessageReferenceAsSelector,
                "Message references can't be used as a selector",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError MessageAttributeAsSelector(int pos)
        {
            return new(
                ErrorType.MessageAttributeAsSelector,
                "Message attributes can't be used as a selector",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError TermReferenceAsSelector(int pos)
        {
            return new(
                ErrorType.TermReferenceAsSelector,
                "Term references can't be used as a selector",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError ExpectedSimpleExpressionAsSelector(int pos)
        {
            return new(
                ErrorType.ExpectedSimpleExpressionAsSelector,
                "Expected a simple expression as selector",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError UnterminatedStringLiteral(int pos)
        {
            return new(
                ErrorType.UnterminatedStringLiteral,
                "Unterminated string literal",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError UnknownEscapeSequence(char seq, int pos)
        {
            var sb = new StringBuilder()
                .AppendFormat("Unknown escape sequence \"{0}\"", seq);

            return new(
                ErrorType.UnknownEscapeSequence,
                sb.ToString(),
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError ForbiddenCallee(int pos)
        {
            return new(
                ErrorType.ForbiddenCallee,
                "Callee is not allowed here",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError ExpectedLiteral(int pos)
        {
            return new(
                ErrorType.ExpectedLiteral,
                "Expected a string or number literal",
                new Range(pos, pos + 1),
                null
            );
        }

        public static ParseError ExpectedInlineExpression(int pos)
        {
            return new(
                ErrorType.ExpectedInlineExpression,
                "Expected an inline expression",
                new Range(pos, pos + 1),
                null
            );
        }
    }
}
