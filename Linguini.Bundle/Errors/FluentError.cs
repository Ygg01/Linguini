using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Bundle.Errors
{
    /// <summary>
    /// Base record for Linguini Fluent errors.
    /// </summary>
    public abstract record FluentError
    {
        /// <summary>
        /// What type of error was encountered
        /// </summary>
        /// <returns>Enumeration describing error.</returns>
        public abstract ErrorType ErrorKind();

        /// <summary>
        /// Where in file was error encountered.
        /// </summary>
        /// <returns>Position of error in file</returns>
        public virtual ErrorSpan? GetSpan()
        {
            return null;
        }

        /// <summary>
        /// String represntation of the error.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Error (${ErrorKind()})";
    }

    /// <summary>
    /// Record used to pretty print the error if possible
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
    /// Represents an error that occurs when there is an attempt to override an existing entry in a FluentBundle.
    /// </summary>
    public record OverrideFluentError : FluentError
    {
        private readonly string _id;
        private readonly EntryKind _kind;

        /// <summary>
        /// Id of term or message that was overriden.
        /// </summary>
        public string Id => _id;

        public OverrideFluentError(string id, EntryKind kind)
        {
            _id = id;
            _kind = kind;
        }

        /// <inheritdoc/>
        public override ErrorType ErrorKind()
        {
            return ErrorType.Overriding;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"For id:{_id} already exist entry of type: {_kind.ToString()}";
        }
    }

    public record ResolverFluentError : FluentError
    {
        private readonly string _description;
        private readonly ErrorType _kind;

        private ResolverFluentError(string desc, ErrorType kind)
        {
            _description = desc;
            _kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return _kind;
        }

        public override string ToString()
        {
            return _description;
        }

        public static ResolverFluentError NoValue(ReadOnlyMemory<char> idName)
        {
            return new($"No value: {idName.Span.ToString()}", ErrorType.NoValue);
        }

        public static ResolverFluentError NoValue(string pattern)
        {
            return new($"No value: {pattern}", ErrorType.NoValue);
        }

        public static ResolverFluentError UnknownVariable(VariableReference outType)
        {
            return new($"Unknown variable: ${outType.Id}", ErrorType.Reference);
        }

        public static ResolverFluentError TooManyPlaceables()
        {
            return new("Too many placeables", ErrorType.TooManyPlaceables);
        }

        public static ResolverFluentError Reference(IInlineExpression self)
        {
            return self switch
            {
                FunctionReference funcRef => new($"Unknown function: {funcRef.Id}()", ErrorType.Reference),
                MessageReference msgRef when msgRef.Attribute == null => new($"Unknown message: {msgRef.Id}",
                    ErrorType.Reference),
                MessageReference msgRef => new($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}",
                    ErrorType.Reference),
                TermReference termReference when termReference.Attribute == null => new(
                    $"Unknown term: -{termReference.Id}", ErrorType.Reference),
                TermReference termReference => new($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}",
                    ErrorType.Reference),
                VariableReference varRef => new($"Unknown variable: ${varRef.Id}", ErrorType.Reference),
                _ => throw new ArgumentException($"Expected reference got ${self.GetType()}")
            };
        }

        public static ResolverFluentError Cyclic(Pattern pattern)
        {
            return new($"Cyclic reference in {pattern.Stringify()} detected!", ErrorType.Cyclic);
        }

        public static ResolverFluentError MissingDefault()
        {
            return new("No default", ErrorType.MissingDefault);
        }
    }

    public record ParserFluentError : FluentError
    {
        private readonly ParseError _error;

        private ParserFluentError(ParseError error)
        {
            _error = error;
        }

        public static ParserFluentError ParseError(ParseError parseError)
        {
            return new(parseError);
        }

        public override ErrorType ErrorKind()
        {
            return ErrorType.Parser;
        }

        public override string ToString()
        {
            return _error.Message;
        }

        public override ErrorSpan? GetSpan()
        {
            if (_error.Slice == null)
                return null;

            return new(_error.Row, _error.Slice.Value.Start.Value, _error.Slice.Value.End.Value,
                _error.Position.Start.Value, _error.Position.End.Value);
        }
    }

    public enum EntryKind : byte
    {
        Message,
        Term,
        Func,
    }

    public static class EntryHelper
    {
        public static EntryKind ToKind(this IEntry self)
        {
            if (self is AstTerm)
            {
                return EntryKind.Term;
            }

            return self is AstMessage ? EntryKind.Message : EntryKind.Func;
        }
    }

    public enum ErrorType : byte
    {
        Parser,
        Reference,
        Cyclic,
        NoValue,
        TooManyPlaceables,
        Overriding,
        MissingDefault
    }
}
