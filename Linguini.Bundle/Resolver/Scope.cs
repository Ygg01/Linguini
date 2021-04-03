#nullable enable
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public class Scope
    {
        public readonly FluentBundle Bundle;
        private Dictionary<string, IFluentType>? _args;
        private Dictionary<string, IFluentType>? _localArgs;
        private List<Pattern> _travelled;
        private List<FluentError> _errors;

        public Scope(FluentBundle fluentBundle, IDictionary<string, IFluentType>? args)
        {
            Placeable = 0;
            Bundle = fluentBundle;
            Dirty = false;

            _errors = new List<FluentError>();
            _travelled = new List<Pattern>();
            if (args != null)
            {
                _args = new Dictionary<string, IFluentType>(args);
            }
            else
            {
                _args = null;
            }

            _localArgs = null;
            _errors = new List<FluentError>();
        }

        public bool Dirty { get; set; }
        public short Placeable { get; private set; }

        public IList<FluentError> Errors => _errors;

        public IReadOnlyDictionary<string, IFluentType>? LocalArgs => _localArgs;

        public IReadOnlyDictionary<string, IFluentType>? Args => _args;

        public short IncrPlaceable()
        {
            Placeable += 1;
            return Placeable;
        }

        public void AddError(ResolverFluentError resolverFluentError)
        {
            _errors.Add(resolverFluentError);
        }

        public bool MaybeTrack(TextWriter writer, Pattern pattern, IExpression expr, out IList<FluentError> errors)
        {
            if (_travelled.Count == 0)
            {
                _travelled.Add(pattern);
            }

            if (!expr.TryWrite(writer, this, out errors))
            {
                return false;
            }

            if (Dirty)
            {
                writer.Write('{');
                expr.WriteError(writer);
                writer.Write('}');
            }

            errors = null;
            return true;
        }

        public void Track(TextWriter writer, Pattern xValue, IInlineExpression self)
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public bool WriteRefError(TextWriter writer, IInlineExpression self)
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public ResolvedArgs GetArguments(CallArguments? termReferenceArguments)
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public void SetLocalArgs(IDictionary<string, IFluentType>? resNamed)
        {
            if (resNamed != null)
            {
                _localArgs = new Dictionary<string, IFluentType>(resNamed);
            }
            else
            {
                _localArgs = new Dictionary<string, IFluentType>();
            }
        }
    }

    public record ResolvedArgs(IList<IFluentType> positional, IDictionary<string, IFluentType> named);
}
