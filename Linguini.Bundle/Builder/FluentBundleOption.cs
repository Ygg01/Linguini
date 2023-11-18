using System;
using System.Collections.Generic;
using System.Globalization;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Builder
{
    public class FluentBundleOption
    {
        public bool UseConcurrent { get; init; }
        public bool EnableExtensions { get; init; }
        public bool UseIsolating { get; init; } = true;
        public byte MaxPlaceable { get; init; } = 100;

        public CultureInfo Culture{ get; init; } = CultureInfo.CurrentCulture;
        
        public List<CultureInfo> Cultures{ get; init; } = new();

        public List<string> Locales { get; } = new();

        public IDictionary<string, ExternalFunction> Functions { get; init; } =
            new Dictionary<string, ExternalFunction>();

        public Func<IFluentType, string>? FormatterFunc { get; init; }
        public Func<string, string>? TransformFunc { get; init; }
    }
}
