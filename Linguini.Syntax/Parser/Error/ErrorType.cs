using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Parser.Error
{
    /// <summary>
    ///     Enum containing different error types.
    /// </summary>
    public enum ErrorType : byte
    {
        /// <summary>
        ///     Expected a token, but found another instead.
        /// </summary>
        ExpectedToken,

        /// <summary>
        ///     Expected next character to be in range, but found another character instead.
        /// </summary>
        ExpectedCharRange,

        /// <summary>
        ///     Expected a message field.
        /// </summary>
        ExpectedMessageField,

        /// <summary>
        ///     Expected a pattern after an identifier and equals sign
        /// </summary>
        MissingValue,

        /// <summary>
        ///     Braces aren't balanced properly.
        /// </summary>
        UnbalancedClosingBrace,

        /// <summary>
        ///     <see cref="AstTerm" /> is used as a <see cref="Placeable" />.
        /// </summary>
        TermAttributeAsPlaceable,

        /// <summary>
        ///     Expected an <see cref="AstTerm" /> but found something else.
        /// </summary>
        ExpectedTermField,

        /// <summary>
        ///     Message reference was incorrectly used as a selector.
        /// </summary>
        MessageReferenceAsSelector,

        /// <summary>
        ///     Message attribute was incorrectly used as a selector.
        /// </summary>
        MessageAttributeAsSelector,

        /// <summary>
        ///     Term reference was incorrectly used as a selector.
        /// </summary>
        TermReferenceAsSelector,

        /// <summary>
        ///     Expected a simple expression in selector.
        /// </summary>
        /// <seealso cref="TextLiteral"/>
        /// <seealso cref="NumberLiteral"/>
        /// <seealso cref="VariableReference"/>
        /// <seealso cref="FunctionReference"/>
        /// <seealso cref="DynamicReference"/>
        ExpectedSimpleExpressionAsSelector,

        /// <summary>
        ///     Opened a string literal without closing.
        /// </summary>
        UnterminatedStringLiteral,

        /// <summary>
        ///     Unicode sequences are expected to start with <c>\U</c> or <c>\u</c>
        /// </summary>
        UnknownEscapeSequence,
        
        /// <summary>
        ///     Found Callee inside functional call.
        /// </summary>
        ForbiddenCallee,
        
        /// <summary>
        ///     Expected a string or number literal, found other values instead.
        /// </summary>
        ExpectedLiteral,
        
        /// <summary>
        ///     Expected an inline expression but found other values instead.
        /// </summary>
        ExpectedInlineExpression,
        
        /// <summary>
        ///     Arguments in functions appear twice.
        /// </summary>
        DuplicatedNamedArgument,
        
        /// <summary>
        ///     Positional arguments must appear first in function calls.
        /// </summary>
        PositionalArgumentFollowsNamed,
        
        /// <summary>
        ///     Selectors had more than ONE default variant.
        /// </summary>
        MultipleDefaultVariants,
        
        /// <summary>
        ///     Selectors had no default variant.
        /// </summary>
        MissingDefaultVariant,
        
        /// <summary>
        ///     Only <c>\u</c> and <c>\U</c> are allowed as escape sequences.
        /// </summary>
        InvalidUnicodeEscapeSequence
    }
}