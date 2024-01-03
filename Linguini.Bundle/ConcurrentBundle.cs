using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;

// ReSharper disable UnusedType.Global

namespace Linguini.Bundle
{
    public sealed class ConcurrentBundle : FluentBundle
    {
        internal ConcurrentDictionary<string, FluentFunction> _funcList = new();
        private ConcurrentDictionary<string, AstTerm> _terms = new();
        private ConcurrentDictionary<string, AstMessage> _messages = new();

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
            return _funcList.TryAdd(funcName, fluentFunction);
        }

        /// <inheritdoc />
        public override void AddFunctionOverriding(string funcName, ExternalFunction fluentFunction)
        {
            _funcList[funcName] = fluentFunction;
        }

        /// <inheritdoc />
        public override void AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction)
        {
            if (_funcList.TryAdd(funcName, fluentFunction)) return;
            throw new ArgumentException($"Function with name {funcName} already exist");
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
            return _funcList.TryGetValue(funcName, out function);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetMessageEnumerable()
        {
            return _messages.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFuncEnumerable()
        {
            return _funcList.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTermEnumerable()
        {
            return _terms.Keys.ToArray();
        }


        /// <inheritdoc />
        public override FluentBundle DeepClone()
        {
            return new ConcurrentBundle()
            {
                _funcList = new ConcurrentDictionary<string, FluentFunction>(_funcList),
                _terms = new ConcurrentDictionary<string, AstTerm>(_terms),
                _messages = new ConcurrentDictionary<string, AstMessage>(_messages),
            };
        }
    }
}