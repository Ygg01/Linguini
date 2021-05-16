using System;
using System.Collections.Generic;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Builder
{
    public class FluentBundleOption
    {
        public bool UseIsolating { get; init; } = true;
        public byte MaxPlaceable { get; init; } = 100;

        public IDictionary<string, ExternalFunction> Functions { get; init; } =
            new Dictionary<string, ExternalFunction>();

        public Func<IFluentType, string>? FormatterFunc { get; init; }
        public Func<string, string>? TransformFunc { get; init; }
    }
}
