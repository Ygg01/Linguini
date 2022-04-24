using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Linguini.Syntax.Serialization;

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
            return new string(Value.Span);
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
            return new string(Value.Span);
        }
    }

    [JsonConverter(typeof(FunctionReferenceSerializer))]
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

    [JsonConverter(typeof(MessageReferenceSerializer))]
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

    [JsonConverter(typeof(TermReferenceSerializer))]
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

    [JsonConverter(typeof(VariableReferenceSerializer))]
    public class VariableReference : IInlineExpression
    {
        public Identifier Id;

        public VariableReference(Identifier id)
        {
            Id = id;
        }
    }

    [JsonConverter(typeof(PlaceableSerializer))]
    public class Placeable : IInlineExpression, IPatternElementPlaceholder, IPatternElement
    {
        public IExpression Expression;

        public Placeable(IExpression expression)
        {
            Expression = expression;
        }
    }

    [JsonConverter(typeof(CallArgumentsSerializer))]
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

    [JsonConverter(typeof(NamedArgumentSerializer))]
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

    [JsonConverter(typeof(SelectExpressionSerializer))]
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

    [JsonConverter(typeof(VariantSerializer))]
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
