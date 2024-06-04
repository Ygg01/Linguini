using System;
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
        /// <summary>
        /// Builder class for constructing Fluent bundles.
        /// </summary>
        /// <param name="useExperimental">Sets the <see cref="FluentBundleOption.EnableExtensions"/> flag. Defaults to <c>false</c></param>
        /// <returns>The <see cref="ILocaleStep">next step (locale setting)</see> in the builder.</returns>
        public static ILocaleStep Builder(bool useExperimental = false)
        {
            return new StepBuilder(useExperimental);
        }

        public interface IStep
        {
        }

        public interface ILocaleStep : IStep
        {
            /// <summary>
            /// Sets the locale to given string.
            /// </summary>
            /// <param name="unparsedLocale">The locale string.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep Locale(string unparsedLocale);

            /// <summary>
            /// Sets the locale chain to given <c>IEnumerable&lt;string&gt;</c> of locale strings.
            /// </summary>
            /// <param name="unparsedLocales">The locale list.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep Locales(IEnumerable<string> unparsedLocales);

            /// <summary>
            /// Sets the locale chain to given <c>params string[]</c> of locale string.
            /// </summary>
            /// <param name="unparsedLocales">The variable argument string uost.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep Locales(params string[] unparsedLocales);


            /// <summary>
            /// Sets the locale to given <see cref="CultureInfo"/>.
            /// </summary>
            /// <param name="culture">The default <see cref="CultureInfo"/> of Bundle.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep CultureInfo(CultureInfo culture);
        }

        public interface IResourceStep : IStep
        {
            [Obsolete]
            IReadyStep AddResources(IEnumerable<string> unparsedResourceList);

            [Obsolete]
            IReadyStep AddResources(params string[] unparsedResourceArray);

            [Obsolete]
            IReadyStep AddResources(IEnumerable<TextReader> unparsedStreamList);

            [Obsolete]
            IReadyStep AddResources(params TextReader[] unparsedStreamList);

            IReadyStep AddResource(string resource, string? filename = null);

            IReadyStep AddResource(TextReader resource, string? filename = null);


            IReadyStep AddResources(params (TextReader, string?)[] readerList);
            
            IReadyStep AddResources(IEnumerable<(TextReader, string?)> readerList);

            IReadyStep AddFile(string path);
            
            IReadyStep AddFiles(params string[] path);
            
            IReadyStep AddFiles(IEnumerable<string> path);
            IReadyStep AddResources(IEnumerable<Resource> resources);
            IReadyStep AddResources(params Resource[] resources);

            /// <summary>
            /// Skip the resource adding step.
            /// </summary>
            /// <returns>The <see cref="IReadyStep">next step (final)</see></returns>
            IReadyStep SkipResources();
        }


        public interface IReadyStep : IBuildStep
        {
            /// <summary>
            /// Sets Bidirectional isolation for bundle. See <see cref="FluentBundleOption.UseIsolating">FluentBundleOption.UseIsolating</see>
            /// </summary>
            /// <param name="isIsolating">Sets BiDi isolation for bundle.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep SetUseIsolating(bool isIsolating);

            /// <summary>
            /// Sets the transformation function. See <see cref="FluentBundleOption.TransformFunc">FluentBundleOption.TransformFunc</see>.
            /// </summary>
            /// <param name="transformFunc">Transform function, which transforms text fragments into another string.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep SetTransformFunc(Func<string, string> transformFunc);

            /// <summary>
            /// Sets the formatter function. See <see cref="FluentBundleOption.FormatterFunc">FluentBundleOption.FormatterFunc</see>.
            /// </summary>
            /// <param name="formatterFunc">Formater function which can specify type specific formatters.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc);

            /// <summary>
            /// Adds a function to the FluentBundle.
            /// </summary>
            /// <param name="name">The name of the custom function.</param>
            /// <param name="externalFunction">The delegate that represents the function.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep AddFunction(string name, ExternalFunction externalFunction);

            /// <summary>
            /// Makes current bundle builder concurrent.
            /// </summary>
            /// <returns>The next step in the builder.</returns>
            IReadyStep UseConcurrent();
        }

        /// <summary>
        /// Final step of Builder, constructs <see cref="FluentBundle"/>.
        /// </summary>
        public interface IBuildStep : IStep
        {
            /// <summary>
            /// Finalizes the build by creating a <see cref="FluentBundle"/>. Throws exception is any errors
            /// occured.
            /// </summary>
            /// <exception cref="LinguiniException">If there was an error during build.</exception>
            /// <returns>Completed <see cref="FluentBundle"/></returns>
            FluentBundle UncheckedBuild();

            /// <summary>
            /// Finalizes the build by creating a tuple of <see cref="FluentBundle"/> and errors encountered.
            /// </summary>
            /// <returns>Completed <see cref="FluentBundle"/></returns>
            (FluentBundle, List<FluentError>?) Build();
        }

        private class StepBuilder : IReadyStep, ILocaleStep, IResourceStep
        {
            private CultureInfo _culture;
            private readonly List<string> _locales = new();
            private readonly List<Resource> _resources = new();
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

            /// <inheritdoc/>
            public IReadyStep SetUseIsolating(bool isIsolating)
            {
                _useIsolating = isIsolating;
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep SetTransformFunc(Func<string, string> transformFunc)
            {
                _transformFunc = transformFunc;
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc)
            {
                _formatterFunc = formatterFunc;
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep AddFunction(string name, ExternalFunction externalFunction)
            {
                _functions[name] = externalFunction;
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep UseConcurrent()
            {
                _concurrent = true;
                return this;
            }

            /// <inheritdoc/>
            public FluentBundle UncheckedBuild()
            {
                var (bundle, errors) = Build();

                if (errors is { Count: > 0 })
                {
                    throw new LinguiniException(errors);
                }

                return bundle;
            }

            /// <inheritdoc/>
            public (FluentBundle, List<FluentError>?) Build()
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
                List<FluentError>? errors = null;

                if (_functions.Count > 0)
                {
                    bundle.AddFunctions(_functions, out var funcErr);
                    if (funcErr != null)
                    {
                        errors ??= new List<FluentError>();
                        errors.AddRange(funcErr);
                    }
                }

                foreach (var resource in _resources)
                {
                    if (!bundle.AddResource(resource, out var resErr))
                    {
                        errors ??= new List<FluentError>();
                        errors.AddRange(resErr);
                    }
                }

                return (bundle, errors);
            }

            /// <inheritdoc/>
            public IResourceStep Locale(string unparsedLocale)
            {
                _culture = new CultureInfo(unparsedLocale);
                _locales.Add(unparsedLocale);
                return this;
            }

            /// <inheritdoc/>
            public IResourceStep Locales(IEnumerable<string> unparsedLocales)
            {
                return Locales(unparsedLocales.ToArray());
            }

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public IResourceStep CultureInfo(CultureInfo culture)
            {
                _culture = culture;
                _locales.Add(culture.ToString());
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep AddResources(params TextReader[] unparsedStreamList)
            {
                foreach (var unparsed in unparsedStreamList)
                {
                    var parsed = LinguiniParser.FromTextReader(unparsed, "???", _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            public IReadyStep AddResource(string resource, string? filename = null)
            {
                throw new NotImplementedException();
            }

            public IReadyStep AddResource(TextReader resource, string? filename = null)
            {
                throw new NotImplementedException();
            }

            public IReadyStep AddResources(params (TextReader, string?)[] readerList)
            {
                throw new NotImplementedException();
            }

            public IReadyStep AddResources(IEnumerable<(TextReader, string?)> readerList)
            {
                throw new NotImplementedException();
            }

            public IReadyStep AddFile(string path)
            {
                throw new NotImplementedException();
            }

            public IReadyStep AddFiles(params string[] path)
            {
                throw new NotImplementedException();
            }

            public IReadyStep AddFiles(IEnumerable<string> path)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public IReadyStep AddResources(IEnumerable<string> unparsedResources)
            {
                foreach (var unparsed in unparsedResources)
                {
                    var parsed = LinguiniParser.FromFragment(unparsed, enableExperimental: _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc/>
            public IReadyStep AddResources(params string[] unparsedResourceArray)
            {
                foreach (var unparsed in unparsedResourceArray)
                {
                    var parsed = LinguiniParser.FromFragment(unparsed, enableExperimental: _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc/>
            public IReadyStep AddResources(IEnumerable<TextReader> unparsedStream)
            {
                foreach (var unparsed in unparsedStream)
                {
                    var parsed = LinguiniParser.FromTextReader(unparsed, "???", enableExperimental: _enableExperimental)
                        .Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc/>
            public IReadyStep AddResources(IEnumerable<Resource> parsedResource)
            {
                _resources.AddRange(parsedResource);
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep AddResources(params Resource[] resources)
            {
                _resources.AddRange(resources);
                return this;
            }

            /// <inheritdoc/>
            public IReadyStep SkipResources()
            {
                return this;
            }
        }
    }
}