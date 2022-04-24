using System;
using System.Collections.Generic;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Builder
{
    public class FluentBundleOption
    {
        public bool UseConcurrent { get; set; }
        public bool UseIsolating { get; set; } = true;
        public byte MaxPlaceable { get; set; } = 100;

        public IList<string> Locales { get; set; } = new List<string>();

        public IDictionary<string, ExternalFunction> Functions { get; set; } =
            new Dictionary<string, ExternalFunction>();

        public Func<IFluentType, string>? FormatterFunc { get; set; }
        public Func<string, string>? TransformFunc { get; set; }
    }
}
