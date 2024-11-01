﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Resolver;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
// ReSharper disable MemberCanBePrivate.Global

namespace Linguini.Bundle
{
    /// <summary>
    /// Abstract base class for Fluent message bundles.
    /// </summary>
    public abstract class FluentBundle : IEquatable<FluentBundle>, IReadBundle
    {
        /// <summary>
        ///     <see cref="CultureInfo" /> of the bundle. Primary bundle locale
        /// </summary>
        public CultureInfo Culture { get; internal set; } = CultureInfo.CurrentCulture;

        /// <summary>
        ///     List of Locales. First element is primary bundle locale, others are fallback locales.
        /// </summary>
        public List<string> Locales { get; internal set; } = new();

        /// <summary>
        ///     When formatting patterns, FluentBundle inserts Unicode Directionality Isolation Marks to indicate that the
        ///     direction of a placeable may differ from the surrounding message.
        ///     This is important for cases such as when a right-to-left user name is presented in the left-to-right message.
        /// </summary>
        public bool UseIsolating { get; set; } = true;

        /// <summary>
        ///     Specifies a method that will be applied only on values extending <see cref="IFluentType" />. Useful for defining a
        ///     special formatter for <see cref="FluentNumber" />.
        /// </summary>
        public Func<string, string>? TransformFunc { get; set; }

        /// <summary>
        ///     Specifies a method that will be applied only on values extending <see cref="IFluentType" />. Useful for defining a
        ///     special formatter for <see cref="FluentNumber" />.
        /// </summary>
        public Func<IFluentType, string>? FormatterFunc { get; init; }

        /// <summary>
        ///     Limit of placeable <see cref="AstTerm" /> within one <see cref="Pattern" />, when fully expanded (all nested
        ///     elements count towards it). Useful for preventing billion laughs attack. Defaults to 100.
        /// </summary>
        public byte MaxPlaceable { get; internal init; } = 100;

        /// <summary>
        ///     Whether experimental features are enabled.
        ///     When `true` experimental features are enabled. Experimental features include stuff like:
        ///     <list type="bullet">
        ///         <item>dynamic reference</item>
        ///         <item>dynamic reference attributes</item>
        ///         <item>term reference as parameters</item>
        ///     </list>
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public bool EnableExtensions { get; init; }

        /// <summary>
        ///     Determines if the provided identifier has a message associated with it.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>True if the identifier has a message; otherwise, false.</returns>
        public abstract bool HasMessage(string identifier);

        /// <summary>
        ///     Tries to get the AstMessage associated with the specified ident.
        /// </summary>
        /// <param name="ident">The identifier to look for.</param>
        /// <param name="message">
        ///     When this method returns, contains the AstMessage associated with the specified ident, if found;
        ///     otherwise, null.
        /// </param>
        /// <returns>True if an AstMessage was found for the specified ident; otherwise, false.</returns>
        public abstract bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message);

