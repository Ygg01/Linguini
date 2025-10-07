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
    /// <summary>
    ///     Static class providing a builder for creating Fluent bundles.
    /// </summary>
    public static class LinguiniBuilder
    {
        /// <summary>
        ///     Builder class for constructing Fluent bundles.
        /// </summary>
        /// <param name="useExperimental">
        ///     Sets the <see cref="FluentBundleOption.EnableExtensions" /> flag. Defaults to
        ///     <c>false</c>
        /// </param>
        /// <returns>The <see cref="ILocaleStep">next step (locale setting)</see> in the builder.</returns>
        public static ILocaleStep Builder(bool useExperimental = false)
        {
            return new StepBuilder(useExperimental);
        }

        /// <summary>
        ///     Common interface used in Fluent bundle builder.
        /// </summary>
        public interface IStep
        {
        }

        /// <summary>
        ///     First step in building Fluent Bundle, adding Locale.
        /// </summary>
        public interface ILocaleStep : IStep
        {
            /// <summary>
            ///     Sets the locale to a given string.
            /// </summary>
            /// <param name="unparsedLocale">The locale string.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep Locale(string unparsedLocale);

            /// <summary>
            ///     Sets the locale chain to given <c>IEnumerable&lt;string&gt;</c> of locale strings.
            /// </summary>
            /// <param name="unparsedLocales">The locale list.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep Locales(IEnumerable<string> unparsedLocales);

            /// <summary>
            ///     Sets the locale chain to given <c>params string[]</c> of locale string.
            /// </summary>
            /// <param name="unparsedLocales">The variable argument string list.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep Locales(params string[] unparsedLocales);


            /// <summary>
            ///     Sets the locale to given <see cref="CultureInfo" />.
            /// </summary>
            /// <param name="culture">The default <see cref="CultureInfo" /> of Bundle.</param>
            /// <returns>The <see cref="IResourceStep">next step (defining resources)</see> in the builder.</returns>
            IResourceStep CultureInfo(CultureInfo culture);

            /// <summary>
            ///     Retrieves current instance of the <see cref="ILocaleStep" />.
            /// </summary>
            /// <returns>A newly constructed <see cref="ILocaleStep" /> object.</returns>
            ILocaleStep GetLocaleStepBuilder();
        }

        /// <summary>
        ///     Usually the second step in building a Fluent bundle. It adds `Resourse` which
        ///     are files or strings that will be parsed, converted to Abstract Syntax Tree(AST),
        ///     and added to the Bundle.
        /// </summary>
        public interface IResourceStep : IStep
        {
            /// <summary>
            ///     Adds <see cref="IEnumerable{T}" /> of fragments to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="unparsedResourceList">Enumerable of string fragments</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResources(IEnumerable<string> unparsedResourceList);

            /// <summary>
            ///     Adds <see cref="IEnumerable{T}" /> of fragments to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="unparsedResourceArray">String array of string fragments</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResources(params string[] unparsedResourceArray);

            /// <summary>
            ///     Adds an enumerable list of <see cref="TextReader" /> to the builder.
            /// </summary>
            /// <param name="unparsedStreamList">An enumerable collection of unparsed resources.</param>
            /// <returns>An instance of <see cref="IReadyStep" /> representing the next step in the builder.</returns>
            [Obsolete(
                "Method is obsolete, please use IResourceStep.AddResources(IEnumerable<(TextReader, string?)>) instead.",
                true)]
            IReadyStep AddResources(IEnumerable<TextReader> unparsedStreamList);

            /// <summary>
            ///     Adds an array  of <see cref="TextReader" /> to the builder.
            /// </summary>
            /// <param name="unparsedStreamList">An array of unparsed text in <see cref="TextReader" />s.</param>
            /// <returns>An instance of <see cref="IReadyStep" /> representing the next step in the builder.</returns>
            [Obsolete(
                "Method is obsolete, please use IResourceStep.AddResources(params (TextReader, string?)[]) instead.",
                true)]
            IReadyStep AddResources(params TextReader[] unparsedStreamList);

            /// <summary>
            ///     Adds a fragments to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="resource">Content of a resource</param>
            /// <param name="filename">Quasi file name used to identify the fragment</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResource(string resource, string? filename = null);

            /// <summary>
            ///     Adds a fragments to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="resource">TextReader content of a resource</param>
            /// <param name="filename">Quasi file name used to identify the fragment</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResource(TextReader resource, string? filename = null);

            /// <summary>
            ///     Adds a <see cref="TextReader" /> to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="readerList">Array of <see cref="TextReader" /> and an optional file name.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResources(params (TextReader, string?)[] readerList);

            /// <summary>
            ///     Adds a <see cref="TextReader" /> to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="readerList"><see cref="IEnumerable{T}" /> of <see cref="TextReader" /> and an optional file name.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResources(IEnumerable<(TextReader, string?)> readerList);

            /// <summary>
            ///     Adds a file to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="path">A file on a path.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddFile(string path);

            /// <summary>
            ///     Adds several files to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="paths">An array of file paths.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddFiles(params string[] paths);

            /// <summary>
            ///     Adds several files to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="paths">An <see cref="Enumerable" /> of file paths.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddFiles(IEnumerable<string> paths);

            /// <summary>
            ///     Adds several parsed resources to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="resources">An <see cref="Enumerable" /> of <see cref="Resource" /> that are added to the bundle.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResources(IEnumerable<Resource> resources);

            /// <summary>
            ///     Adds several parsed resources to a <see cref="FluentBundle" />.
            /// </summary>
            /// <param name="resources">An array of <see cref="Resource" /> that are added to the bundle.</param>
            /// <returns>The <see cref="IReadyStep">next step (final)</see> in the builder.</returns>
            IReadyStep AddResources(params Resource[] resources);

            /// <summary>
            ///     Skip the resource adding step.
            /// </summary>
            /// <returns>The <see cref="IReadyStep">next step (final)</see></returns>
            IReadyStep SkipResources();

            /// <summary>
            ///     Retrieves current instance of the <see cref="IResourceStep" />.
            /// </summary>
            /// <returns>A newly constructed <see cref="IResourceStep" /> object.</returns>
            IResourceStep GetResourceStepBuilder();
        }


        /// <summary>
        ///     Usually the second step in building a Fluent bundle. It adds `Resourse` which
        ///     are files or strings that will be parsed, converted to Abstract Syntax Tree(AST),
        ///     and added to the Bundle.
        /// </summary>
        public interface IReadyStep : IBuildStep
        {
            /// <summary>
            ///     Sets Bidirectional isolation for bundle. See
            ///     <see cref="FluentBundleOption.UseIsolating">FluentBundleOption.UseIsolating</see>
            /// </summary>
            /// <param name="isIsolating">Sets BiDi isolation for bundle.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep SetUseIsolating(bool isIsolating);

            /// <summary>
            ///     Sets the transformation function. See
            ///     <see cref="FluentBundleOption.TransformFunc">FluentBundleOption.TransformFunc</see>.
            /// </summary>
            /// <param name="transformFunc">Transform function, which transforms text fragments into another string.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep SetTransformFunc(Func<string, string> transformFunc);

            /// <summary>
            ///     Sets the formatter function. See
            ///     <see cref="FluentBundleOption.FormatterFunc">FluentBundleOption.FormatterFunc</see>.
            /// </summary>
            /// <param name="formatterFunc">Formater function which can specify type specific formatters.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc);

            /// <summary>
            ///     Adds a function to the FluentBundle.
            /// </summary>
            /// <param name="name">The name of the custom function.</param>
            /// <param name="externalFunction">The delegate that represents the function.</param>
            /// <returns>The next step in the builder.</returns>
            IReadyStep AddFunction(string name, ExternalFunction externalFunction);

            /// <summary>
            ///     Makes current bundle builder concurrent.
            /// </summary>
            /// <returns>The next step in the builder.</returns>
            IReadyStep UseConcurrent();

            /// <summary>
            ///     Retrieves current instance of the <see cref="IResourceStep" />.
            /// </summary>
            /// <returns>A newly constructed <see cref="IResourceStep" /> object.</returns>
            IReadyStep GetReadyStepBuilder();
        }

        /// <summary>
        ///     Final step of Builder, constructs <see cref="FluentBundle" />.
        /// </summary>
        public interface IBuildStep : IStep
        {
            /// <summary>
            ///     Finalizes the build by creating a <see cref="FluentBundle" />. Throws exception is any errors
            ///     occured.
            /// </summary>
            /// <exception cref="LinguiniException">If there was an error during build.</exception>
            /// <returns>Completed <see cref="FluentBundle" /></returns>
            FluentBundle UncheckedBuild();

            /// <summary>
            ///     Finalizes the build by creating a tuple of <see cref="FluentBundle" /> and errors encountered.
            /// </summary>
            /// <returns>Completed <see cref="FluentBundle" /></returns>
            (FluentBundle, List<FluentError>?) Build();
        }

        /// <summary>
        ///     Class designed as a builder for constructing FluentBundle instances.
        ///     Provides a fluent interface to set up configurations, resources, locales, and other behaviors
        ///     required to build and configure a FluentBundle, using fluent interfaces. I.e. your autocomplete should guide
        ///     you to the right implementation
        /// </summary>
        public class StepBuilder : IReadyStep, ILocaleStep, IResourceStep
        {
            private readonly bool _enableExperimental;
            private readonly Dictionary<string, ExternalFunction> _functions = new();
            private readonly List<string> _locales = new();
            private readonly List<Resource> _resources = new();
            private bool _concurrent;
            private CultureInfo _culture;
            private Func<IFluentType, string>? _formatterFunc;
            private Func<string, string>? _transformFunc;
            private bool _useIsolating;

            internal StepBuilder(bool useExperimental = false)
            {
                _culture = System.Globalization.CultureInfo.CurrentCulture;
                _enableExperimental = useExperimental;
            }

            internal StepBuilder(StepBuilder other)
            {
                _culture = other._culture;
                _useIsolating = other._useIsolating;
                _concurrent = other._concurrent;
                _enableExperimental = other._enableExperimental;

                _locales = new List<string>(other._locales);
                _resources = new List<Resource>(other._resources);
                _functions = new Dictionary<string, ExternalFunction>(other._functions);
                _transformFunc = other._transformFunc;
                _formatterFunc = other._formatterFunc;
            }

            /// <inheritdoc />
            public ILocaleStep GetLocaleStepBuilder()
            {
                return new StepBuilder(this);
            }

            /// <inheritdoc />
            public IResourceStep Locale(string unparsedLocale)
            {
                _culture = new CultureInfo(unparsedLocale);
                _locales.Add(unparsedLocale);
                return this;
            }

            /// <inheritdoc />
            public IResourceStep Locales(IEnumerable<string> unparsedLocales)
            {
                return Locales(unparsedLocales.ToArray());
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public IResourceStep CultureInfo(CultureInfo culture)
            {
                _culture = culture;
                _locales.Add(culture.ToString());
                return this;
            }

            /// <inheritdoc />
            public IReadyStep SetUseIsolating(bool isIsolating)
            {
                _useIsolating = isIsolating;
                return this;
            }

            /// <inheritdoc />
            public IReadyStep SetTransformFunc(Func<string, string> transformFunc)
            {
                _transformFunc = transformFunc;
                return this;
            }

            /// <inheritdoc />
            public IReadyStep SetFormatterFunc(Func<IFluentType, string> formatterFunc)
            {
                _formatterFunc = formatterFunc;
                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddFunction(string name, ExternalFunction externalFunction)
            {
                _functions[name] = externalFunction;
                return this;
            }

            /// <inheritdoc />
            public IReadyStep UseConcurrent()
            {
                _concurrent = true;
                return this;
            }

            /// <inheritdoc />
            public IReadyStep GetReadyStepBuilder()
            {
                return new StepBuilder(this);
            }

            /// <inheritdoc />
            public FluentBundle UncheckedBuild()
            {
                var (bundle, errors) = Build();

                if (errors is { Count: > 0 })
                {
                    throw new LinguiniException(errors);
                }

                return bundle;
            }

            /// <inheritdoc />
            public (FluentBundle, List<FluentError>?) Build()
            {
                var concurrent = new FluentBundleOption
                {
                    FormatterFunc = _formatterFunc,
                    TransformFunc = _transformFunc,
                    Locales = _locales,
                    UseIsolating = _useIsolating,
                    UseConcurrent = _concurrent,
                    EnableExtensions = _enableExperimental
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

            /// <inheritdoc />
            public IResourceStep GetResourceStepBuilder()
            {
                return new StepBuilder(this);
            }

            /// <inheritdoc />
            public IReadyStep AddResources(params TextReader[] unparsedStreamList)
            {
                foreach (var unparsed in unparsedStreamList)
                {
                    var parsed = LinguiniParser.FromTextReader(unparsed, "???", _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResource(string resource, string? filename = null)
            {
                var parsed = LinguiniParser.FromFragment(resource, filename, _enableExperimental).Parse();
                _resources.Add(parsed);
                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResource(TextReader resource, string? filename = null)
            {
                var parsed = LinguiniParser.FromTextReader(resource, filename ?? "????", _enableExperimental).Parse();
                _resources.Add(parsed);

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(params (TextReader, string?)[] unparsedStreamList)
            {
                foreach (var (reader, inputName) in unparsedStreamList)
                {
                    var parsed = LinguiniParser.FromTextReader(reader, inputName ?? "???", _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(IEnumerable<(TextReader, string?)> unparsedStreamList)
            {
                foreach (var (reader, inputName) in unparsedStreamList)
                {
                    var parsed = LinguiniParser.FromTextReader(reader, inputName ?? "???", _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddFile(string path)
            {
                var parsed = LinguiniParser.FromFile(path, _enableExperimental).Parse();
                _resources.Add(parsed);

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddFiles(params string[] paths)
            {
                foreach (var path in paths)
                {
                    var parsed = LinguiniParser.FromFile(path).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddFiles(IEnumerable<string> paths)
            {
                foreach (var path in paths)
                {
                    var parsed = LinguiniParser.FromFile(path).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(IEnumerable<string> unparsedResources)
            {
                foreach (var unparsed in unparsedResources)
                {
                    var parsed = LinguiniParser.FromFragment(unparsed, enableExperimental: _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(params string[] unparsedResourceArray)
            {
                foreach (var unparsed in unparsedResourceArray)
                {
                    var parsed = LinguiniParser.FromFragment(unparsed, enableExperimental: _enableExperimental).Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(IEnumerable<TextReader> unparsedStream)
            {
                foreach (var unparsed in unparsedStream)
                {
                    var parsed = LinguiniParser.FromTextReader(unparsed, "???", _enableExperimental)
                        .Parse();
                    _resources.Add(parsed);
                }

                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(IEnumerable<Resource> parsedResource)
            {
                _resources.AddRange(parsedResource);
                return this;
            }

            /// <inheritdoc />
            public IReadyStep AddResources(params Resource[] resources)
            {
                _resources.AddRange(resources);
                return this;
            }

            /// <inheritdoc />
            public IReadyStep SkipResources()
            {
                return this;
            }
        }
    }
}