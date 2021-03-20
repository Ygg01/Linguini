#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using Linguini.Bundle.Entry;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Message = Linguini.Bundle.Entry.Message;

namespace Linguini.Bundle
{
    public class FluentBundle
    {
        private HashSet<string> _funcList;
        private readonly Dictionary<string, IBundleEntry> _entries;

        public CultureInfo Culture { get; internal set; }
        public List<string> Locales { get; internal set; }
        public List<Resource> Resources { get; }

        public IReadOnlyDictionary<string, IBundleEntry> Entries => _entries;

        public bool UseIsolating { get; set; }
        public Func<string, string>? TransformFunc { get; set; }
        public Func<IFluentType, string>? FormatterFunc { get; set; }

        internal FluentBundle()
        {
            Culture = CultureInfo.CurrentCulture;
            Locales = new List<string>();
            Resources = new List<Resource>();
            _entries = new Dictionary<string, IBundleEntry>();
            _funcList = new HashSet<string>();
            UseIsolating = true;
        }

        public FluentBundle(string locale, FluentBundleOption option) : this()
        {
            Locales = new List<string>();
            Locales.Add(locale);
            Culture = new CultureInfo(locale, false);
            UseIsolating = option.UseIsolating;
            FormatterFunc = option.FormatterFunc;
            TransformFunc = option.TransformFunc;
            AddFunctions(option.Functions, InsertBehavior.None);
        }

        public void AddFunctions(IDictionary<string, FluentFunction> functions,
            InsertBehavior behavior = InsertBehavior.Throw)
        {
            foreach (var keyValue in functions)
            {
                AddFunction(keyValue.Key, keyValue.Value, behavior);
            }
        }

        public void AddFunction(string funcName, FluentFunction fluentFunction,
            InsertBehavior behavior = InsertBehavior.Throw)
        {
            switch (behavior)
            {
                case InsertBehavior.None:
                    _entries.TryAdd(funcName, fluentFunction);
                    break;
                case InsertBehavior.Overriding:
                    _entries[funcName] = fluentFunction;
                    break;
                default:
                    _entries.Add(funcName, fluentFunction);
                    break;
            }

            _funcList.Add(funcName);
        }

        public void AddResource(Resource res)
        {
            var resPos = Resources.Count;
            for (var entryPos = 0; entryPos < res.Body.Count; entryPos++)
            {
                var entry = res.Body[entryPos];
                var id = "";
                IBundleEntry bundleEntry;
                if (entry.TryConvert(out Syntax.Ast.AstMessage message))
                {
                    id = message.GetId();
                    bundleEntry = new Entry.Message(resPos, entryPos);
                }
                else if (entry.TryConvert(out AstTerm term))
                {
                    id = term.GetId();
                    bundleEntry = new Entry.Term(resPos, entryPos);
                }
                else
                {
                    continue;
                }

                _entries.Add(id, bundleEntry);
            }

            Resources.Add(res);
        }

        public void AddResourceOverriding(Resource res)
        {
            var resPos = Resources.Count;
            for (var entryPos = 0; entryPos < res.Body.Count; entryPos++)
            {
                var entry = res.Body[entryPos];
                var id = "";
                IBundleEntry bundleEntry;
                if (entry.TryConvert(out Syntax.Ast.AstMessage message))
                {
                    id = message.GetId();
                    bundleEntry = new Entry.Message(resPos, entryPos);
                }
                else if (entry.TryConvert(out AstTerm term))
                {
                    id = term.GetId();
                    bundleEntry = new Entry.Term(resPos, entryPos);
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
            return Entries.ContainsKey(id) 
                   && Entries[id].TryConvert<IBundleEntry, Message>(out _);
        }

        public bool TryGetMessage(string id, [NotNullWhen(true)] out AstMessage? message)
        {
            if (Entries.ContainsKey(id)
                && Entries.TryGetValue(id, out var value)
                && value.TryConvert(out Message msg))
            {
                var res = Resources[msg.ResPos];
                var entry = res.Body[msg.EntryPos];

                return entry.TryConvert(out message);
            }

            message = null;
            return false;
        }
        
        public bool TryGetTerm(string id, [NotNullWhen(true)] out AstTerm? astTerm)
        {
            if (Entries.ContainsKey(id)
                && Entries.TryGetValue(id, out var value)
                && value.TryConvert(out Term term))
            {
                var res = Resources[term.ResPos];
                var entry = res.Body[term.EntryPos];

                return entry.TryConvert(out astTerm);
            }

            astTerm = null;
            return false;
        }

        public bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function)
        {
            if (Entries.ContainsKey(funcName)
                && Entries.TryGetValue(funcName, out var value))
            {
                
                return value.TryConvert(out function);
            }

            function = null;
            return false;
        }
    }

