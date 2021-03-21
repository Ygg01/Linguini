using System.Collections.Generic;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;

namespace Linguini.Bundle.Entry
{
    public interface IBundleEntry
    {
        EntryKind ToKind();
    }

    public delegate IFluentType ExternalFunction(
        IList<IFluentType> positionalArgs,
        IDictionary<string, IFluentType> namedArgs);

    public record Message(int ResPos, int EntryPos) : IBundleEntry
    {
        public EntryKind ToKind()
        {
            return EntryKind.Message;
        }
    }

    public record Term(int ResPos, int EntryPos) : IBundleEntry
    {
        public EntryKind ToKind()
        {
            return EntryKind.Term;
        }
    }

    public record FluentFunction(ExternalFunction Function) : IBundleEntry
    {
        public EntryKind ToKind()
        {
            return EntryKind.Function;
        }
    }
}
