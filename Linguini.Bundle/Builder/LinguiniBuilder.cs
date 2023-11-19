﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;

namespace Linguini.Bundle.Builder
{
    public static class LinguiniBuilder
    {
        public static ILocaleStep Builder(bool useExperimental = false)
        {
            return new StepBuilder(useExperimental);
        }

        public interface IStep
        {
        }

        public interface ILocaleStep : IStep
        {
            IResourceStep Locale(string unparsedLocale);
            IResourceStep Locales(IEnumerable<string> unparsedLocales);
            IResourceStep Locales(params string[] unparsedLocales);

            IResourceStep CultureInfo(CultureInfo culture);
        }

        public interface IResourceStep : IStep
        {
            IReadyStep AddResources(IEnumerable<string> unparsedResourceList);
            IReadyStep AddResources(params string[] unparsedResourceArray);
            IReadyStep AddResources(IEnumerable<TextReader> unparsedStreamList);
            IReadyStep AddResources(params TextReader[] unparsedStreamList);
            IReadyStep AddResource(string resource);
            IReadyStep AddResource(TextReader resource);
            IReadyStep AddResources(IEnumerable<Resource> resources);
            IReadyStep AddResources(params Resource[] resources);
            IReadyStep SkipResources();
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
            IReadyStep AddFunction(string name, ExternalFunction externalFunction);
            IReadyStep UseConcurrent();
        }

        private class StepBuilder : IReadyStep, ILocaleStep, IResourceStep
        {
            private CultureInfo _culture;
            private readonly List<string> _locales = new();
            private readonly List<Resource> _resources = new();
            private readonly List<object> _source = new();
            private bool _useIsolating;
            private Func<IFluentType, string>? _formatterFunc;
            private Func<string, string>? _transformFunc;
            private readonly Dictionary<string, ExternalFunction> _functions = new();
            private bool _concurrent;
            private readonly bool _enableExperimental;

            internal StepBuilder(bool useExperimental = false)
            {
                _culture = System.Globalization.CultureInfo.CurrentCulture;
                _enableExperimental = useExperimental;
            }

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

            public IReadyStep AddFunction(string name, ExternalFunction externalFunction)
            {
                _functions[name] = externalFunction;
                return this;
            }

            public IReadyStep UseConcurrent()
            {
                _concurrent = true;
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
                var concurrent = new FluentBundleOption
                {
                    FormatterFunc = _formatterFunc,
                    TransformFunc = _transformFunc,
                    Locales = _locales,
                    UseIsolating = _useIsolating,
                    UseConcurrent = _concurrent,
                    EnableExtensions = _enableExperimental,
                };
                var bundle = FluentBundle.MakeUnchecked(concurrent);
                bundle.Culture = _culture;

                var errors = new List<FluentError>();
                if (_functions.Count > 0)
                {
                    bundle.AddFunctions(_functions, out var funcErr);
                    errors.AddRange(funcErr);
                }

                foreach (var resource in _resources)
                {
                    if (!bundle.AddResource(resource,out var resErr))
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

            public IResourceStep Locales(IEnumerable<string> unparsedLocales)
            {
                return Locales(unparsedLocales.ToArray());
            }

            public IResourceStep Locales(params string[] unparsedLocales)
            {
                _locales.AddRange(unparsedLocales);
                if (_locales.Count > 0)
                {
                    // TODO proper culture info negotiations
                    _culture = new CultureInfo(_locales[0]);
                }
                else
                {
                    throw new ArgumentException("Expected at least one locale to be passed");
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
                var resource = new LinguiniParser(unparsed, _enableExperimental).Parse();
                _resources.Add(resource);
                return this;
            }

            public IReadyStep AddResource(TextReader unparsed)
            {
                var resource = new LinguiniParser(unparsed, _enableExperimental).Parse();
                _resources.Add(resource);
                return this;
            }

            public IReadyStep AddResources(params TextReader[] unparsedStreamList)
            {
                foreach (var unparsed in unparsedStreamList)
                {
                    var parsed = new LinguiniParser(unparsed, _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResources(IEnumerable<string> unparsedResources)
            {
                foreach (var unparsed in unparsedResources)
                {
                    var parsed = new LinguiniParser(unparsed, _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResources(params string[] unparsedResourceArray)
            {
                foreach (var unparsed in unparsedResourceArray)
                {
                    var parsed = new LinguiniParser(unparsed, _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResources(IEnumerable<TextReader> unparsedStream)
            {
                foreach (var unparsed in unparsedStream)
                {
                    var parsed = new LinguiniParser(unparsed, _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResources(IEnumerable<Resource> parsedResource)
            {
                _resources.AddRange(parsedResource);
                return this;
            }

            public IReadyStep AddResources(params Resource[] resources)
            {
                _resources.AddRange(resources);
                return this;
            }

            public IReadyStep SkipResources()
            {
                return this;
            }
        }
    }
}
