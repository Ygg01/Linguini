#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public class Scope : IScope
    {
        public readonly FluentBundle Bundle;
        private readonly Dictionary<string, IFluentType>? _args;
        private Dictionary<string, IFluentType>? _localArgs;
        private readonly List<Pattern> _travelled;
        private readonly List<FluentError> _errors;


        public Scope(FluentBundle fluentBundle, IDictionary<string, IFluentType>? args)
        {
            Placeable = 0;
            Bundle = fluentBundle;
            Dirty = false;

            _errors = new List<FluentError>();
            _travelled = new List<Pattern>();
            _args = args != null ? new Dictionary<string, IFluentType>(args) : null;

            _localArgs = null;
            _errors = new List<FluentError>();
        }

        public bool Dirty { get; set; }
        private short Placeable { get; set; }

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

        public void MaybeTrack(TextWriter writer, Pattern pattern, IExpression expr)
        {
            if (_travelled.Count == 0)
            {
                _travelled.Add(pattern);
            }

            expr.TryWrite(writer, this);

            if (Dirty)
            {
                writer.Write('{');
                expr.WriteError(writer);
                writer.Write('}');
            }
        }

        public void Track(TextWriter writer, Pattern pattern, IInlineExpression exp)
        {
            if (_travelled.Contains(pattern))
            {
                AddError(ResolverFluentError.Cyclic(pattern));
                writer.Write('{');
                exp.WriteError(writer);
                writer.Write('}');
            }
            else
            {
                _travelled.Add(pattern);
                pattern.Write(writer, this);
                PopTraveled();
            }
        }

        private void PopTraveled()
        {
            if (_travelled.Count > 0)
            {
                _travelled.RemoveAt(_travelled.Count - 1);
            }
        }

        public bool WriteRefError(TextWriter writer, IInlineExpression exp)
        {
            AddError(ResolverFluentError.Reference(exp));
            try
            {
                writer.Write('{');
                exp.WriteError(writer);
                writer.Write('}');
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ResolvedArgs GetArguments(CallArguments? callArguments)
        {
            var positionalArgs = new List<IFluentType>();
            var namedArgs = new Dictionary<string, IFluentType>();
            if (callArguments != null)
            {
                var listPositional = callArguments.Value.PositionalArgs;
                for (var i = 0; i < listPositional.Count; i++)
                {
                    var expr = listPositional[i].Resolve(this);
                    positionalArgs.Add(expr);
                }

                var listNamed = callArguments.Value.NamedArgs;
                for (var i = 0; i < listNamed.Count; i++)
                {
                    var arg = listNamed[i];
                    namedArgs.Add(arg.Name.ToString(), arg.Value.Resolve(this));
                }
            }

            return new ResolvedArgs(positionalArgs, namedArgs);
        }

        public void SetLocalArgs(IDictionary<string, IFluentType>? resNamed)
        {
            _localArgs = resNamed != null
                ? new Dictionary<string, IFluentType>(resNamed)
                : new Dictionary<string, IFluentType>();
        }

        public PluralCategory GetPluralRules(RuleType type, FluentNumber number)
        {
            return ResolverHelpers.PluralRules.GetPluralCategory(Bundle.Culture, type, number);
        }
    }

    public class ResolvedArgs
    {
        public IList<IFluentType> Positional;
        public IDictionary<string, IFluentType> Named;

        public ResolvedArgs(IList<IFluentType> positional, IDictionary<string, IFluentType> named)
        {
            Positional = positional;
            Named = named;
        }

        public void Deconstruct(out IList<IFluentType> posArg, out IDictionary<string, IFluentType> namedArg)
        {
            posArg = Positional;
            namedArg = Named;
        }
    }
}
