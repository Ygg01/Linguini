using System;
using System.Collections.Generic;

namespace FluentSharp.Ast
{
    public struct StringLiteral : IInlineExpression
    {
        public ReadOnlyMemory<char> Value;

        public StringLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }
    }

    public struct NumberLiteral : IInlineExpression
    {
        public ReadOnlyMemory<char> Value;
    }

    public struct FunctionReference : IInlineExpression
    {
        public Identifier Id;
        public CallArguments Arguments;
    }

    public struct MessageReference : IInlineExpression
    {
        public Identifier Id;
        public Identifier? Attribute;
    }

    public struct TermReference : IInlineExpression
    {
        public Identifier Id;
        public Identifier? Attribute;
        public CallArguments? Arguments;
    }

    public struct VariableReference : IInlineExpression
    {
        public Identifier Id;
    }

    public struct Placeable : IInlineExpression
    {
        public IExpression Expression;
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
        public List<Variant> Variants = new();

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
