using System;
using System.Collections.Generic;

namespace Linguini.Ast
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
            return new(Value.ToArray());
        }
    }

    public class NumberLiteral : IInlineExpression
    {
        public ReadOnlyMemory<char> Value;

        public NumberLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }
    }

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

    public class VariableReference : IInlineExpression
    {
        public Identifier Id;

        public VariableReference(Identifier id)
        {
            Id = id;
        }
    }

    public class Placeable : IInlineExpression, IPatternElementPlaceholder, IPatternElement
    {
        public IExpression Expression;

        public Placeable(IExpression expression)
        {
            Expression = expression;
        }
    }

    public struct CallArguments
    {
        public List<IInlineExpression> PositionalArgs;
        public List<NamedArgument> NamedArgs;
    }

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

    public enum VariantType : sbyte
    {
        Identifier,
        NumberLiteral,
    }

    public struct Variant
    {
        public VariantType Type;
        public ReadOnlyMemory<char> Key;
        public Pattern Value;
        public bool IsDefault;
    }
}
