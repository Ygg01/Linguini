using System.Diagnostics.CodeAnalysis;

namespace Linguini.Shared
{
    public static class Util
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
    }
}
