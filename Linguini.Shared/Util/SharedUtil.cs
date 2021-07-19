using System;
using System.Diagnostics.CodeAnalysis;

namespace Linguini.Shared.Util
{
    public static class SharedUtil
    {
        public static bool InRange(this int self, int start, int end)
        {
            return self >= start && self <= end;
        }

        public static bool InRange(this ulong self, int start, int end)
        {
            return self >= (ulong) start && self <= (ulong) end;
        }
        
        public static bool InRange(this long self, int start, int end)
        {
            return self >= start && self <= end;
        }

        public static bool InRange(this double value, int start, int end)
        {
            for (var x = Convert.ToDouble(start); x <= end; x++)
            {
                if (value.Equals(x))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
