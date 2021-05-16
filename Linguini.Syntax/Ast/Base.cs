using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Linguini.Syntax.Serialization;

namespace Linguini.Syntax.Ast
{
    [JsonConverter(typeof(AttributeSerializer))]
    public class Attribute
    {
        public Identifier Id;
        public Pattern Value;

        public Attribute(Identifier id, Pattern value)
        {
            Id = id;
            Value = value;
        }
    }

    [JsonConverter(typeof(PatternSerializer))]
    public class Pattern
    {
        public List<IPatternElement> Elements;

        public Pattern(List<IPatternElement> elements)
        {
            Elements = elements;
        }

        public Pattern()
        {
            Elements = new List<IPatternElement>();
        }
    }

    [JsonConverter(typeof(IdentifierSerializer))]
    public class Identifier : IEquatable<Identifier>
    {
        public readonly ReadOnlyMemory<char> Name;

        public Identifier(ReadOnlyMemory<char> name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return new(Name.Span);
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
            if (pattern != null && pattern.Elements.Count > 0)
            {
                for (var i = 0; i < pattern.Elements.Count; i++)
                {
                    sb.Append(pattern.Elements[i]);
                }
            }

            return sb.ToString();
        }
    }
}
