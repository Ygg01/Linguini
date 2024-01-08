using System;
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

namespace Linguini.Bundle
{
    public interface IReadBundle
    {
        /// <summary>
        /// Determines if the provided identifier has a message associated with it. </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>True if the identifier has a message; otherwise, false.</returns>
        /// 
        bool HasMessage(string identifier);

        /// <summary>
        /// Determines whether the given identifier with attribute has a message.
        /// </summary>
        /// <param name="idWithAttr">The identifier with attribute.</param>
        /// <returns>True if the identifier with attribute has a message; otherwise, false.</returns>
        bool HasAttrMessage(string idWithAttr);

        /// <summary>
        /// Retrieves the attribute message by processing the given message template with the provided arguments.
        /// </summary>
        /// <param name="msgWithAttr">The string consisting of `messageId.Attribute`.</param>
        /// <param name="args">The dictionary of arguments to be used for resolution in the message template. Can be null.</param>
        /// <returns>The processed message.</returns>
        /// <exception cref="LinguiniException">Thrown when there are errors encountered during attribute substitution.</exception>
        string? GetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args = null);

        string? GetAttrMessage(string msgWithAttr, params (string, IFluentType)[] args);

        bool TryGetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message);

        bool TryGetMessage(string id, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message);

        bool TryGetMessage(string id, string? attribute, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message);

        /// <summary>
        /// Tries to get the AstMessage associated with the specified ident.
        /// </summary>
        /// <param name="ident">The identifier to look for.</param>
        /// <param name="message">When this method returns, contains the AstMessage associated with the specified ident, if found; otherwise, null.</param>
        /// <returns>True if an AstMessage was found for the specified ident; otherwise, false.</returns>
        bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message);

        /// <summary>
        /// Tries to get a term by its identifier.
        /// </summary>
        /// <param name="ident">The identifier of the AST term.</param>
        /// <param name="term">When this method returns, contains the AST term associated with the specified identifier, if the identifier is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the identifier is found and the corresponding AST term is retrieved; otherwise, false.</returns>
        bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term);

        /// <summary>
        /// Tries to get the FluentFunction associated with the specified Identifier.
        /// </summary>
        /// <param name="id">The Identifier used to identify the FluentFunction.</param>
        /// <param name="function">When the method returns, contains the FluentFunction associated with the Identifier, if the Identifier is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the FluentFunction associated with the Identifier is found; otherwise, false.</returns>
        bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function);

        /// <summary>
        /// Tries to retrieve a FluentFunction object by the given function name.
        /// </summary>
        /// <param name="funcName">The name of the function to retrieve.</param>
        /// <param name="function">An output parameter that will hold the retrieved FluentFunction object, if found.</param>
        /// <returns>
        /// True if a FluentFunction object with the specified name was found and assigned to the function output parameter.
        /// False if a FluentFunction object with the specified name was not found.
        /// </returns>
        bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function);

        /// <summary>
        /// This method retrieves an enumerable collection of all message identifiers. </summary>
        /// <returns>
        /// An enumerable collection of message identifiers. </returns>
        IEnumerable<string> GetMessageEnumerable();

        /// <summary>
        /// Retrieves an enumerable collection of string function names.
        /// </summary>
        /// <returns>An enumerable collection of functions names.</returns>
        IEnumerable<string> GetFuncEnumerable();

        /// <summary>
        /// Retrieves an enumerable collection of terms.
        /// </summary>
        /// <returns>An enumerable collection of terms.</returns>
        IEnumerable<string> GetTermEnumerable();
    }

    public abstract class FluentBundle : IEquatable<FluentBundle>, IReadBundle
    {
        /// <summary>
        /// <see cref="CultureInfo"/> of the bundle. Primary bundle locale
        /// </summary>
        public CultureInfo Culture { get; internal set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// List of Locales. First element is primary bundle locale, others are fallback locales.
        /// </summary>
        public List<string> Locales { get; internal set; } = new();

        /// <summary>
        /// When formatting patterns, FluentBundle inserts Unicode Directionality Isolation Marks to indicate that the direction of a placeable may differ from the surrounding message.
        /// This is important for cases such as when a right-to-left user name is presented in the left-to-right message.
        /// </summary>
        public bool UseIsolating { get; set; } = true;

        /// <summary>
        /// Specifies a method that will be applied only on values extending <see cref="IFluentType"/>. Useful for defining a special formatter for <see cref="FluentNumber"/>.
        /// </summary>
        public Func<string, string>? TransformFunc { get; set; }

        /// <summary>
        /// Specifies a method that will be applied only on values extending <see cref="IFluentType"/>. Useful for defining a special formatter for <see cref="FluentNumber"/>.
        /// </summary>
        public Func<IFluentType, string>? FormatterFunc { get; init; }

        /// <summary>
        /// Limit of placeable <see cref="AstTerm"/> within one <see cref="Pattern"/>, when fully expanded (all nested elements count towards it). Useful for preventing billion laughs attack. Defaults to 100.
        /// </summary>
        public byte MaxPlaceable { get; internal init; } = 100;

        /// <summary>
        /// Whether experimental features are enabled.
        ///
        /// When `true` experimental features are enabled. Experimental features include stuff like:
        /// <list type="bullet">
        /// <item>dynamic reference</item>
        /// <item>dynamic reference attributes</item>
        /// <item>term reference as parameters</item>
        /// </list>
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public bool EnableExtensions { get; init; }

        public bool AddResource(string input, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            return AddResource(res, out errors);
        }

        public bool AddResource(TextReader reader, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var res = new LinguiniParser(reader, EnableExtensions).Parse();
            return AddResource(res, out errors);
        }

        public bool AddResource(Resource res, [NotNullWhen(false)] out List<FluentError>? errors)
        {
            var innerErrors = new List<FluentError>();
            foreach (var parseError in res.Errors)
            {
                innerErrors.Add(ParserFluentError.ParseError(parseError));
            }

            for (var entryPos = 0; entryPos < res.Entries.Count; entryPos++)
            {
                var entry = res.Entries[entryPos];
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

        private void InternalResourceOverriding(Resource resource)
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
        /// Adds the given AstMessage to the collection of messages, by overriding any existing messages with the same name.
        /// </summary>
        /// <param name="message">The AstMessage to be added.</param>
        protected abstract void AddMessageOverriding(AstMessage message);

        /// <summary>
        /// Adds a term to the AstTerm list, overriding any existing term with the same name.
        /// </summary>
        /// <param name="term">The term to be added.</param>
        protected abstract void AddTermOverriding(AstTerm term);

        /// <summary>
        /// Adds a resource.
        /// Any messages or terms in bundle will be overriden by the existing ones.
        /// </summary>
        /// <param name="input">The input string containing the resource data.</param>
        public void AddResourceOverriding(string input)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            InternalResourceOverriding(res);
        }

        public void AddResourceOverriding(TextReader input)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            InternalResourceOverriding(res);
        }

        protected abstract bool TryAddTerm(AstTerm term, [NotNullWhen(false)] List<FluentError>? errors);

        protected abstract bool TryAddMessage(AstMessage msg, [NotNullWhen(false)] List<FluentError>? errors);


        public abstract bool TryAddFunction(string funcName, ExternalFunction fluentFunction);

        public abstract void AddFunctionOverriding(string funcName, ExternalFunction fluentFunction);

        public abstract void AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction);

        /// <summary>
        /// Adds a collection of external functions to the dictionary of available functions.
        /// If any function cannot be added, the errors are returned in the list.
        /// </summary>
        /// <param name="functions">A dictionary of function names and their corresponding ExternalFunction objects.</param>
        /// <param name="errors">A list of errors indicating functions that could not be added.</param>
        public void AddFunctions(IDictionary<string, ExternalFunction> functions, out List<FluentError>? errors)
        {
            errors = new List<FluentError>();
            foreach (var keyValue in functions)
            {
                if (!TryAddFunction(keyValue.Key, keyValue.Value))
                {
                    errors.Add(new OverrideFluentError(keyValue.Key, EntryKind.Func));
                }
            }
        }

        /// <summary>
        /// Determines if the provided identifier has a message associated with it. </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns>True if the identifier has a message; otherwise, false.</returns>
        /// 
        public abstract bool HasMessage(string identifier);

        /// <summary>
        /// Determines whether the given identifier with attribute has a message.
        /// </summary>
        /// <param name="idWithAttr">The identifier with attribute.</param>
        /// <returns>True if the identifier with attribute has a message; otherwise, false.</returns>
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

        /// <summary>
        /// Retrieves the attribute message by processing the given message template with the provided arguments.
        /// </summary>
        /// <param name="msgWithAttr">The string consisting of `messageId.Attribute`.</param>
        /// <param name="args">The dictionary of arguments to be used for resolution in the message template. Can be null.</param>
        /// <returns>The processed message.</returns>
        /// <exception cref="LinguiniException">Thrown when there are errors encountered during attribute substitution.</exception>
        public string? GetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args = null)
        {
            TryGetAttrMessage(msgWithAttr, args, out var errors, out var message);
            if (errors is { Count: > 0 })
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
            if (errors is { Count: > 0 })
            {
                throw new LinguiniException(errors);
            }

            return message;
        }

        public bool TryGetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            if (msgWithAttr.Contains("."))
            {
                var split = msgWithAttr.Split('.');
                return TryGetMessage(split[0], split[1], args, out errors, out message);
            }

            return TryGetMessage(msgWithAttr, null, args, out errors, out message);
        }

        public bool TryGetMessage(string id, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
            => TryGetMessage(id, null, args, out errors, out message);

        public bool TryGetMessage(string id, string? attribute, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            string? value = null;
            errors = new List<FluentError>();

            if (TryGetAstMessage(id, out var astMessage))
            {
                var pattern = attribute != null
                    ? TypeHelpers.GetAttribute(astMessage, attribute)?.Value
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

        /// <summary>
        /// Tries to get the AstMessage associated with the specified ident.
        /// </summary>
        /// <param name="ident">The identifier to look for.</param>
        /// <param name="message">When this method returns, contains the AstMessage associated with the specified ident, if found; otherwise, null.</param>
        /// <returns>True if an AstMessage was found for the specified ident; otherwise, false.</returns>
        public abstract bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message);

        /// <summary>
        /// Tries to get a term by its identifier.
        /// </summary>
        /// <param name="ident">The identifier of the AST term.</param>
        /// <param name="term">When this method returns, contains the AST term associated with the specified identifier, if the identifier is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the identifier is found and the corresponding AST term is retrieved; otherwise, false.</returns>
        public abstract bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term);

        /// <summary>
        /// Tries to get the FluentFunction associated with the specified Identifier.
        /// </summary>
        /// <param name="id">The Identifier used to identify the FluentFunction.</param>
        /// <param name="function">When the method returns, contains the FluentFunction associated with the Identifier, if the Identifier is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the FluentFunction associated with the Identifier is found; otherwise, false.</returns>
        public bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function)
        {
            return TryGetFunction(id.ToString(), out function);
        }

        /// <summary>
        /// Tries to retrieve a FluentFunction object by the given function name.
        /// </summary>
        /// <param name="funcName">The name of the function to retrieve.</param>
        /// <param name="function">An output parameter that will hold the retrieved FluentFunction object, if found.</param>
        /// <returns>
        /// True if a FluentFunction object with the specified name was found and assigned to the function output parameter.
        /// False if a FluentFunction object with the specified name was not found.
        /// </returns>
        public abstract bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function);

        public string FormatPattern(Pattern pattern, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors)
        {
            var scope = new Scope(this, args);
            var value = pattern.Resolve(scope);
            errors = scope.Errors;
            return value.AsString();
        }

        /// <summary>
        /// This method retrieves an enumerable collection of all message identifiers. </summary>
        /// <returns>
        /// An enumerable collection of message identifiers. </returns>
        public abstract IEnumerable<string> GetMessageEnumerable();

        /// <summary>
        /// Retrieves an enumerable collection of string function names.
        /// </summary>
        /// <returns>An enumerable collection of functions names.</returns>
        public abstract IEnumerable<string> GetFuncEnumerable();

        /// <summary>
        /// Retrieves an enumerable collection of terms.
        /// </summary>
        /// <returns>An enumerable collection of terms.</returns>
        public abstract IEnumerable<string> GetTermEnumerable();

        /// <summary>
        /// Creates a deep clone of the current instance of the AbstractFluentBundle class.
        /// </summary>
        /// <returns>A new instance of the AbstractFluentBundle class that is a deep clone of the current instance.</returns>
        public abstract FluentBundle DeepClone();

        /// <summary>
        /// Creates a FluentBundle object with the specified options.
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
                    FuncList = new ConcurrentDictionary<string, FluentFunction>(func),
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
                    FuncList = func,
                }
            };
        }

        /// <inheritdoc/>
        public bool Equals(FluentBundle? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Culture.Equals(other.Culture) && Locales.SequenceEqual(other.Locales) &&
                   UseIsolating == other.UseIsolating && Equals(TransformFunc, other.TransformFunc) &&
                   Equals(FormatterFunc, other.FormatterFunc) && MaxPlaceable == other.MaxPlaceable &&
                   EnableExtensions == other.EnableExtensions;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FluentBundle)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Culture, Locales, UseIsolating, TransformFunc, FormatterFunc, MaxPlaceable,
                EnableExtensions);
        }
    }
}