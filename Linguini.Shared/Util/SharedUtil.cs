using System;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Shared.Util
{
    /// <summary>
    /// Shared utils used by PluralRules and FluentBundle, exported to avoid circular dependencies.
    /// </summary>
    public static class SharedUtil
    {
        /// <summary>
        /// Method for matching Fluent types.
        /// 
        /// Should be overriden if the type has custom value matching. E.g. for datetime
        /// one could compare by the long value of their timestamp or by their representations.
        /// </summary>
        /// <param name="self">Currently called value</param>
        /// <param name="other">The other FluentType to compare this value with</param>
        /// <param name="scope">Current scope of the fluent bundle</param>
        /// <returns><c>true</c> if the values match and <c>false</c> otherwise</returns>
        public static bool Matches(IFluentType self, IFluentType other, IScope scope)
        {
            return (self, other) switch
            {
                (FluentString fs1, FluentString fs2) => fs1.Equals(fs2),
                (FluentNumber fn1, FluentNumber fn2) => fn1.Equals(fn2),
                (FluentReference fn1, FluentReference fn2) => fn1.Equals(fn2),
                (FluentString fs1, FluentNumber fn2) => scope.MatchByPluralCategory(fs1, fn2),
                _ => false,
            };
        }
        
        /// <summary>
        /// Returns if the integer is in the range of <see cref="int"/>s provided. Range is inclusive on both sides.
        /// </summary>
        /// <param name="self">number being checked for being in range</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>true if <c>self</c> is in range of <c>start</c> and <c>end</c> (both inclusive); false otherwise</returns>
        public static bool InRange(this int self, int start, int end)
        {
            return self >= start && self <= end;
        }

        /// <summary>
        /// Returns if the <see cref="ulong"/> is in the range of <see cref="int"/>s provided. Range is inclusive on both sides.
        /// </summary>
        /// <param name="self">number being checked for being in range</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>true if <c>self</c> is in range of <c>start</c> and <c>end</c> (both inclusive); false otherwise</returns>
        public static bool InRange(this ulong self, int start, int end)
        {
            return self >= (ulong) start && self <= (ulong) end;
        }
        
        /// <summary>
        /// Returns if the <see cref="long"/> is in the range of <see cref="int"/>s provided. Range is inclusive on both sides.
        /// </summary>
        /// <param name="self">number being checked for being in range</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>true if <c>self</c> is in range of <c>start</c> and <c>end</c> (both inclusive); false otherwise</returns>
        public static bool InRange(this long self, int start, int end)
        {
            return self >= start && self <= end;
        }

        /// <summary>
        /// Returns if the <see cref="double"/> is in the range of <see cref="int"/>s provided. Range is inclusive on both sides.
        /// </summary>
        /// <param name="value">number being checked for being in range</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>true if <c>self</c> is in range of <c>start</c> and <c>end</c> (both inclusive); false otherwise</returns>
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
