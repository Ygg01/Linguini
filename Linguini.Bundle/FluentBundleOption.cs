using System;
using System.Collections.Generic;
using Linguini.Bundle.Types;

namespace Linguini.Bundle
{
    public class FluentBundleOption
    {
        public bool UseIsolating = true;

        public FluentBundleOption(bool useIsolating, Func<IFluentType, string>? formatterFunc, 
            Func<string, string>? transformFunc, byte maxPlaceable, IDictionary<string, ExternalFunction> functions)
        {
            UseIsolating = useIsolating;
            FormatterFunc = formatterFunc;
            TransformFunc = transformFunc;
            MaxPlaceable = maxPlaceable;
            Functions = functions;
        }

        public IDictionary<string, ExternalFunction> Functions { get; set; }
        public Func<IFluentType, string>? FormatterFunc;
        public Func<string, string>? TransformFunc;
        public byte MaxPlaceable = 100;
    }
}