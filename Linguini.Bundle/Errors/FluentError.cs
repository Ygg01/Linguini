using System;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Bundle.Errors
{
    public abstract record FluentError
    {
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

        public override string ToString()
        {
            return $"For id:{Id} already exist entry of type: {Kind.ToString()}";
        }
    }

    public record ResolverFluentError : FluentError
    {
        public string Description;

        public override string ToString()
        {
            return Description;
        }
    }

    public record ParserFluentError : FluentError
    {
        public ParseError Error;

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
}
