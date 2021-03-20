using System.Collections.Generic;
using Linguini.Bundle.Types;

namespace Linguini.Bundle.Entry
{
    public interface IBundleEntry
    {
    }

    public delegate IFluentType ExternalFunction(
        IList<object> positionalArgs,
        IDictionary<string, object> namedArgs);

    public record Message(int ResPos, int EntryPos) : IBundleEntry;

    public record Term(int ResPos, int EntryPos) : IBundleEntry;

    public record FluentFunction(ExternalFunction Function) : IBundleEntry;
}
