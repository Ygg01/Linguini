using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

// ReSharper disable UnusedType.Global

namespace Linguini.Bundle
{
    public sealed class NonConcurrentBundle : FluentBundle
    {
        internal Dictionary<string, FluentFunction> FuncList = new();
        private Dictionary<string, AstTerm> _terms = new();
        private Dictionary<string, AstMessage> _messages = new();

        /// <inheritdoc />
        protected override void AddMessageOverriding(AstMessage message)
        {
            _messages[message.GetId()] = message;
        }

        /// <inheritdoc />
        protected override void AddTermOverriding(AstTerm term)
        {
            _terms[term.GetId()] = term;
        }

        /// <inheritdoc />
        protected override bool TryAddTerm(AstTerm term, List<FluentError>? errors)
        {
            if (_terms.TryAdd(term.GetId(), term)) return true;
            errors ??= new List<FluentError>();
            errors.Add(new OverrideFluentError(term.GetId(), EntryKind.Term));
            return false;
        }

        /// <inheritdoc />
        protected override bool TryAddMessage(AstMessage message, List<FluentError>? errors)
        {
            if (_messages.TryAdd(message.GetId(), message)) return true;
            errors ??= new List<FluentError>();
            errors.Add(new OverrideFluentError(message.GetId(), EntryKind.Message));
            return false;
        }

        
        /// <inheritdoc />
        public override bool TryAddFunction(string funcName, ExternalFunction fluentFunction)
        {
            return FuncList.TryAdd(funcName, fluentFunction);
        }

        /// <inheritdoc />
        public override void AddFunctionOverriding(string funcName, ExternalFunction fluentFunction)
        {
            FuncList[funcName] = fluentFunction;
        }

        /// <inheritdoc />
        public override void AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction)
        {
            FuncList.Add(funcName, fluentFunction);
        }

        /// <inheritdoc />
        public override bool HasMessage(string identifier)
        {
            return _messages.ContainsKey(identifier);
        }
        
        /// <inheritdoc />
        public override bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message)
        {
            return _messages.TryGetValue(ident, out message);
        }

        /// <inheritdoc />
        public override bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            return _terms.TryGetValue(ident, out term);
        }


        /// <inheritdoc />
        public override bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function)
        {
            return FuncList.TryGetValue(funcName, out function);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetMessageEnumerable()
        {
            return _messages.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFuncEnumerable()
        {
            return FuncList.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTermEnumerable()
        {
            return _terms.Keys.ToArray();
        }


        /// <inheritdoc />
        public override FluentBundle DeepClone()
        {
            return new NonConcurrentBundle()
            {
                FuncList = new Dictionary<string, FluentFunction>(FuncList),
                _terms = new Dictionary<string, AstTerm>(_terms),
                _messages = new Dictionary<string, AstMessage>(_messages),
                Culture = (CultureInfo) Culture.Clone(),
                Locales = new List<string>(Locales),
                UseIsolating = UseIsolating,
                TransformFunc = (Func<string, string>?)TransformFunc?.Clone(),
                FormatterFunc = (Func<IFluentType, string>?)FormatterFunc?.Clone(),
                MaxPlaceable = MaxPlaceable,
                EnableExtensions = EnableExtensions,
            };
        }
    }
}