using System;
using Linguini.Bundle.Builder;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Linguini.Bundle.Errors
{
    /// <summary>
    ///     Provides base record for Linguini's Fluent related errors.
    /// </summary>
    public abstract record FluentError
    {
        /// <summary>
        ///     What type of error was encountered
        /// </summary>
        /// <returns>Enumeration describing error.</returns>
        public abstract ErrorType ErrorKind();

        /// <summary>
        ///     Where in file was error encountered.
        /// </summary>
        /// <returns>Position of error in file</returns>
        public virtual ErrorSpan? GetSpan()
        {
            return null;
        }

        /// <summary>
        ///     String representation of the error.
        /// </summary>
        /// <returns>Error represented as string</returns>
        public override string ToString()
        {
            return $"Error (${ErrorKind()})";
        }
    }

    /// <summary>
    ///     Record used to pretty print the error if possible
    /// </summary>
    /// <param name="Row">Row in which error occured</param>
    /// <param name="StartSpan">Start of error context</param>
    /// <param name="EndSpan">End of error context</param>
    /// <param name="StartMark">Start of error mark</param>
    /// <param name="EndMark">End of error mark</param>
    public record ErrorSpan(int Row, int StartSpan, int EndSpan, int StartMark, int EndMark)
    {
    }

    /// <summary>
    ///     Represents an error that occurs when there is an attempt to override an existing entry in a FluentBundle.
    /// </summary>
    public record OverrideFluentError : FluentError
    {
        private readonly EntryKind _kind;
        private readonly string? _location;

        /// <summary>
        ///     Constructs an <see cref="OverrideFluentError" />
        /// </summary>
        /// <param name="id">Duplicated identifier</param>
        /// <param name="kind">Enumeration showing what kind of identifier was duplicate <c>MESSAGE | TERM | FUNCTION</c></param>
        /// <param name="location">Location where the error originated. Due to backwards compatibility it defaults to null</param>
        public OverrideFluentError(string id, EntryKind kind, string? location = null)
        {
            Id = id;
            _kind = kind;
            _location = location;
        }

        /// <summary>
        ///     Id of term or message that was overriden.
        /// </summary>
        public string Id { get; }

        /// <inheritdoc />
        public override ErrorType ErrorKind()
        {
            return ErrorType.Overriding;
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"For id:{Id} already exist entry of type: {_kind.ToString()}";
        }
    }

    /// <summary>
    /// </summary>
    public record ResolverFluentError : FluentError
    {
        private readonly string _description;
        private readonly ErrorType _kind;
        private readonly string? _location;

        private ResolverFluentError(string desc, ErrorType kind, string? location)
        {
            _description = desc;
            _kind = kind;
            _location = location;
        }

        /// <inheritdoc />
        public override ErrorType ErrorKind()
        {
            return _kind;
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return _description;
        }

        /// <summary>
        ///     Creates a new ResolverFluentError indicating that a required value was not found.
        /// </summary>
        /// <param name="idName">The identifier of the missing value.</param>
        /// <param name="location">Optional location where the error occurred.</param>
        /// <returns>A ResolverFluentError representing the "No Value" error.</returns>
        public static ResolverFluentError NoValue(ReadOnlyMemory<char> idName, string? location = null)
        {
            return new ResolverFluentError($"No value: {idName.Span.ToString()}", ErrorType.NoValue, location);
        }

        /// <summary>
        ///     Indicates that a required value is missing within the Fluent processing context.
        /// </summary>
        /// <returns>An enumeration indicating an error caused by the absence of a value.</returns>
        public static ResolverFluentError NoValue(string pattern, string? location = null)
        {
            return new ResolverFluentError($"No value: {pattern}", ErrorType.NoValue, location);
        }

        /// <summary>
        ///     Creates a ResolverFluentError instance for an unknown variable reference.
        /// </summary>
        /// <param name="outType">The variable reference that caused the error.</param>
        /// <param name="location">The optional location where the error occurred.</param>
        /// <returns>A ResolverFluentError describing the unknown variable error.</returns>
        public static ResolverFluentError UnknownVariable(VariableReference outType, string? location = null)
        {
            return new ResolverFluentError($"Unknown variable: ${outType.Id}", ErrorType.Reference, location);
        }

        /// <summary>
        ///     Creates a new ResolverFluentError indicating that there are too many placeable expressions
        ///     in the resolved pattern, which exceeds the allowed limit.
        /// </summary>
        /// <param name="location">Optional location where the error occurred.</param>
        /// <returns>A ResolverFluentError representing the "Too Many Placeables" error.</returns>
        public static ResolverFluentError TooManyPlaceables(string? location = null)
        {
            return new ResolverFluentError("Too many placeables", ErrorType.TooManyPlaceables, location);
        }

        /// <summary>
        ///     Creates a new ResolverFluentError indicating that there are too many placeable expressions
        ///     in the resolved pattern, which exceeds the allowed limit.
        /// </summary>
        /// <param name="location">Optional location where the error occurred.</param>
        /// <returns>A ResolverFluentError representing the "Too Many Placeables" error.</returns>
        public static ResolverFluentError TooManyPlaceables(int placeables, int maxPlaceable, string? location = null)
        {
            return new ResolverFluentError(
                $"Too many placeables expanded: {placeables}, \nmax allowed is {maxPlaceable}`",
                ErrorType.TooManyPlaceables, location);
        }

        /// <summary>
        ///     Generates a ResolverFluentError based on the provided reference type and its location.
        /// </summary>
        /// <param name="self">The reference object representing a specific inline expression.</param>
        /// <param name="location">The optional string indicating the location associated with the error.</param>
        /// <returns>A new instance of <see cref="ResolverFluentError" /> specifying the error details.</returns>
        /// <exception cref="ArgumentException">Thrown when the given reference object is of an unsupported type.</exception>
        public static ResolverFluentError Reference(IInlineExpression self, string? location = null)
        {
            return self switch
            {
                FunctionReference funcRef => new ResolverFluentError($"Unknown function: {funcRef.Id}()",
                                                                     ErrorType.Reference, location),
                MessageReference msgRef when msgRef.Attribute == null => new ResolverFluentError(
                    $"Unknown message: {msgRef.Id}",
                    ErrorType.Reference, location),
                MessageReference msgRef => new ResolverFluentError($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}",
                                                                   ErrorType.Reference, location),
                TermReference termReference when termReference.Attribute == null => new ResolverFluentError(
                    $"Unknown term: -{termReference.Id}", ErrorType.Reference, location),
                TermReference termReference => new ResolverFluentError(
                    $"Unknown attribute: -{termReference.Id}.{termReference.Attribute}",
                    ErrorType.Reference, location),
                VariableReference varRef => new ResolverFluentError($"Unknown variable: ${varRef.Id}",
                                                                    ErrorType.Reference, location),
                DynamicReference dynamicReference => new ResolverFluentError(
                    $"Unknown dynamic reference: $${dynamicReference.Id}", ErrorType.Reference, location),
                _ => throw new ArgumentException($"Expected reference got ${self.GetType()}")
            };
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ResolverFluentError" /> representing a cyclic reference error.
        /// </summary>
        /// <param name="pattern">The pattern where the cyclic reference was detected.</param>
        /// <param name="location">The optional location of the error.</param>
        /// <returns>A <see cref="ResolverFluentError" /> indicating a cyclic reference error.</returns>
        public static ResolverFluentError Cyclic(Pattern pattern, string? location = null)
        {
            return new ResolverFluentError($"Cyclic reference in {pattern.Stringify()} detected!", ErrorType.Cyclic,
                                           location);
        }

        /// <summary>
        ///     Creates a FluentError indicating that the default value is missing in a selection or resolution process.
        /// </summary>
        /// <param name="location">The location in the resource where the error was encountered. Can be null.</param>
        /// <returns>An instance of the <see cref="ResolverFluentError" /> representing a missing default error.</returns>
        public static ResolverFluentError MissingDefault(string? location = null)
        {
            return new ResolverFluentError("No default", ErrorType.MissingDefault, location);
        }
    }

    /// <summary>
    ///     Represents a Fluent error that occurs during the parsing process.
    /// </summary>
    public record ParserFluentError : FluentError
    {
        private readonly ParseError _error;
        private readonly string? _location;

        private ParserFluentError(ParseError error, string? location = null)
        {
            _error = error;
            _location = location;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ParserFluentError" /> from a given parse error.
        /// </summary>
        /// <param name="parseError">The parse error to be wrapped in a <see cref="ParserFluentError" />.</param>
        /// <returns>An instance of <see cref="ParserFluentError" /> encapsulating the specified parse error.</returns>
        public static ParserFluentError ParseError(ParseError parseError)
        {
            return new ParserFluentError(parseError);
        }

        /// <inheritdoc />
        public override ErrorType ErrorKind()
        {
            return ErrorType.Parser;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _error.Message;
        }

        /// <inheritdoc />
        public override ErrorSpan? GetSpan()
        {
            if (_error.Slice == null)
            {
                return null;
            }

            return new ErrorSpan(_error.Row, _error.Slice.Value.Start.Value, _error.Slice.Value.End.Value,
                                 _error.Position.Start.Value, _error.Position.End.Value);
        }
    }

    /// <summary>
    ///     Enumeration showing which kind of Entry was duplicated.
    /// </summary>
    public enum EntryKind : byte
    {
        /// <summary>
        ///     Simple message that starts with ASCII character e.g. <c>message-id</c>
        /// </summary>
        Message,

        /// <summary>
        ///     Referencable message that starts with <c>'-'</c> e.g. <c>-term-id</c>
        /// </summary>
        Term,

        /// <summary>
        ///     Callable functions
        /// </summary>
        Func
    }

    /// <summary>
    ///     Extensions method for <see cref="IEntry" />
    /// </summary>
    public static class EntryHelper
    {
        /// <summary>
        ///     Gets <see cref="EntryKind" /> for <paramref name="self" />.
        /// </summary>
        /// <param name="self"><see cref="IEntry" /> in <see cref="FluentBundle" /> being evaluated.</param>
        /// <returns></returns>
        public static EntryKind ToKind(this IEntry self)
        {
            return self switch
            {
                AstTerm    => EntryKind.Term,
                AstMessage => EntryKind.Message,
                _          => EntryKind.Func
            };
        }
    }

    /// <summary>
    ///     Source of Error for Fluent Bundle
    /// </summary>
    public enum ErrorType : byte
    {
        /// <summary>
        ///     Error during parsing
        /// </summary>
        Parser,

        /// <summary>
        ///     Call to unknown Reference.
        /// </summary>
        Reference,

        /// <summary>
        ///     Cyclic calls. Prohibited in Fluent.
        /// </summary>
        Cyclic,

        /// <summary>
        ///     No Value was found with that id.
        /// </summary>
        NoValue,

        /// <summary>
        ///     Too many <see cref="Placeable" />.
        /// </summary>
        /// <remarks>
        ///     Means that the entire placeable stack was consumed.
        ///     This indicates too much nesting. Which may be unintentional or malicious.
        ///     To change the maximum number of Placeables see <see cref="FluentBundleOption.MaxPlaceable" />
        /// </remarks>
        TooManyPlaceables,

        /// <summary>
        ///     A value added to <see cref="FluentBundle" /> is overriding an existing one.
        /// </summary>
        /// <remarks>
        ///     The Bundle was not set to replace on duplicate items found.
        ///     To change this, use <see cref="FluentBundle.AddResourceOverriding(string)" />
        /// </remarks>
        Overriding,

        /// <summary>
        ///     Selector without a default mark.
        /// </summary>
        MissingDefault
    }
}