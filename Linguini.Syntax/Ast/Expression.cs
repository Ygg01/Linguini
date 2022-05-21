﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER
using System.Text.Json.Serialization;
using Linguini.Syntax.Serialization;
#endif

namespace Linguini.Syntax.Ast
{
    public class TextLiteral : IInlineExpression, IPatternElement
    {
        public ReadOnlyMemory<char> Value;

        public TextLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        public override string ToString()
        {
            MemoryMarshal.TryGetString(Value, out var text, out var _, out var _);
            return text;
        }
    }

    public class NumberLiteral : IInlineExpression
    {
        public ReadOnlyMemory<char> Value;

        public NumberLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        public override string ToString()
        {
            MemoryMarshal.TryGetString(Value, out var text, out var _, out var _);
            return text;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(FunctionReferenceSerializer))]
#endif
    public class FunctionReference : IInlineExpression
    {
        public Identifier Id;
        public CallArguments Arguments;

        public FunctionReference(Identifier id, CallArguments arguments)
        {
            Id = id;
            Arguments = arguments;
        }
    }
#if NET5_0_OR_GREATER
    [JsonConverter(typeof(MessageReferenceSerializer))]
#endif
    public class MessageReference : IInlineExpression
    {
        public Identifier Id;
        public Identifier? Attribute;

        public MessageReference(Identifier id, Identifier? attribute)
        {
            Id = id;
            Attribute = attribute;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(TermReferenceSerializer))]
#endif
    public class TermReference : IInlineExpression
    {
        public Identifier Id;
        public Identifier? Attribute;
        public CallArguments? Arguments;

        public TermReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(VariableReferenceSerializer))]
#endif
    public class VariableReference : IInlineExpression
    {
        public Identifier Id;

        public VariableReference(Identifier id)
        {
            Id = id;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(PlaceableSerializer))]
#endif
    public class Placeable : IInlineExpression, IPatternElementPlaceholder, IPatternElement
    {
        public IExpression Expression;

        public Placeable(IExpression expression)
        {
            Expression = expression;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(CallArgumentsSerializer))]
#endif
    public struct CallArguments
    {
        public List<IInlineExpression> PositionalArgs;
        public List<NamedArgument> NamedArgs;

        public CallArguments(List<IInlineExpression> positionalArgs, List<NamedArgument> namedArgs)
        {
            PositionalArgs = positionalArgs;
            NamedArgs = namedArgs;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(NamedArgumentSerializer))]
#endif
    public struct NamedArgument
    {
        public Identifier Name;
        public IInlineExpression Value;

        public NamedArgument(Identifier name, IInlineExpression value)
        {
            Name = name;
            Value = value;
        }
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(SelectExpressionSerializer))]
#endif
    public class SelectExpression : IExpression
    {
        public IInlineExpression Selector;
        public List<Variant> Variants;

        public SelectExpression(IInlineExpression selector, List<Variant> variants)
        {
            Selector = selector;
            Variants = variants;
        }
    }

    public enum VariantType : byte
    {
        Identifier,
        NumberLiteral,
    }

#if NET5_0_OR_GREATER
    [JsonConverter(typeof(VariantSerializer))]
#endif
    public class Variant
    {
        public VariantType Type;
        public ReadOnlyMemory<char> Key;
        public Pattern Value;
        public bool IsDefault;

        public Variant(VariantType type, ReadOnlyMemory<char> key)
        {
            Type = type;
            Key = key;
            Value = new Pattern();
            IsDefault = false;
        }
    }
}