        /// <summary>
        ///     Tries to get a term by its identifier.
        /// </summary>
        /// <param name="ident">The identifier of the AST term.</param>
        /// <param name="term">
        ///     When this method returns, contains the AST term associated with the specified identifier, if the
        ///     identifier is found; otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if the identifier is found and the corresponding AST term is retrieved; otherwise, false.</returns>
        public abstract bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term);

        /// <summary>
        ///     Tries to get the FluentFunction associated with the specified Identifier.
        /// </summary>
        /// <param name="id">The Identifier used to identify the FluentFunction.</param>
        /// <param name="function">
        ///     When the method returns, contains the FluentFunction associated with the Identifier, if the
        ///     Identifier is found; otherwise, null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if the FluentFunction associated with the Identifier is found; otherwise, false.</returns>
        public bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function)
        {
            return TryGetFunction(id.ToString(), out function);
        }

        /// <summary>
        ///     Tries to retrieve a FluentFunction object by the given function name.
        /// </summary>
        /// <param name="funcName">The name of the function to retrieve.</param>
        /// <param name="function">An output parameter that will hold the retrieved FluentFunction object, if found.</param>
        /// <returns>
        ///     True if a FluentFunction object with the specified name was found and assigned to the function output parameter.
        ///     False if a FluentFunction object with the specified name was not found.
        /// </returns>
        public abstract bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function);

        /// <summary>
        /// Formats a Fluent pattern using the provided arguments and returns the formatted string.
        /// </summary>
        /// <param name="pattern">The <see cref="Pattern"/> to be formatted.</param>
        /// <param name="args">The dictionary of arguments to be used for formatting.</param>
        /// <param name="errors">
        /// When the formatting fails due to errors, this parameter is set to a list of FluentError instances
        /// describing the encountered errors. Otherwise, it is set to null.
        /// </param>
        /// <returns>
        /// The formatted string if the pattern is successfully resolved; otherwise, null.
        /// </returns>
        public string FormatPatternErrRef(Pattern pattern, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] ref IList<FluentError>? errors)
        {
            var scope = new Scope(this, args);
            var value = pattern.Resolve(scope);
            errors = scope.Errors;
            return value.AsString();
        }

        /// <summary>
        ///     This method retrieves an enumerable collection of all message identifiers.
        /// </summary>
        /// <returns>
        ///     An enumerable collection of message identifiers.
        /// </returns>
        public abstract IEnumerable<string> GetMessageEnumerable();

        /// <summary>
        ///     Retrieves an enumerable collection of string function names.
        /// </summary>
        /// <returns>An enumerable collection of functions names.</returns>
        public abstract IEnumerable<string> GetFuncEnumerable();

        /// <summary>
        ///     Retrieves an enumerable collection of terms.
        /// </summary>
        /// <returns>An enumerable collection of terms.</returns>
        public abstract IEnumerable<string> GetTermEnumerable();

        /// <summary>
        ///  Parses a <see cref="string"/> input and adds the provided Resources to the bundle.
        /// </summary>
        /// <param name="input">The string input representing a Fluent template.</param>
        /// <param name="errors">Upon method completion, contains a list of any errors that occurred during the resource addition. If no errors occurred, the value is null.</param>
        /// <returns>True if the resource was added successfully; otherwise, false.</returns>
        /// <seealso cref="AddResource(string,out System.Collections.Generic.List{Linguini.Bundle.Errors.FluentError}?)"/>
        public bool AddResource(string input, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            return AddResource(res, out errors);
        }

        /// <summary>
        /// Parses a <see cref="TextReader"/> and adds it to the FluentBundle.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> representing the Fluent resource.</param>
        /// <param name="errors">The list of Fluent errors, if any.</param>
        /// <returns>True if the resource was added successfully; otherwise, false.</returns>
        /// <seealso cref="AddResource(string,out System.Collections.Generic.List{Linguini.Bundle.Errors.FluentError}?)"/>
        public bool AddResource(TextReader reader, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var res = new LinguiniParser(reader, EnableExtensions).Parse();
            return AddResource(res, out errors);
        }

        /// <summary>
        /// Adds a resource to the FluentBundle.
        /// </summary>
        /// <param name="resource">The input string containing the resource.</param>
        /// <param name="errors">The list of <see cref="FluentError"/>s encountered during parsing, if any.</param>
        /// <returns>True if the resource was successfully added; otherwise, false.</returns>
        public bool AddResource(Resource resource, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var innerErrors = new List<FluentError>();
            foreach (var parseError in resource.Errors) innerErrors.Add(ParserFluentError.ParseError(parseError));

            foreach (var entry in resource.Entries)
            {
                switch (entry)
                {
                    case AstMessage message:
                        TryAddMessage(message, innerErrors);
                        break;
                    case AstTerm term:
                        TryAddTerm(term, innerErrors);
                        break;
                }
            }

            if (innerErrors.Count == 0)
            {
                errors = null;
                return true;
            }

            errors = innerErrors;
            return false;
        }

        
        /// <summary>
        ///     Adds the given AstMessage to the collection of messages, by overriding any existing messages with the same name.
        /// </summary>
        /// <param name="message">The AstMessage to be added.</param>
        protected abstract void AddMessageOverriding(AstMessage message);

        /// <summary>
        ///     Adds a term to the AstTerm list, overriding any existing term with the same name.
        /// </summary>
        /// <param name="term">The term to be added.</param>
        protected abstract void AddTermOverriding(AstTerm term);



        /// <summary>
        /// Adds a <c>string</c> resource to the FluentBundle, overriding any existing messages and terms with the same identifiers.
        /// </summary>
        /// <param name="input">The resource content to add.</param>
        public void AddResourceOverriding(string input)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            AddResourceOverriding(res);
        }

        /// <summary>
        /// Adds a <see cref="TextReader"/> to the FluentBundle, overriding any existing messages and terms with the same identifiers.
        /// </summary>
        /// <param name="input">The text reader to be added to parsed and added to bundle.</param>
        public void AddResourceOverriding(TextReader input)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            AddResourceOverriding(res);
        }
        
        /// <summary>
        /// Adds a <see cref="Resource"/> to the FluentBundle, overriding any existing messages and terms with the same identifiers.
        /// </summary>
        /// <param name="resource">The resource content to add.</param>
        public void AddResourceOverriding(Resource resource)
        {
            for (var entryPos = 0; entryPos < resource.Entries.Count; entryPos++)
            {
                var entry = resource.Entries[entryPos];

                switch (entry)
                {
                    case AstMessage message:
                        AddMessageOverriding(message);
                        break;
                    case AstTerm term:
                        AddTermOverriding(term);
                        break;
                }
            }
        }

        /// <summary>
        /// Tries to add a term to the bundle.
        /// </summary>
        /// <param name="term">The term to add.</param>
        /// <param name="errors">A list to store any errors that occur during the <c>TryAdd</c> operation.</param>
        /// <returns><see langword="true"/> if the term was added successfully, <see langword="false"/> otherwise.</returns>
        protected abstract bool TryAddTerm(AstTerm term, [NotNullWhen(false)] List<FluentError>? errors);

        /// <summary>
        /// Tries to add a message to the bundle.
        /// </summary>
        /// <param name="msg">The message to add.</param>
        /// <param name="errors">A list to store any errors that occur during the <c>TryAdd</c> operation.</param>
        /// <returns><see langword="true"/> if the message was added successfully, <see langword="false"/> otherwise.</returns>
        protected abstract bool TryAddMessage(AstMessage msg, [NotNullWhen(false)] List<FluentError>? errors);


        /// <summary>
        /// Tries to add a custom function to the FluentBundle. If it fails, it will return false.
        /// </summary>
        /// <param name="funcName">The name by which FluentBundle can refer to it.</param>
        /// <param name="fluentFunction">The <see cref="ExternalFunction"/> that will be added.</param>
        /// <returns>True if the function was added successfully; otherwise, if for example function already exist, returns false.</returns>
        public abstract bool TryAddFunction(string funcName, ExternalFunction fluentFunction);

        /// <summary>
        /// Adds a function to fluent Bundle unlike <see cref="TryAddFunction"/> it will not fail, but
        /// override existing function.
        /// </summary>
        /// <param name="funcName">The name of the function to insert or override.</param>
        /// <param name="fluentFunction">The  <see cref="ExternalFunction"/> that will be inserted.</param>
        public abstract void AddFunctionOverriding(string funcName, ExternalFunction fluentFunction);

        /// <summary>
        /// Adds external function to the FluentBundle. If function already exist an exception will be raised.
        /// </summary>
        /// <param name="funcName">The name of the function.</param>
        /// <param name="fluentFunction">The <see cref="ExternalFunction"/>  to add.</param>
        public abstract void AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction);
        
        internal abstract IDictionary<string, AstMessage> GetMessagesDictionary();
        internal abstract IDictionary<string, AstTerm> GetTermsDictionary();
        internal abstract IDictionary<string, FluentFunction> GetFunctionDictionary();

        /// <summary>
        ///     Creates a deep clone of the current instance of the AbstractFluentBundle class.
        /// </summary>
        /// <returns>A new instance of the AbstractFluentBundle class that is a deep clone of the current instance.</returns>
        public abstract FluentBundle DeepClone();

        /// <summary>
        ///     Adds a collection of external functions to the dictionary of available functions.
        ///     If any function cannot be added, the errors are returned in the list.
        /// </summary>
        /// <param name="functions">A dictionary of function names and their corresponding ExternalFunction objects.</param>
        /// <param name="errors">A list of errors indicating functions that could not be added.</param>
        public void AddFunctions(IDictionary<string, ExternalFunction> functions, out List<FluentError>? errors)
        {
            errors = new List<FluentError>();
            foreach (var keyValue in functions)
                if (!TryAddFunction(keyValue.Key, keyValue.Value))
                    errors.Add(new OverrideFluentError(keyValue.Key, EntryKind.Func));
        }



        /// <summary>
        ///     Creates a FluentBundle object with the specified options.
        /// </summary>
        /// <param name="option">The FluentBundleOption object that contains the options for creating the FluentBundle</param>
        /// <returns>A FluentBundle object created with the specified options</returns>
        public static FluentBundle MakeUnchecked(FluentBundleOption option)
        {
            var primaryLocale = option.Locales.Count > 0
                ? option.Locales[0]
                : CultureInfo.CurrentCulture.Name;
            var cultureInfo = new CultureInfo(primaryLocale, false);
            var func = option.Functions.ToDictionary(x => x.Key, x => (FluentFunction)x.Value);
            return option.UseConcurrent switch
            {
                true => new ConcurrentBundle
                {
                    Locales = option.Locales,
                    Culture = cultureInfo,
                    EnableExtensions = option.EnableExtensions,
                    FormatterFunc = option.FormatterFunc,
                    TransformFunc = option.TransformFunc,
                    MaxPlaceable = option.MaxPlaceable,
                    UseIsolating = option.UseIsolating,
                    Functions = new ConcurrentDictionary<string, FluentFunction>(func)
                },
                _ => new NonConcurrentBundle
                {
                    Locales = option.Locales,
                    Culture = cultureInfo,
                    EnableExtensions = option.EnableExtensions,
                    FormatterFunc = option.FormatterFunc,
                    TransformFunc = option.TransformFunc,
                    MaxPlaceable = option.MaxPlaceable,
                    UseIsolating = option.UseIsolating,
                    Functions = func
                }
            };
        }

        /// <summary>
        /// Converts the current object to a <see cref="FrozenBundle"/>.
        /// </summary>
        /// <returns>A new instance of FrozenBundle.</returns>
        public FrozenBundle ToFrozenBundle()
        {
            return new FrozenBundle(this);
        }

        /// <inheritdoc />
        public bool Equals(FluentBundle? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Culture.Equals(other.Culture) && Locales.SequenceEqual(other.Locales) &&
                   UseIsolating == other.UseIsolating && Equals(TransformFunc, other.TransformFunc) &&
                   Equals(FormatterFunc, other.FormatterFunc) && MaxPlaceable == other.MaxPlaceable &&
                   EnableExtensions == other.EnableExtensions;
        }
        
        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FluentBundle)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Culture, Locales, UseIsolating, TransformFunc, FormatterFunc, MaxPlaceable,
                EnableExtensions);
        }
    }
}