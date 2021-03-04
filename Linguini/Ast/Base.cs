using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Linguini.Ast
{
    public struct Attribute
    {
        public Identifier Id;
        public Pattern Value;

        public Attribute(Identifier id, Pattern value)
        {
            Id = id;
            Value = value;
        }
    }

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

    public class Identifier
    {
        public ReadOnlyMemory<char> Name;

        public Identifier(ReadOnlyMemory<char> name)
        {
            Name = name;
        }
    }

    public interface IExpression
    {
    }

    public enum CommentLevel : sbyte
    {
        None = 0,
        Comment = 1,
        GroupComment = 2,
        ResourceComment = 3,
    }

    public interface IEntry
    {
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
            StringBuilder sb = new StringBuilder();
            if (pattern != null && pattern.Elements.Count > 0)
            {
                for (int i = 0; i < pattern.Elements.Count; i++)
                {
                    sb.Append(pattern.Elements[i]);
                }
            }
            return sb.ToString();
        }
    }
}
