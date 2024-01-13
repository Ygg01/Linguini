using System;
using System.Collections.Generic;
using System.Globalization;
// ReSharper disable UnusedMember.Global

namespace Linguini.Syntax.Ast
{
    public class TextLiteral : IInlineExpression, IPatternElement
    {
        public readonly ReadOnlyMemory<char> Value;

        public TextLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        public bool Equals(IPatternElement? other)
        {
            if (other is TextLiteral textLiteralOther)
            {
                return Value.Span.SequenceEqual(textLiteralOther.Value.Span);
            }

            return false;
        }

        public TextLiteral(string id)
        {
            Value = id.AsMemory();
        }

        public override string ToString()
        {
            return Value.Span.ToString();
        }
    }

    public class NumberLiteral : IInlineExpression
    {
        public readonly ReadOnlyMemory<char> Value;

        public NumberLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }
        
        public NumberLiteral(float num)
        {
            Value = num.ToString(CultureInfo.InvariantCulture).AsMemory();
        }

        public NumberLiteral(double num)
        {
            Value = num.ToString(CultureInfo.InvariantCulture).AsMemory();
        }


        public override string ToString()
        {
            return Value.Span.ToString();
        }
    }


    public class FunctionReference : IInlineExpression
    {
        public readonly Identifier Id;
        public readonly CallArguments Arguments;

        public FunctionReference(Identifier id, CallArguments arguments)
        {
            Id = id;
            Arguments = arguments;
        }

        public FunctionReference(string id, CallArguments arguments)
        {
            Id = new Identifier(id);
            Arguments = arguments;
        }
    }

    public class MessageReference : IInlineExpression
    {
        public readonly Identifier Id;
        public readonly Identifier? Attribute;

        public MessageReference(Identifier id, Identifier? attribute)
        {
            Id = id;
            Attribute = attribute;
        }
        
        public MessageReference(string id, string? attribute = null)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }
        }
    }

    public class DynamicReference : IInlineExpression
    {
        public readonly Identifier Id;
        public readonly Identifier? Attribute;
        public readonly CallArguments? Arguments;

        public DynamicReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }
        
        public DynamicReference(string id, string? attribute = null, CallArguments? arguments = null)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }

            if (arguments != null)
            {
                Arguments = arguments.Value;
            }
        }
    }

    public class TermReference : IInlineExpression
    {
        public readonly Identifier Id;
        public readonly Identifier? Attribute;
        public readonly CallArguments? Arguments;

        public TermReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }
        
        public TermReference(string id, string? attribute = null, CallArguments? arguments = null)
        {
            Id = new Identifier(id);
            if (attribute != null)
            {
                Attribute = new Identifier(attribute);
            }

            if (arguments != null)
            {
                Arguments = arguments.Value;
            }
        }
    }
    
    public class VariableReference : IInlineExpression
    {
        public readonly Identifier Id;

        public VariableReference(Identifier id)
        {
            Id = id;
        }
    }

    public class Placeable : IInlineExpression, IPatternElementPlaceholder, IPatternElement
    {
        public readonly IExpression Expression;

        public Placeable(IExpression expression)
        {
            Expression = expression;
        }

        public bool Equals(IPatternElement? other)
        {
            if (other is Placeable otherPlaceable)
            {
                return Expression == otherPlaceable.Expression;
            }
            return false;
        }
    }
    
    public struct CallArguments
    {
        public readonly List<IInlineExpression> PositionalArgs;
        public readonly List<NamedArgument> NamedArgs;

        public CallArguments(List<IInlineExpression> positionalArgs, List<NamedArgument> namedArgs)
        {
            PositionalArgs = positionalArgs;
            NamedArgs = namedArgs;
        }
    }

    public struct NamedArgument
    {
        public readonly Identifier Name;
        public readonly IInlineExpression Value;

        public NamedArgument(Identifier name, IInlineExpression value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SelectExpression : IExpression
    {
        public readonly IInlineExpression Selector;
        public readonly List<Variant> Variants;

        public SelectExpression(IInlineExpression selector, List<Variant> variants)
        {
            Selector = selector;
            Variants = variants;
        }
    }

    public class SelectExpressionBuilder : IAddVariant
    {
        private readonly IInlineExpression _selector;
        private readonly List<Variant> _variants = new();

        public SelectExpressionBuilder(IInlineExpression selector)
        {
            _selector = selector;
        }

        public IAddVariant AddVariant(string selector, PatternBuilder patternBuilder)
        {
            _variants.Add(new Variant(selector, patternBuilder));
            return this;
        }

        public IAddVariant AddVariant(float selector, PatternBuilder patternBuilder)
        {
            _variants.Add(new Variant(selector, patternBuilder));
            return this;
        }

        public SelectExpressionBuilder SetDefault(int? defaultSelector = null)
        {
            var selector = defaultSelector is >= 0 && defaultSelector < _variants.Count
                ? _variants.Count - 1
                : defaultSelector!.Value;
            _variants[selector].InternalDefault = true;
            return this;
        }

        public SelectExpression Build()
        {
            return new SelectExpression(_selector, _variants);
        }
    }

    public interface IAddVariant
    {
        public IAddVariant AddVariant(string selector, PatternBuilder patternBuilder);
        public IAddVariant AddVariant(float selector, PatternBuilder patternBuilder);
        public SelectExpressionBuilder SetDefault(int? defaultSelector = null);
    }

    public enum VariantType : byte
    {
        Identifier,
        NumberLiteral,
    }


    public class Variant
    {
        public readonly VariantType Type;
        public readonly ReadOnlyMemory<char> Key;
        public Pattern Value => InternalValue;
        public bool IsDefault => InternalDefault;
       
        protected internal bool InternalDefault;
        protected internal Pattern InternalValue;

        public Variant(VariantType type, ReadOnlyMemory<char> key)
        {
            Type = type;
            Key = key;
            InternalValue = new Pattern();
            InternalDefault = false;
        }
        
        public Variant(string key, PatternBuilder builder)
        {
            Type = VariantType.Identifier;
            Key = key.AsMemory();
            InternalValue = builder.Build();
            InternalDefault = false;
        }
        
        public Variant(float key, PatternBuilder builder)
        {
            Type = VariantType.NumberLiteral;
            Key = key.ToString(CultureInfo.InvariantCulture).AsMemory();
            InternalValue = builder.Build();
            InternalDefault = false;
        }
    }
}