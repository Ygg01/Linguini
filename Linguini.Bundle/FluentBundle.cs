#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Linguini.Bundle.Entry;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Resolver;
using Linguini.Bundle.Types;
using Linguini.Shared;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using PluralRules;

namespace Linguini.Bundle
{
    using FluentArgs = IDictionary<string, IFluentType>;

    public class FluentBundle 
    {
        private HashSet<string> _funcList;
        private Dictionary<string, IBundleEntry> _entries;

        public CultureInfo Culture { get; internal set; }
        public List<string> Locales { get; internal set; }
        private List<Resource> Resources { get; init; }


        public bool UseIsolating { get; set; }
        public Func<string, string>? TransformFunc { get; set; }
        public Func<IFluentType, string>? FormatterFunc { get; set; }
        public byte MaxPlaceable { get; }

        internal FluentBundle()
        {
            Culture = CultureInfo.CurrentCulture;
            Locales = new List<string>();
            Resources = new List<Resource>();
            _entries = new Dictionary<string, IBundleEntry>();
            _funcList = new HashSet<string>();
            UseIsolating = true;
            MaxPlaceable = 100;
        }

        public FluentBundle(string locale, FluentBundleOption option, out List<FluentError> errors) : this()
        {
            Locales = new List<string>(1);
            Locales.Add(locale);
            Culture = new CultureInfo(locale, false);
            UseIsolating = option.UseIsolating;
            FormatterFunc = option.FormatterFunc;
            TransformFunc = option.TransformFunc;
            MaxPlaceable = option.MaxPlaceable;
            AddFunctions(option.Functions, out errors, InsertBehavior.None);
        }

        public void AddFunctions(IDictionary<string, ExternalFunction> functions, out List<FluentError> errors,
            InsertBehavior behavior = InsertBehavior.Throw)
        {
            errors = new List<FluentError>();
            foreach (var keyValue in functions)
            {
                if (!AddFunction(keyValue.Key, keyValue.Value, out var errs, behavior))
                {
                    errors.AddRange(errs);
                }
            }
        }

        public bool AddFunction(string funcName, ExternalFunction fluentFunction,
            [NotNullWhen(false)] out IList<FluentError>? errors,
            InsertBehavior behavior = InsertBehavior.Throw)
        {
            errors = new List<FluentError>();
            switch (behavior)
            {
                case InsertBehavior.None:
                    if (!_entries.TryAdd(funcName, (FluentFunction) fluentFunction))
                    {
                        errors = new List<FluentError>
                        {
                            new OverrideFluentError(funcName, EntryKind.Function)
                        };
                    }

                    break;
                case InsertBehavior.Overriding:
                    _entries[funcName] = (FluentFunction) fluentFunction;
                    break;
                default:
                    if (_entries.ContainsKey(funcName))
                    {
                        errors.Add(new OverrideFluentError(funcName, EntryKind.Function));
                        return false;
                    }

                    _entries.Add(funcName, (FluentFunction) fluentFunction);
                    break;
            }

            _funcList.Add(funcName);
            return true;
        }

        public bool AddResource(Resource res, [NotNullWhen(false)] out List<FluentError> errors)
        {
            var resPos = Resources.Count;
            errors = new List<FluentError>();
            foreach (var parseError in res.Errors)
            {
                errors.Add(ParserFluentError.ParseError(parseError));
            }

            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];
                var id = "";
                IBundleEntry bundleEntry;
                if (entry.TryConvert(out AstMessage? message))
                {
                    id = message.GetId();
                    bundleEntry = new Message(resPos, entryPos);
                }
                else if (entry.TryConvert(out AstTerm? term))
                {
                    id = $"-{term.GetId()}";
                    bundleEntry = new Term(resPos, entryPos);
                }
                else
                {
                    continue;
                }

