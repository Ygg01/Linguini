using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ForCanBeConvertedToForeach

namespace Linguini.Syntax.Ast
{

    public class Attribute
    {
        public readonly Identifier Id;
        public readonly Pattern Value;

        public Attribute(Identifier id, Pattern value)
        {
            Id = id;
            Value = value;
        }

        public void Deconstruct(out Identifier id, out Pattern value)
        {
            id = Id;
            value = Value;
        }
        
        public static Attribute From(string id, PatternBuilder patternBuilder)
        {
            return new Attribute(Identifier.From(id), patternBuilder.Build());
        }
    }

    public class Pattern
    {
        public readonly List<IPatternElement> Elements;

        public Pattern(List<IPatternElement> elements)
        {
            Elements = elements;
        }

        public Pattern()
        {
            Elements = new List<IPatternElement>();
        }
    }

    public class PatternBuilder
    {
        private readonly List<IPatternElement> _patternElements = new();

        private PatternBuilder() { }

        public PatternBuilder AddText(string textLiteral)
        {
            _patternElements.Add(new TextLiteral(textLiteral.AsMemory()));
            return this;
        }
        
        public PatternBuilder AddPlaceable(IExpression expr)
        {
            _patternElements.Add(new Placeable(expr));
            return this;
        }

        public Pattern Build()
        {
            return new Pattern(_patternElements);
        }
    }


    public class Identifier : IEquatable<Identifier>
    {
        public readonly ReadOnlyMemory<char> Name;

        public Identifier(ReadOnlyMemory<char> name)
        {
            Name = name;
        }

        public static Identifier From(string id)
        {
            return new Identifier(id.AsMemory());
        }

        public override string ToString()
        {
            return Name.Span.ToString();
        }

        public bool Equals(Identifier? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ToString().Equals(other.ToString());
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public interface IExpression
    {
    }

    public enum CommentLevel : byte
    {
        None = 0,
        Comment = 1,
        GroupComment = 2,
        ResourceComment = 3,
    }

    public interface IEntry
    {
        string GetId();
    }

    public interface IInlineExpression : IExpression
    {
    }

    public static class Base
    {

        public static string Stringify(this Pattern? pattern)
        {
            var sb = new StringBuilder();
            if (pattern == null || pattern.Elements.Count <= 0) return sb.ToString();
            for (var i = 0; i < pattern.Elements.Count; i++)
            {
                sb.Append(pattern.Elements[i]);
            }

            return sb.ToString();
        }
    }
}
