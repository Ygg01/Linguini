using System;
using System.Collections.Generic;

namespace FluentSharp
{
    public interface Expression
    {
    }

    public interface InlineExpression : Expression
    {
    }

    public struct StringLiteral : InlineExpression
    {
        public ReadOnlyMemory<char> Value;

        public StringLiteral(ReadOnlyMemory<char> value)
        {
            this.Value = value;
        }
    }

    public struct NumberLiteral : InlineExpression
    {
        public ReadOnlyMemory<char> Value;
    }

    public struct FunctionReference : InlineExpression
    {
        public Identifier Id;
        public CallArguments Arguments;
    }

    public struct MessageReference : InlineExpression
    {
        public Identifier Id;
        public Identifier? Attribute;
    }

    public struct TermReference : InlineExpression
    {
        public Identifier Id;
        public Identifier? Attribute;
        public CallArguments? Arguments;
    }

    public struct VariableReference : InlineExpression
    {
        public Identifier Id;
    }

    public struct Placeable: InlineExpression
    {
        public Expression Expression;
    }

    public struct CallArguments
    {
        public List<InlineExpression> PositionalArgs;
        public List<NamedArgument> NamedArgs;
    }

    public struct NamedArgument
    {
        public Identifier Name;
        public InlineExpression Value;

        public NamedArgument(Identifier name, InlineExpression value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SelectExpression : Expression
    {
        public InlineExpression Selector;
        public List<Variant> Variants = new();

        public SelectExpression(InlineExpression selector, List<Variant> variants)
        {
            Selector = selector;
            Variants = variants;
        }
    }

    public struct Variant
    {
        public VariantType Type;
        public ReadOnlyMemory<char> Key;
        public Pattern Value;
        public bool IsDefault;
    }

    public enum VariantType : sbyte
    {
        Identifier,
        NumberLiteral,
    }
}