                if (_entries.ContainsKey(id))
                {
                    errors.Add(new OverrideFluentError(id, _entries[id].ToKind()));
                }
                else
                {
                    _entries.Add(id, bundleEntry);
                }
            }

            Resources.Add(res);
            if (errors.Count == 0)
            {
                return true;
            }

            return false;
        }

        public void AddResourceOverriding(Resource res)
        {
            var resPos = Resources.Count;
            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];
                var id = "";
                IBundleEntry bundleEntry;
                if (entry.TryConvert(out AstMessage? message))
                {
                    id = message.GetId();
                    bundleEntry = new Message(resPos, entryPos);
                }
                else if (entry.TryConvert(out AstTerm? term))
                {
                    id = term.GetId();
                    bundleEntry = new Term(resPos, entryPos);
                }
                else
                {
                    continue;
                }

                _entries[id] = bundleEntry;
            }

            Resources.Add(res);
        }

        public bool HasMessage(string id)
        {
            return _entries.ContainsKey(id)
                   && _entries[id].TryConvert<IBundleEntry, Message>(out _);
        }

        public string? GetMsg(string id, FluentArgs args, out IList<FluentError> errors)
        {
            return GetMsg(id, null, args, out errors);
        }


        public string? GetMsg(string id, string? attribute, FluentArgs args, out IList<FluentError> errors)
        {
            string? value = null;
            errors = new List<FluentError>();

            if (TryGetMessage(id, out var astMessage))
            {
                Pattern? pattern;
                pattern = attribute != null
                    ? astMessage.GetAttribute(attribute)?.Value
                    : astMessage.Value;

                if (pattern == null)
                {
                    var message = (attribute == null)
                        ? id
                        : $"{id}.{attribute}";
                    errors.Add(ResolverFluentError.NoValue($"{message}"));
                    return FluentNone.None.ToString();
                }

                value = FormatPattern(pattern, args, out errors);
            }

            return value;
        }

        public bool TryGetMessage(string id, [NotNullWhen(true)] out AstMessage? message)
        {
            if (_entries.ContainsKey(id)
                && _entries.TryGetValue(id, out var value)
                && value.ToKind() == EntryKind.Message
                && value.TryConvert(out Message? msg))
            {
                var res = Resources[msg.ResPos];
                var entry = res.Entries[msg.EntryPos];

                return entry.TryConvert(out message);
            }

            message = null;
            return false;
        }

        public bool TryGetTerm(string id, [NotNullWhen(true)] out AstTerm? astTerm)
        {
            var termId = $"-{id}";
            if (_entries.ContainsKey(termId)
                && _entries.TryGetValue(termId, out var value)
                && value.ToKind() == EntryKind.Term
                && value.TryConvert(out Term? term))
            {
                var res = Resources[term.ResPos];
                var entry = res.Entries[term.EntryPos];

                return entry.TryConvert(out astTerm);
            }

            astTerm = null;
            return false;
        }

        public bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function)
        {
            return TryGetFunction(id.ToString(), out function);
        }

        public bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function)
        {
            if (_entries.ContainsKey(funcName)
                && _entries.TryGetValue(funcName, out var value)
                && value.ToKind() == EntryKind.Function)
            {
                return value.TryConvert(out function);
            }

            function = null;
            return false;
        }

        public string FormatPattern(Pattern pattern, FluentArgs? args,
            out IList<FluentError> errors)
        {
            var scope = new Scope(this, args);
            var value = pattern.Resolve(scope);
            errors = scope.Errors;
            return value.AsString();
        }

        public FluentBundle DeepClone()
        {
            return new()
            {
                Culture = (CultureInfo) Culture.Clone(),
                Resources = new List<Resource>(Resources),
                FormatterFunc = FormatterFunc,
                Locales = new List<string>(Locales),
                _entries = new(_entries),
                _funcList = new HashSet<string>(_funcList),
                TransformFunc = TransformFunc,
                UseIsolating = UseIsolating,
            };
        }
    }
}
