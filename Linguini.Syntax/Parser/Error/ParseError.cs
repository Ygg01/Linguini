using System;
using System.Text;
using Linguini.Syntax.Ast;
using Linguini.Syntax.IO;

namespace Linguini.Syntax.Parser.Error
{
    public class ParseError
    {
        public ErrorType Kind { get; }
        public string Message { get; }
        public int Row { get; set; }
        public Range Position { get; }
        public Range? Slice { get; set; }

        private ParseError(ErrorType kind, string message, Range position)
        {
            Kind = kind;
            Message = message;
            Position = position;
            Slice = null;
            Row = 0;
        }

        public static ParseError ExpectedToken(char chr, int pos)
        {
            var sb = new StringBuilder().AppendFormat("Expected a token starting with  \"{0}\"", chr);

            return new(
                ErrorType.ExpectedToken,
                sb.ToString(),
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ExpectedCharRange(string range, int pos)
        {
            var sb = new StringBuilder().AppendFormat("Expected one of \"{0}\"", range);

            return new(
                ErrorType.ExpectedCharRange,
                sb.ToString(),
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ExpectedMessageField(ReadOnlyMemory<char> entryId, int start, int end)
        {
            var sb = new StringBuilder()
                .Append(@"Expected a message field for """).Append(entryId).Append('"');

            return new(
                ErrorType.ExpectedMessageField,
                sb.ToString(),
                new Range(start, end)
            );
        }

        public static ParseError MissingValue(int pos)
        {
            return new(
                ErrorType.MissingValue,
                "Expected a value",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError UnbalancedClosingBrace(int pos)
        {
            return new(
                ErrorType.UnbalancedClosingBrace,
                "Unbalanced closing brace",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError TermAttributeAsPlaceable(int pos)
        {
            return new(
                ErrorType.TermAttributeAsPlaceable,
                "Term attributes can't be used as a selector",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ExpectedTermField(Identifier id, int start, int end)
        {
            var sb = new StringBuilder()
                .Append("Expected a term field for \"").Append(id.Name).Append("\"");

            return new(
                kind: ErrorType.ExpectedTermField,
                sb.ToString(),
                new Range(start, end)
            );
        }

        public static ParseError MessageReferenceAsSelector(int pos)
        {
            return new(
                ErrorType.MessageReferenceAsSelector,
                "Message references can't be used as a selector",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError MessageAttributeAsSelector(int pos)
        {
            return new(
                ErrorType.MessageAttributeAsSelector,
                "Message attributes can't be used as a selector",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError TermReferenceAsSelector(int pos)
        {
            return new(
                ErrorType.TermReferenceAsSelector,
                "Term references can't be used as a selector",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ExpectedSimpleExpressionAsSelector(int pos)
        {
            return new(
                ErrorType.ExpectedSimpleExpressionAsSelector,
                "Expected a simple expression as selector",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError UnterminatedStringLiteral(int pos)
        {
            return new(
                ErrorType.UnterminatedStringLiteral,
                "Unterminated string literal",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError UnknownEscapeSequence(char seq, int pos)
        {
            var sb = new StringBuilder()
                .AppendFormat("Unknown escape sequence \"{0}\"", seq);

            return new(
                ErrorType.UnknownEscapeSequence,
                sb.ToString(),
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ForbiddenCallee(int pos)
        {
            return new(
                ErrorType.ForbiddenCallee,
                "Callee is not allowed here",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ExpectedLiteral(int pos)
        {
            return new(
                ErrorType.ExpectedLiteral,
                "Expected a string or number literal",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError ExpectedInlineExpression(int pos)
        {
            return new(
                ErrorType.ExpectedInlineExpression,
                "Expected an inline expression",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError DuplicatedNamedArgument(Identifier id, int pos)
        {
            var sb = new StringBuilder()
                .Append("The \"").Append(id.Name).Append("\" argument appears twice");

            return new(
                ErrorType.DuplicatedNamedArgument,
                sb.ToString(),
                new Range(pos, pos + 1)
            );
        }

        public static ParseError PositionalArgumentFollowsNamed(int pos)
        {
            return new(
                ErrorType.PositionalArgumentFollowsNamed,
                "Positional arguments must come before named arguments",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError MultipleDefaultVariants(int pos)
        {
            return new(
                ErrorType.MultipleDefaultVariants,
                "A select expression can only have one default variant",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError MissingDefaultVariant(int pos)
        {
            return new(
                ErrorType.MultipleDefaultVariants,
                "The select expression must have a default variant",
                new Range(pos, pos + 1)
            );
        }

        public static ParseError InvalidUnicodeEscapeSequence(string seq, int readerPosition)
        {
            return new(
                ErrorType.InvalidUnicodeEscapeSequence,
                $"Invalid unicode escape sequence, \"{seq}\"",
                new Range(readerPosition, readerPosition + 1)
            );
        }
    }
}
