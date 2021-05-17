#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Resolver;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Shared.Util;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle
{
    using FluentArgs = IDictionary<string, IFluentType>;

    public class FluentBundle
    {
        private Dictionary<string, FluentFunction> _funcList;
        private Dictionary<(string, EntryKind), IEntry> _entries;
        public CultureInfo Culture { get; internal init; }
        public List<string> Locales { get; internal init; }


        public bool UseIsolating { get; set; }
        public Func<string, string>? TransformFunc { get; set; }
        public Func<IFluentType, string>? FormatterFunc { get; init; }
        public byte MaxPlaceable { get; }

        internal FluentBundle()
        {
            Culture = CultureInfo.CurrentCulture;
            Locales = new List<string>();
            _entries = new Dictionary<(string, EntryKind), IEntry>();
            _funcList = new Dictionary<string, FluentFunction>();
            UseIsolating = true;
            MaxPlaceable = 100;
        }

        public FluentBundle(string locale, FluentBundleOption option, out List<FluentError> errors) : this()
        {
            Locales = new List<string>(1) {locale};
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
            var key = funcName;
            switch (behavior)
            {
                case InsertBehavior.None:
                    if (!_funcList.TryAdd(key, fluentFunction))
                    {
                        errors = new List<FluentError>
                        {
                            new OverrideFluentError(funcName, EntryKind.Unknown)
                        };
                    }

                    break;
                case InsertBehavior.Overriding:
                    _funcList[key] = fluentFunction;
                    break;
                default:
                    if (_funcList.ContainsKey(key))
                    {
                        errors.Add(new OverrideFluentError(funcName, EntryKind.Unknown));
                        return false;
                    }

                    _funcList.Add(key, fluentFunction);
                    break;
            }

            return true;
        }

        public bool AddResource(Resource res, [NotNullWhen(false)] out List<FluentError> errors)
        {
            errors = new List<FluentError>();
            foreach (var parseError in res.Errors)
            {
                errors.Add(ParserFluentError.ParseError(parseError));
            }

            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];
                switch (entry)
                {
                    case AstMessage message:
                        AddEntry(errors, message);
                        break;
                    case AstTerm term:
                        AddEntry(errors, term);
                        break;
                }
            }

            if (errors.Count == 0)
            {
                return true;
            }

            return false;
        }

        private void AddEntry(List<FluentError> errors, IEntry term, bool overwrite = false)
        {
            var id = (term.GetId(), term.ToKind());
            if (_entries.ContainsKey(id) && !overwrite)
            {
                errors.Add(new OverrideFluentError(id.Item1, id.Item2));
            }
            else
            {
                _entries[id] = term;
            }
        }

        public void AddResourceOverriding(Resource res)
        {
            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];

                if (entry is AstTerm or AstMessage)
                {
                    AddEntry(new List<FluentError>(), entry, true);
                }
            }
        }

        public bool HasMessage(string identifier)
        {
            var id = (identifier, EntryKind.Message);
            return _entries.ContainsKey(id)
                   && _entries[id] is AstMessage;
        }

        public bool TryGetAttrMsg(string msgWithAttr, FluentArgs? args,
            out IList<FluentError> errors, out string? message)
        {
            if (msgWithAttr.Contains('.'))
            {
                var split = msgWithAttr.Split('.');
                return TryGetMsg(split[0], split[1], args, out errors, out message);
            }

            return TryGetMsg(msgWithAttr, args, out errors, out message);
        }

        public bool TryGetMsg(string id, FluentArgs? args,
            out IList<FluentError> errors, [NotNullWhen(true)] out string? message)
        {
            return TryGetMsg(id, null, args, out errors, out message);
        }

        public bool TryGetMsg(string id, string? attribute, FluentArgs? args,
            out IList<FluentError> errors, [NotNullWhen(true)] out string? message)
        {
            string? value = null;
            errors = new List<FluentError>();

            if (TryGetAstMessage(id, out var astMessage))
            {
                var pattern = attribute != null
                    ? astMessage.GetAttribute(attribute)?.Value
                    : astMessage.Value;

                if (pattern == null)
                {
                    var msg = (attribute == null)
                        ? id
                        : $"{id}.{attribute}";
                    errors.Add(ResolverFluentError.NoValue($"{msg}"));
                    message = FluentNone.None.ToString();
                    return false;
                }

                value = FormatPattern(pattern, args, out errors);
            }

            message = value;
            return message != null;
        }

        public bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message)
        {
            var id = (ident, EntryKind.Message);
            if (_entries.ContainsKey(id)
                && _entries.TryGetValue(id, out var value)
                && value.ToKind() == EntryKind.Message)
            {
                return _entries[id].TryConvert(out message);
            }

            message = null;
            return false;
        }

        public bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? astTerm)
        {
            var termId = (ident, EntryKind.Term);
            if (_entries.ContainsKey(termId)
                && _entries.TryGetValue(termId, out var value)
                && value.ToKind() == EntryKind.Term)
            {
                return _entries[termId].TryConvert(out astTerm);
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
            if (_funcList.ContainsKey(funcName))
            {
                return _funcList.TryGetValue(funcName, out function);
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

        public IEnumerable<string> GetMessageEnumerable()
        {
            foreach (var keyValue in _entries)
            {
                if (keyValue.Value.ToKind() == EntryKind.Message)
                    yield return keyValue.Key.Item1;
            }
        }

        public IEnumerable<string> GetFuncEnumerable()
        {
            foreach (var keyValue in _funcList)
            {
                yield return keyValue.Key;
            }
        }

        public FluentBundle DeepClone()
        {
            return new()
            {
                Culture = (CultureInfo) Culture.Clone(),
                FormatterFunc = FormatterFunc,
                Locales = new List<string>(Locales),
                _entries = new(_entries),
                _funcList = new (_funcList),
                TransformFunc = TransformFunc,
                UseIsolating = UseIsolating,
            };
        }
    }
}
