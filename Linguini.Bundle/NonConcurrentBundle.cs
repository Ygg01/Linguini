﻿using System;
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
    public sealed class NonConcurrentBundle : FluentBundle, IEquatable<NonConcurrentBundle>
    {
        internal Dictionary<string, FluentFunction> Functions = new();
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
            return Functions.TryAdd(funcName, fluentFunction);
        }

        /// <inheritdoc />
        public override void AddFunctionOverriding(string funcName, ExternalFunction fluentFunction)
        {
            Functions[funcName] = fluentFunction;
        }

        /// <inheritdoc />
        public override void AddFunctionUnchecked(string funcName, ExternalFunction fluentFunction)
        {
            Functions.Add(funcName, fluentFunction);
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
            return Functions.TryGetValue(funcName, out function);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetMessageEnumerable()
        {
            return _messages.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFuncEnumerable()
        {
            return Functions.Keys.ToArray();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTermEnumerable()
        {
            return _terms.Keys.ToArray();
        }

        internal override IDictionary<string, AstMessage> GetMessagesDictionary()
        {
            return _messages;
        }

        internal override IDictionary<string, AstTerm> GetTermsDictionary()
        {
            return _terms;
        }

        internal override IDictionary<string, FluentFunction> GetFunctionDictionary()
        {
            return Functions;
        }


        /// <inheritdoc />
        public override FluentBundle DeepClone()
        {
            return new NonConcurrentBundle()
            {
                Functions = new Dictionary<string, FluentFunction>(Functions),
                _terms = new Dictionary<string, AstTerm>(_terms),
                _messages = new Dictionary<string, AstMessage>(_messages),
                Culture = (CultureInfo)Culture.Clone(),
                Locales = new List<string>(Locales),
                UseIsolating = UseIsolating,
                TransformFunc = (Func<string, string>?)TransformFunc?.Clone(),
                FormatterFunc = (Func<IFluentType, string>?)FormatterFunc?.Clone(),
                MaxPlaceable = MaxPlaceable,
                EnableExtensions = EnableExtensions,
            };
        }
        
        public static NonConcurrentBundle Thaw(FrozenBundle frozenBundle)
        {
            return new NonConcurrentBundle
            {
                _messages = new Dictionary<string, AstMessage>(frozenBundle.Messages),
                Functions = new Dictionary<string, FluentFunction>(frozenBundle.Functions),
                _terms = new Dictionary<string, AstTerm>(frozenBundle.Terms),
                FormatterFunc = frozenBundle.FormatterFunc,
                Locales = frozenBundle.Locales,
                UseIsolating = frozenBundle.UseIsolating,
                MaxPlaceable = frozenBundle.MaxPlaceable,
                EnableExtensions = frozenBundle.EnableExtensions,
                TransformFunc = frozenBundle.TransformFunc,
                Culture = frozenBundle.Culture
            };
        }

        public bool Equals(NonConcurrentBundle? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Functions.SequenceEqual(other.Functions) && _terms.SequenceEqual(other._terms) &&
                   _messages.SequenceEqual(other._messages);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is NonConcurrentBundle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Functions, _terms, _messages);
        }
    }
}