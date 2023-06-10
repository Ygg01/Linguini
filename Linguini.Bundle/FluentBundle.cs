#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Resolver;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle
{
    using FluentArgs = IDictionary<string, IFluentType>;

    public class FluentBundle
    {
        private IDictionary<string, FluentFunction> _funcList;
        private IDictionary<(string, EntryKind), IEntry> _entries;

        #region Properties

        public CultureInfo Culture { get; internal set; }
        public List<string> Locales { get; internal set; }
        public bool UseIsolating { get; set; }
        public Func<string, string>? TransformFunc { get; set; }
        public Func<IFluentType, string>? FormatterFunc { get; init; }
        public byte MaxPlaceable { get; private init; }

        #endregion

        #region Constructors

        private FluentBundle()
        {
            _entries = new Dictionary<(string, EntryKind), IEntry>();
            _funcList = new Dictionary<string, FluentFunction>();
            Culture = CultureInfo.CurrentCulture;
            Locales = new List<string>();
            UseIsolating = true;
            MaxPlaceable = 100;
        }

        public static FluentBundle MakeUnchecked(FluentBundleOption option)
        {
            var bundle = ConstructBundle(option);
            bundle.AddFunctions(option.Functions, out _);
            return bundle;
        }

        private static FluentBundle ConstructBundle(FluentBundleOption option)
        {
            var primaryLocale = option.Locales.Count > 0
                ? option.Locales[0]
                : CultureInfo.CurrentCulture.Name;
            var cultureInfo = new CultureInfo(primaryLocale, false);
            var locales = new List<string> { primaryLocale };
            IDictionary<(string, EntryKind), IEntry> entries;
            IDictionary<string, FluentFunction> functions;
            if (option.UseConcurrent)
            {
                entries = new ConcurrentDictionary<(string, EntryKind), IEntry>();
                functions = new ConcurrentDictionary<string, FluentFunction>();
            }
            else
            {
                entries = new Dictionary<(string, EntryKind), IEntry>();
                functions = new Dictionary<string, FluentFunction>();
            }

            return new FluentBundle
            {
                Culture = cultureInfo,
                Locales = locales,
                _entries = entries,
                _funcList = functions,
                TransformFunc = option.TransformFunc,
                FormatterFunc = option.FormatterFunc,
                UseIsolating = option.UseIsolating,
                MaxPlaceable = option.MaxPlaceable,
            };
        }

        #endregion

        #region AddMethods

        public bool AddResource(Resource res, out List<FluentError> errors)
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

        public void AddResourceOverriding(Resource res)
        {
            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];

                if (entry is AstTerm or AstMessage)
                {
                    AddEntryOverriding(entry);
                }
            }
        }

        private void AddEntryOverriding(IEntry term)
        {
            var id = (term.GetId(), term.ToKind());
            _entries[id] = term;
        }

        private void AddEntry(List<FluentError> errors, IEntry term)
        {
            var id = (term.GetId(), term.ToKind());
            if (_entries.ContainsKey(id))
            {
                errors.Add(new OverrideFluentError(id.Item1, id.Item2));
            }
            else
            {
                _entries[id] = term;
            }
        }
        
        public bool TryAddFunction(string funcName, ExternalFunction fluentFunction)
        {
            return TryInsert(funcName, fluentFunction, InsertBehavior.None);
        }
        
        public bool AddFunctionOverriding(string funcName, ExternalFunction fluentFunction)
        {
            return TryInsert(funcName, fluentFunction, InsertBehavior.OverwriteExisting);
        }
        
        public bool AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction)
        {
            return TryInsert(funcName, fluentFunction);
        }

        public void AddFunctions(IDictionary<string, ExternalFunction> functions, out List<FluentError> errors)
        {
            errors = new List<FluentError>();
            foreach (var keyValue in functions)
            {
                if (!TryInsert(keyValue.Key, keyValue.Value, InsertBehavior.None))
                {
                    errors.Add(new OverrideFluentError(keyValue.Key, EntryKind.Func));
                }
            }
        }

        private bool TryInsert(string funcName, ExternalFunction fluentFunction,
            InsertBehavior behavior = InsertBehavior.ThrowOnExisting)
        {
            switch (behavior)
            {
#if NET5_0_OR_GREATER
                case InsertBehavior.None:
                    return _funcList.TryAdd(funcName, fluentFunction);
#else
                case InsertBehavior.None:
                    if (!_funcList.ContainsKey(funcName))
                    {
                        _funcList.Add(funcName, fluentFunction);
                        return true;
                    }

                    return false;
#endif
                case InsertBehavior.OverwriteExisting:
                    _funcList[funcName] = fluentFunction;
                    break;
                default:
                    if (_funcList.ContainsKey(funcName))
                    {
                        throw new KeyNotFoundException($"Function {funcName} already exists!");
                    }

                    _funcList.Add(funcName, fluentFunction);
                    break;
            }

            return true;
        }

        #endregion

        #region AttrMessage

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasMessage(string identifier)
        {
            var id = (identifier, EntryKind.Message);
            return _entries.ContainsKey(id)
                   && _entries[id] is AstMessage;
        }

        public bool HasAttrMessage(string idWithAttr)
        {
            var attributes = idWithAttr.IndexOf('.');
            if (attributes < 0)
            {
                return HasMessage(idWithAttr);
            }

            var id = idWithAttr.AsSpan(0, attributes).ToString();
            var attr = idWithAttr.AsSpan(attributes + 1).ToString();
            if (TryGetAstMessage(id, out var astMessage))
            {
                return astMessage.GetAttribute(attr) != null;
            }

            return false;
        }

        public string? GetAttrMessage(string msgWithAttr, FluentArgs? args = null)
        {
            TryGetAttrMessage(msgWithAttr, args, out var errors, out var message);
            if (errors.Count > 0)
            {
                throw new LinguiniException(errors);
            }

            return message;
        }

        public string? GetAttrMessage(string msgWithAttr, params (string, IFluentType)[] args)
        {
            var dictionary = new Dictionary<string, IFluentType>(args.Length);
            foreach (var (key, val) in args)
            {
                dictionary.Add(key, val);
            }

            TryGetAttrMessage(msgWithAttr, dictionary, out var errors, out var message);
            if (errors.Count > 0)
            {
                throw new LinguiniException(errors);
            }

            return message;
        }

        public bool TryGetAttrMessage(string msgWithAttr, FluentArgs? args,
            out IList<FluentError> errors, out string? message)
        {
            if (msgWithAttr.Contains("."))
            {
                var split = msgWithAttr.Split('.');
                return TryGetMessage(split[0], split[1], args, out errors, out message);
            }

            return TryGetMessage(msgWithAttr, null, args, out errors, out message);
        }

        public bool TryGetMessage(string id, FluentArgs? args,
            out IList<FluentError> errors, [NotNullWhen(true)] out string? message)
            => TryGetMessage(id, null, args, out errors, out message);

        public bool TryGetMessage(string id, string? attribute, FluentArgs? args,
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
                && value.ToKind() == EntryKind.Message
                && _entries[id] is AstMessage astMessage)
            {
                message = astMessage;
                return true;
            }

            message = null;
            return false;
        }

        public bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            var termId = (ident, EntryKind.Term);
            if (_entries.ContainsKey(termId)
                && _entries.TryGetValue(termId, out var value)
                && value.ToKind() == EntryKind.Term
                && _entries[termId] is AstTerm astTerm)
            {
                term = astTerm;
                return true;
            }

            term = null;
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

        #endregion

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
                Culture = (CultureInfo)Culture.Clone(),
                FormatterFunc = FormatterFunc,
                Locales = new List<string>(Locales),
                _entries = new Dictionary<(string, EntryKind), IEntry>(_entries),
                _funcList = new Dictionary<string, FluentFunction>(_funcList),
                TransformFunc = TransformFunc,
                UseIsolating = UseIsolating,
            };
        }
    }
}