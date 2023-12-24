using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Resolver;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;

namespace Linguini.Bundle
{
    using FluentArgs = IDictionary<string, IFluentType>;

    public class FluentBundle
    {
        private IDictionary<string, FluentFunction> _funcList;
        private IDictionary<string, AstTerm> _terms;
        private IDictionary<string, AstMessage> _messages;

        #region Properties
        /// <summary>
        /// <see cref="CultureInfo"/> of the bundle. Primary bundle locale
        /// </summary>
        public CultureInfo Culture { get; internal set; }
        /// <summary>
        /// List of Locales. First element is primary bundle locale, others are fallback locales.
        /// </summary>
        public List<string> Locales { get; internal set; }
        /// <summary>
        /// When formatting patterns, FluentBundle inserts Unicode Directionality Isolation Marks to indicate that the direction of a placeable may differ from the surrounding message.
        /// This is important for cases such as when a right-to-left user name is presented in the left-to-right message.
        /// </summary>
        public bool UseIsolating { get; set; }
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
        public byte MaxPlaceable { get; private init; }
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
        public bool EnableExtensions { get; init; }

        #endregion

        #region Constructors

        private FluentBundle()
        {
            _terms = new Dictionary<string, AstTerm>();
            _messages = new Dictionary<string, AstMessage>();
            _funcList = new Dictionary<string, FluentFunction>();
            Culture = CultureInfo.CurrentCulture;
            Locales = new List<string>();
            UseIsolating = true;
            MaxPlaceable = 100;
            EnableExtensions = false;
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
            IDictionary<string, AstTerm> terms;
            IDictionary<string, AstMessage> messages;
            IDictionary<string, FluentFunction> functions;
            if (option.UseConcurrent)
            {
                terms = new ConcurrentDictionary<string, AstTerm>();
                messages = new ConcurrentDictionary<string, AstMessage>();
                functions = new ConcurrentDictionary<string, FluentFunction>();
            }
            else
            {
                terms = new Dictionary<string, AstTerm>();
                messages = new Dictionary<string, AstMessage>();
                functions = new Dictionary<string, FluentFunction>();
            }

            return new FluentBundle
            {
                Culture = cultureInfo,
                Locales = locales,
                _terms = terms,
                _messages = messages,
                _funcList = functions,
                TransformFunc = option.TransformFunc,
                FormatterFunc = option.FormatterFunc,
                UseIsolating = option.UseIsolating,
                MaxPlaceable = option.MaxPlaceable,
                EnableExtensions = option.EnableExtensions,
            };
        }

        #endregion

        #region AddMethods

        public bool AddResource(string input, out List<FluentError> errors)
        {
            var res = new LinguiniParser(input, EnableExtensions).Parse();
            return AddResource(res, out errors);
        }
        
        public bool AddResource(TextReader reader, out List<FluentError> errors)
        {
            var res = new LinguiniParser(reader, EnableExtensions).Parse();
            return AddResource(res, out errors);
        }


        internal bool AddResource(Resource res, out List<FluentError> errors)
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
                        AddMessage(errors, message);
                        break;
                    case AstTerm term:
                        AddTerm(errors, term);
                        break;
                }
            }

            if (errors.Count == 0)
            {
                return true;
            }

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

        private void AddMessageOverriding(AstMessage message)
        {
            _messages[message.GetId()] = message;
        }

        private void AddTermOverriding(AstTerm term)
        {
            _terms[term.GetId()] = term;
        }

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

        private void AddTerm(List<FluentError> errors, AstTerm term)
        {
            if (_terms.TryAdd(term.GetId(), term))
                return;

            errors.Add(new OverrideFluentError(term.GetId(), EntryKind.Term));
        }

        private void AddMessage(List<FluentError> errors, AstMessage msg)
        {
            if (_messages.TryAdd(msg.GetId(), msg))
                return;

            errors.Add(new OverrideFluentError(msg.GetId(), EntryKind.Message));
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
                case InsertBehavior.None:
                    if (!_funcList.ContainsKey(funcName))
                    {
                        _funcList.Add(funcName, fluentFunction);
                        return true;
                    }

                    return false;
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
            return _messages.ContainsKey(identifier);
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
            return _messages.TryGetValue(ident, out message);
        }

        public bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            return _terms.TryGetValue(ident, out term);
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
            return _messages.Keys;
        }

        public IEnumerable<string> GetFuncEnumerable()
        {
            return _funcList.Keys;
        }

        public IEnumerable<string> GetTermEnumerable()
        {
            return _terms.Keys;
        }

        public FluentBundle DeepClone()
        {
            return new()
            {
                Culture = (CultureInfo)Culture.Clone(),
                FormatterFunc = FormatterFunc,
                Locales = new List<string>(Locales),
                _messages = new Dictionary<string, AstMessage>(_messages),
                _terms = new Dictionary<string, AstTerm>(_terms),
                _funcList = new Dictionary<string, FluentFunction>(_funcList),
                TransformFunc = TransformFunc,
                UseIsolating = UseIsolating,
            };
        }
    }
}