using System;
using System.Text;

using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Parser.Error
{
    /// <summary>
    /// Represents a parsing error encountered during the processing of syntax.
    /// </summary>
    public class ParseError
    {
        /// <summary>
        /// Read-only property showing which <see cref="ErrorType"/> occured.
        /// </summary>
        public ErrorType Kind { get; }
        
        /// <summary>
        /// Read-only message of the error.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// On which row the error occurred.
        /// </summary>
        public int Row { get; }
        
        /// <summary>
        /// On which position in stream did error occured.
        /// </summary>
        public Range Position { get; }
        
        /// <summary>
        /// Slice depicting the error message.
        /// </summary>
        public Range? Slice { get; set; }

        private ParseError(ErrorType kind, string message, Range position, int row)
        {
            Kind = kind;
            Message = message;
            Position = position;
            Slice = null;
            Row = row;
        }

        /// <summary>
        /// Creates a ParseError that indicates an expected token error.
        /// </summary>
        /// <param name="expected">The character that was expected.</param>
        /// <param name="actual">The actual character encountered, or null if none was found.</param>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedToken"/>.</returns>
        public static ParseError ExpectedToken(char expected, char? actual, int pos, int row)
        {
            var sb = new StringBuilder()
                .AppendFormat("Expected a token starting with  \"{0}\" found \"{1}\" instead",
                    expected,
                    actual == null
                        ? "EOF"
                        : actual.ToString());

            return new(
                ErrorType.ExpectedToken,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates an expected token error.
        /// </summary>
        /// <param name="exp1">The character that was expected.</param>
        /// <param name="exp2">The character that was expected.</param>
        /// <param name="actual">The actual character encountered, or null if none was found.</param>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedToken"/>.</returns>
        public static ParseError ExpectedToken(char exp1, char exp2, char? actual, int pos, int row)
        {
            var sb = new StringBuilder()
                .AppendFormat("Expected  \"{0}\" or \"{1}\" found \"{2}\" instead",
                    exp1, exp2, actual == null
                        ? "EOF"
                        : actual.ToString());

            return new(
                ErrorType.ExpectedToken,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }


        /// <summary>
        /// Creates a ParseError that indicates an expected character range error.
        /// </summary>
        /// <param name="range">The string representation of the expected character range.</param>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedCharRange"/>.</returns>
        public static ParseError ExpectedCharRange(string range, int pos, int row)
        {
            var sb = new StringBuilder().AppendFormat("Expected one of \"{0}\"", range);

            return new(
                ErrorType.ExpectedCharRange,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates an expected message field error.
        /// </summary>
        /// <param name="entryId">The identifier of the entry where the error occurred.</param>
        /// <param name="start">The starting position of the error in the source.</param>
        /// <param name="end">The ending position of the error in the source.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedMessageField"/>.</returns>
        public static ParseError ExpectedMessageField(ReadOnlyMemory<char> entryId, int start, int end, int row)
        {
            var sb = new StringBuilder()
                .Append(@"Expected a message field for """).Append(entryId).Append('"');

            return new(
                ErrorType.ExpectedMessageField,
                sb.ToString(),
                new Range(start, end),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates that a <see cref="Pattern"/> was expected after an identifier 
        /// and an equals <c>'='</c> sign.
        /// </summary>
        /// <param name="pos">The position of error in resources.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.MissingValue"/>.</returns>
        public static ParseError MissingValue(int pos, int row)
        {
            return new(
                ErrorType.MissingValue,
                "Expected a value",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates that braces aren't balanced properly
        /// </summary>
        /// <param name="pos">The position of error in resources.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.UnbalancedClosingBrace"/>.</returns>
        public static ParseError UnbalancedClosingBrace(int pos, int row)
        {
            return new(
                ErrorType.UnbalancedClosingBrace,
                "Unbalanced closing brace",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates that a term is used as a placeable.
        /// </summary>
        /// <param name="pos">The position of error in resources.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.TermAttributeAsPlaceable"/>.</returns>
        public static ParseError TermAttributeAsPlaceable(int pos, int row)
        {
            return new(
                ErrorType.TermAttributeAsPlaceable,
                "Term attributes can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a missing term field.
        /// </summary>
        /// <param name="id">The identifier of the term that is missing the field.</param>
        /// <param name="start">The starting position in the source where the error occurred.</param>
        /// <param name="end">The ending position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedTermField"/>.</returns>
        public static ParseError ExpectedTermField(Identifier id, int start, int end, int row)
        {
            var sb = new StringBuilder()
                .Append("Expected a term field for \"").Append(id.Name).Append("\"");

            return new(
                kind: ErrorType.ExpectedTermField,
                sb.ToString(),
                new Range(start, end),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a message reference is being used as a selector, which is not allowed.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.MessageReferenceAsSelector"/>.</returns>
        public static ParseError MessageReferenceAsSelector(int pos, int row)
        {
            return new(
                ErrorType.MessageReferenceAsSelector,
                "Message references can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates that message attributes cannot be used as selectors.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.MessageAttributeAsSelector"/>.</returns>
        public static ParseError MessageAttributeAsSelector(int pos, int row)
        {
            return new(
                ErrorType.MessageAttributeAsSelector,
                "Message attributes can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError for an invalid use of a term reference as a selector.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.TermReferenceAsSelector"/>.</returns>
        public static ParseError TermReferenceAsSelector(int pos, int row)
        {
            return new(
                ErrorType.TermReferenceAsSelector,
                "Term references can't be used as a selector",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates an expected simple expression as a selector error.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedSimpleExpressionAsSelector"/>.</returns>
        public static ParseError ExpectedSimpleExpressionAsSelector(int pos, int row)
        {
            return new(
                ErrorType.ExpectedSimpleExpressionAsSelector,
                "Expected a simple expression as selector",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a string literal wasn't closed.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.UnterminatedStringLiteral"/>.</returns>
        public static ParseError UnterminatedStringLiteral(int pos, int row)
        {
            return new(
                ErrorType.UnterminatedStringLiteral,
                "Unterminated string literal",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates an unknown escape sequence error.
        /// </summary>
        /// <param name="seq">The unknown escape sequence character that caused the error.</param>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.UnknownEscapeSequence"/>.</returns>
        public static ParseError UnknownEscapeSequence(char seq, int pos, int row)
        {
            var sb = new StringBuilder()
                .AppendFormat("Unknown escape sequence \"{0}\"", seq);

            return new(
                ErrorType.UnknownEscapeSequence,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a callee was used within a function reference.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ForbiddenCallee"/>.</returns>
        public static ParseError ForbiddenCallee(int pos, int row)
        {
            return new(
                ErrorType.ForbiddenCallee,
                "Callee is not allowed here",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a string or number literal was expected, but something else was found.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedLiteral"/>.</returns>
        public static ParseError ExpectedLiteral(int pos, int row)
        {
            return new(
                ErrorType.ExpectedLiteral,
                "Expected a string or number literal",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates an expected inline expression error.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.ExpectedInlineExpression"/>.</returns>
        public static ParseError ExpectedInlineExpression(int pos, int row)
        {
            return new(
                ErrorType.ExpectedInlineExpression,
                "Expected an inline expression",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError indicating a duplicated named argument error.
        /// </summary>
        /// <param name="id">The identifier of the argument that appears more than once.</param>
        /// <param name="pos">The position in the source where the duplicated argument was found.</param>
        /// <param name="row">The row number where the duplicated argument occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.DuplicatedNamedArgument"/>.</returns>
        public static ParseError DuplicatedNamedArgument(Identifier id, int pos, int row)
        {
            var sb = new StringBuilder()
                .Append("The \"").Append(id.Name).Append("\" argument appears twice");

            return new(
                ErrorType.DuplicatedNamedArgument,
                sb.ToString(),
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a positional argument follows a named argument error.
        /// </summary>
        /// <remarks>
        /// Positional arguments in functions have to appear first to prevent ambiguity.
        /// </remarks>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with the <see cref="ErrorType.PositionalArgumentFollowsNamed"/> type.</returns>
        public static ParseError PositionalArgumentFollowsNamed(int pos, int row)
        {
            return new(
                ErrorType.PositionalArgumentFollowsNamed,
                "Positional arguments must come before named arguments",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates the presence of multiple default variants in a select expression.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.MultipleDefaultVariants"/>.</returns>
        public static ParseError MultipleDefaultVariants(int pos, int row)
        {
            return new(
                ErrorType.MultipleDefaultVariants,
                "A select expression can only have one default variant",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates a missing default variant error in a select expression.
        /// </summary>
        /// <param name="pos">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.MissingDefaultVariant"/>.</returns>
        public static ParseError MissingDefaultVariant(int pos, int row)
        {
            return new(
                ErrorType.MissingDefaultVariant,
                "The select expression must have a default variant",
                new Range(pos, pos + 1),
                row
            );
        }

        /// <summary>
        /// Creates a ParseError that indicates an invalid Unicode escape sequence error.
        /// </summary>
        /// <param name="seq">The invalid Unicode escape sequence string encountered.</param>
        /// <param name="readerPosition">The position in the source where the error occurred.</param>
        /// <param name="row">The row number where the error occurred.</param>
        /// <returns>A new instance of ParseError with <see cref="ErrorType.InvalidUnicodeEscapeSequence"/>.</returns>
        public static ParseError InvalidUnicodeEscapeSequence(string seq, int readerPosition, int row)
        {
            return new(
                ErrorType.InvalidUnicodeEscapeSequence,
                $"Invalid unicode escape sequence, \"{seq}\"",
                new Range(readerPosition, readerPosition + 1),
                row
            );
        }
    }
}