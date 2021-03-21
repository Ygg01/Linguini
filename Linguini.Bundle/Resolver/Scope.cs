using System.Collections.Generic;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public class Scope
    {
        public FluentBundle Bundle;
        private IDictionary<string, IFluentType>? _args;
        private IDictionary<string, IFluentType>? _localArgs;
        private short placeable;
        private List<Pattern> visitedPatterns;
        private List<FluentError> errors;
        private bool dirty;
        public Scope(FluentBundle fluentBundle, IDictionary<string, IFluentType>? args, out IList<FluentError> errors)
        {
            throw new System.NotImplementedException();
        }
    }
}
