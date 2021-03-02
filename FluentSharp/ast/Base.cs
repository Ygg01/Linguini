using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentSharp.Ast
{
    public struct Attribute
    {
        public Identifier Id;
        public Pattern Value;
    }

    public struct Pattern
    {
        public List<ReadOnlyMemory<char>> Elements;
    }

    public struct Identifier
    {
        public ReadOnlyMemory<char> Name;
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
        public static bool TryConvert<T>(this IEntry entry, [NotNullWhen(true)] out T outType)
        {
            var type = typeof(T);
            if (type.IsInstanceOfType(entry))
            {
                outType = (T) entry;
                return true;
            }

            outType = default!;
            return false;
        }
    }
}
