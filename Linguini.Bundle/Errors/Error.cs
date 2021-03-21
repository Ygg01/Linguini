using System;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Bundle.Errors
{
    public abstract record Error
    {
    }

    public record OverrideError : Error
    {
        public readonly string Id;
        public EntryKind Kind;

        public OverrideError(string id, EntryKind kind)
        {
            Id = id;
            Kind = kind;
        }

        public override string ToString()
        {
            return $"For id:{Id} already exist entry of type: {Kind.ToString()}";
        }
    }

    public record ResolverError : Error
    {
        public string Description;

        public override string ToString()
        {
            return Description;
        }
    }

    public record ParserError : Error
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
