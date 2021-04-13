using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Bundle.Errors
{
    public abstract record FluentError
    {
        public abstract ErrorType ErrorKind();
    }

    public record OverrideFluentError : FluentError
    {
        public readonly string Id;
        public EntryKind Kind;
    
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
        public string Description;
        private ErrorType Kind;

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
            if (self.TryConvert(out FunctionReference funcRef))
            {
                return new($"Unknown function: {funcRef.Id}()", ErrorType.Reference);
            }

            if (self.TryConvert(out MessageReference msgRef))
            {
                if (msgRef.Attribute == null)
                {
                    return new($"Unknown message: {msgRef.Id}", ErrorType.Reference);
                }
                else
                {
                    return new($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}", ErrorType.Reference);
                }
            }

            if (self.TryConvert(out TermReference termReference))
            {
                if (termReference.Attribute == null)
                {
                    return new($"Unknown term: -{termReference.Id}", ErrorType.Reference);
                }
                else
                {
                    return new($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}", ErrorType.Reference);
                }
            }

            if (self.TryConvert(out VariableReference varRef))
            {
                return new($"Unknown variable: ${varRef.Id}", ErrorType.Reference);
            }

            throw new ArgumentException($"Expected reference got ${self.GetType()}");
        }

        public static ResolverFluentError Cyclic(Pattern pattern)
        {
            return new($"Cyclic pattern {pattern.Stringify()} detected!", ErrorType.Cyclic);
        }

        public static ResolverFluentError MissingDefault()
        {
            return new("No default", ErrorType.MissingDefault);
        }
    }

    public record ParserFluentError : FluentError
    {
        private ParseError Error;

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
            return ErrorType.Syntax;
        }

        public override string ToString()
        {
            return Error.Message;
        }
    }

    public enum EntryKind : byte
    {
        Message,
        Term,
        Function,
    }

    public enum ErrorType : byte
    {
        Reference,
        Cyclic,
        NoValue,
        TooManyPlaceables,
        Overriding,
        Syntax,
        MissingDefault
    }
}