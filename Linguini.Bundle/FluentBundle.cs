#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Linguini.Bundle.Entry;
using Linguini.Bundle.Errors;
using Linguini.Bundle.PluralRules;
using Linguini.Bundle.Resolver;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;

namespace Linguini.Bundle
{
    using FluentArgs = IDictionary<string, IFluentType>;
    
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
            Locales = new List<string>();
            Locales.Add(locale);
            Culture = new CultureInfo(locale, false);
            UseIsolating = option.UseIsolating;
            FormatterFunc = option.FormatterFunc;
            TransformFunc = option.TransformFunc;
            MaxPlaceable = option.MaxPlaceable;
            AddFunctions(option.Functions, out errors, InsertBehavior.None);
        }

        public void AddFunctions(IDictionary<string, FluentFunction> functions, out List<FluentError> errors,
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

        public bool AddFunction(string funcName, FluentFunction fluentFunction,
            [NotNullWhen(false)] out IList<FluentError>? errors,
            InsertBehavior behavior = InsertBehavior.Throw)
        {
            errors = null;
            switch (behavior)
            {
                case InsertBehavior.None:
                    if (!_entries.TryAdd(funcName, fluentFunction))
                    {
                        errors = new List<FluentError>
                        {
                            new OverrideFluentError(funcName, EntryKind.Function)
                        };
                    }

                    break;
                case InsertBehavior.Overriding:
                    _entries[funcName] = fluentFunction;
                    break;
                default:
                    if (_entries.ContainsKey(funcName))
                    {
                        errors = new List<FluentError>
                        {
                            new OverrideFluentError(funcName, EntryKind.Function)
                        };
                    }

                    _entries.Add(funcName, fluentFunction);
                    break;
            }

            _funcList.Add(funcName);
            return errors == null;
        }

        public bool AddResource(Resource res, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var resPos = Resources.Count;
            var accErrors = new List<FluentError>();
            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];
                var id = "";
                IBundleEntry bundleEntry;
                if (entry.TryConvert(out AstMessage message))
                {
                    id = message.GetId();
                    bundleEntry = new Message(resPos, entryPos);
                }
                else if (entry.TryConvert(out AstTerm term))
                {
                    id = term.GetId();
                    bundleEntry = new Term(resPos, entryPos);
                }
                else
                {
                    continue;
                }

                if (_entries.ContainsKey(id))
                {
                    accErrors.Add(new OverrideFluentError(id, _entries[id].ToKind()));
                }
                else
                {
                    _entries.Add(id, bundleEntry);
                }
            }

            Resources.Add(res);
            if (accErrors.Count == 0)
            {
                errors = null;
                return true;
            }

            errors = accErrors;
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
                if (entry.TryConvert(out AstMessage message))
                {
                    id = message.GetId();
                    bundleEntry = new Message(resPos, entryPos);
                }
                else if (entry.TryConvert(out AstTerm term))
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
            return Entries.ContainsKey(id)
                   && Entries[id].TryConvert<IBundleEntry, Message>(out _);
        }

        public bool TryGetMessage(string id, [NotNullWhen(true)] out AstMessage? message)
        {
            if (Entries.ContainsKey(id)
                && Entries.TryGetValue(id, out var value)
                && value.ToKind() == EntryKind.Message
                && value.TryConvert(out Message msg))
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
            if (Entries.ContainsKey(id)
                && Entries.TryGetValue(id, out var value)
                && value.ToKind() == EntryKind.Term
                && value.TryConvert(out Term term))
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
            if (Entries.ContainsKey(funcName)
                && Entries.TryGetValue(funcName, out var value)
                && value.ToKind() == EntryKind.Function)
            {
                return value.TryConvert(out function);
            }

            function = null;
            return false;
        }

        public bool TryWritePattern(TextWriter writer, Pattern pattern, FluentArgs? args,
            out IList<FluentError> errors)
        {
            var scope = new Scope(this, args);
            pattern.Write(writer, scope, out errors);

            return errors.Count == 0;
        }

        public string FormatPattern(Pattern pattern, FluentArgs? args,
            out IList<FluentError> errors)
        {
            errors = new List<FluentError>();
            var scope = new Scope(this, args);
            var value = pattern.Resolve(scope);
            return value.AsString();
        }

        public PluralCategory GetPluralRules(PluralRuleType cardinal, FluentNumber outType)
        {
            // TODO
            throw new NotImplementedException();
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
        public byte MaxPlaceable = 100;
    }

    public class LinguiniBundler
    {
        public static ILocaleStep New()
        {
            return new StepBuilder();
        }

        public interface IStep
        {
        }

        public interface ILocaleStep : IStep
        {
            IResourceStep Locale(string unparsedLocale);
            IResourceStep Locales(IList<string> unparsedLocales);
            IResourceStep CultureInfo(CultureInfo culture);
        }

        public interface IResourceStep : IStep
        {
            IReadyStep AddResource(string unparsedResource);
            IReadyStep AddResources(IList<string> unparsedResourceList);
            IReadyStep AddResource(TextReader unparsed);
            IReadyStep AddResources(IList<TextReader> unparsedStreamList);
            IReadyStep AddResource(Resource resource);
            IReadyStep AddResources(IList<Resource> resource);
        }

        public interface IBuildStep : IStep
        {
            FluentBundle UncheckedBuild();

            (FluentBundle, List<FluentError>) Build();
        }

        public interface IReadyStep : IBuildStep
        {
            IReadyStep SetUseIsolating(bool isIsolating);
            IReadyStep SetTransformFunc(Func<string, string> transformFunc);
            IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc);
        }

        private class StepBuilder : IReadyStep, ILocaleStep, IResourceStep
        {
            private CultureInfo _culture;
            private List<string> _locales = new();
            private List<Resource> _resources = new();
            private bool _useIsolating = true;
            private Func<IFluentType, string>? _formatterFunc;
            private Func<string, string>? _transformFunc;
            private Dictionary<string, FluentFunction> _functions = new();

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

            public FluentBundle UncheckedBuild()
            {
                var (bundle, errors) = Build();

                if (errors.Count > 0)
                {
                    throw new LinguiniException(errors);
                }

                return bundle;
            }

            public (FluentBundle, List<FluentError>) Build()
            {
                var bundle = new FluentBundle()
                {
                    Culture = _culture,
                    Locales = _locales,
                    UseIsolating = _useIsolating,
                    FormatterFunc = _formatterFunc,
                    TransformFunc = _transformFunc,
                };

                var errors = new List<FluentError>();
                if (_functions.Count > 0)
                {
                    bundle.AddFunctions(_functions, out var funcErr);
                    errors.AddRange(funcErr);
                }
                foreach (var resource in _resources)
                {
                    if (!bundle.AddResource(resource, out var resErr))
                    {
                        errors.AddRange(resErr);
                    }
                }

                return (bundle, errors);
            }

            public IResourceStep Locale(string unparsedLocale)
            {
                _culture = new CultureInfo(unparsedLocale);
                _locales.Add(unparsedLocale);
                return this;
            }

            public IResourceStep Locales(IList<string> unparsedLocales)
            {
                if (unparsedLocales.Count > 0)
                {
                    _culture = new CultureInfo(unparsedLocales[0]);
                    _locales.AddRange(unparsedLocales);
                }

                return this;
            }

            public IResourceStep CultureInfo(CultureInfo culture)
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
