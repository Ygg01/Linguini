using System.Diagnostics.CodeAnalysis;

namespace FluentSharp
{
    public interface IExpression
    {
    }

    public enum CommentLevel : sbyte
    {
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
