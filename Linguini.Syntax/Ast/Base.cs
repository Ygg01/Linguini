using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public override string ToString()
        {
            if (Elements.Count == 1 && Elements[0] is TextLiteral)
            {
                return Elements[0].ToString()!;
            }

            return "";
        }
    }

    [JsonConverter(typeof(IdentifierSerializer))]
    public class Identifier : IEquatable<Identifier>
    {
        public ReadOnlyMemory<char> Name;

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

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Identifier) obj);
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
        public static bool TryConvert<TIn, TOut>(this TIn entry, [NotNullWhen(true)] out TOut? outType)
            where TOut : TIn
        {
            var type = typeof(TOut);
            if (type.IsInstanceOfType(entry))
            {
                outType = (TOut) entry;
                return true;
            }

            outType = default!;
            return false;
        }

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
