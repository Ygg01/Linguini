using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;
// ReSharper disable All

namespace Linguini.Bundle.Errors
{
    public abstract record FluentError
    {
        public abstract ErrorType ErrorKind();

        public virtual ErrorSpan? GetSpan()
        {
            return null;
        }
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

    public record OverrideFluentError : FluentError
    {
        public string Id { get; }
        public EntryKind Kind { get; }

        public OverrideFluentError(string id, EntryKind kind)
        {
            Id = id;
            Kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return ErrorType.Overriding;
        }

        public override string ToString()
        {
            return $"For id:{Id} already exist entry of type: {Kind.ToString()}";
        }
    }

    public record ResolverFluentError : FluentError
    {
        public string Description { get; }
        public ErrorType Kind { get; }

        private ResolverFluentError(string desc, ErrorType kind)
        {
            Description = desc;
            Kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return Kind;
        }

        public override string ToString()
        {
            return Description;
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
            // TODO only allow references here
            if (self is FunctionReference funcRef)
            {
                return new($"Unknown function: {funcRef.Id}()", ErrorType.Reference);
            }

            if (self is MessageReference msgRef)
            {
                if (msgRef.Attribute == null)
                {
                    return new($"Unknown message: {msgRef.Id}", ErrorType.Reference);
                }

                return new($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}", ErrorType.Reference);
            }

            if (self is TermReference termReference)
            {
                if (termReference.Attribute == null)
                {
                    return new($"Unknown term: -{termReference.Id}", ErrorType.Reference);
                }

                return new($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}", ErrorType.Reference);
            }

            if (self is VariableReference varRef)
            {
                return new($"Unknown variable: ${varRef.Id}", ErrorType.Reference);
            }

            throw new ArgumentException($"Expected reference got ${self.GetType()}");
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
        public ParseError Error { get; }

        private ParserFluentError(ParseError error)
        {
            Error = error;
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
            return Error.Message;
        }

        public override ErrorSpan? GetSpan()
        {
            if (Error.Slice == null)
                return null;

            return new(Error.Row, Error.Slice.Value.Start.Value, Error.Slice.Value.End.Value,
                Error.Position.Start.Value, Error.Position.End.Value);
        }
    }

    public enum EntryKind : byte
    {
        Message,
        Term,
        Func,
    }

    [Obsolete]
    public static class EntryHelper
    {
        [Obsolete]
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
