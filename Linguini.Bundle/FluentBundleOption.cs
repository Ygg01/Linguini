using System;
using System.Collections.Generic;
using Linguini.Bundle.Types;

namespace Linguini.Bundle
{
    public class FluentBundleOption
    {
        public bool UseIsolating = true;
        public IDictionary<string, ExternalFunction> Functions { get; set; }
        public Func<IFluentType, string>? FormatterFunc;
        public Func<string, string>? TransformFunc;
        public byte MaxPlaceable = 100;
    }
}