    public enum InsertBehavior : byte
    {
        None,
        Overriding,
        Throw,
    }

    public class FluentBundleOption
    {
        public bool UseIsolating = true;
        public IDictionary<string, FluentFunction> Functions { get; set; }
        public Func<IFluentType, string>? FormatterFunc;
        public Func<string, string>? TransformFunc;
    }

    public class LinguiniBundler
    {
        public interface IStep
        {
        }

        public interface ILocaleStep : IStep
        {
            IReadyStep Locale(string unparsedLocale);
            IReadyStep Locales(IList<string> unparsedLocales);
            IReadyStep CultureInfo(CultureInfo culture);
        }

        public interface IBuildStep : IStep
        {
            FluentBundle Build();
        }

        public interface IReadyStep : IBuildStep
        {
            IReadyStep SetUseIsolating(bool isIsolating);
            IReadyStep SetTransformFunc(Func<string, string> transformFunc);
            IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc);
        }

        private class StepBuilder : IReadyStep, ILocaleStep
        {
            private CultureInfo _culture;
            private List<string> _locales = new();
            private List<Resource> _resources = new();
            private bool _useIsolating = true;
            private Func<IFluentType, string>? _formatterFunc;
            private Func<string, string>? _transformFunc;
            private Dictionary<string, FluentFunction> _functions;
            private Dictionary<string, IBundleEntry> _entries;

            public IReadyStep SetUseIsolating(bool isIsolating)
            {
                _useIsolating = isIsolating;
                return this;
            }

            public IReadyStep SetTransformFunc(Func<string, string> transformFunc)
            {
                _transformFunc = transformFunc;
                return this;
            }

            public IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc)
            {
                _formatterFunc = formatterFunc;
                return this;
            }

            public FluentBundle Build()
            {
                var bundle = new FluentBundle()
                {
                    Culture = _culture,
                    Locales = _locales,
                    UseIsolating = _useIsolating,
                    FormatterFunc = _formatterFunc,
                    TransformFunc = _transformFunc,
                };

                bundle.AddFunctions(_functions);
                foreach (var resource in _resources)
                {
                    bundle.AddResource(resource);
                }

                return bundle;
            }

            public IReadyStep Locale(string unparsedLocale)
            {
                _culture = new CultureInfo(unparsedLocale);
                _locales.Add(unparsedLocale);
                return this;
            }

            public IReadyStep Locales(IList<string> unparsedLocales)
            {
                if (unparsedLocales.Count > 0)
                {
                    _culture = new CultureInfo(unparsedLocales[0]);
                    _locales.AddRange(unparsedLocales);
                }

                return this;
            }

            public IReadyStep CultureInfo(CultureInfo culture)
            {
                _culture = culture;
                _locales.Add(culture.ToString());
                return this;
            }

            public IReadyStep AddResource(string unparsed)
            {
                var resource = new LinguiniParser(unparsed).Parse();
                _resources.Add(resource);
                return this;
            }

            public IReadyStep AddResource(TextReader unparsed)
            {
                var resource = new LinguiniParser(unparsed).Parse();
                _resources.Add(resource);
                return this;
            }

            public IReadyStep AddResource(Resource parsedResource)
            {
                _resources.Add(parsedResource);
                return this;
            }

            public IReadyStep AddResources(IList<string> unparsedResources)
            {
                foreach (var unparsed in unparsedResources)
                {
                    var parsed = new LinguiniParser(unparsed).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResources(IList<TextReader> unparsedStream)
            {
                foreach (var unparsed in unparsedStream)
                {
                    var parsed = new LinguiniParser(unparsed).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResources(IList<Resource> parsedResource)
            {
                _resources.AddRange(parsedResource);
                return this;
            }
        }
    }
}
