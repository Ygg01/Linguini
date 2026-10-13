using System;
using System.Collections.Generic;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Shared.Util
{
    /// <summary>
    ///     Shared utils used by PluralRules and FluentBundle, exported to avoid circular dependencies.
    /// </summary>
    public static class SharedUtil
    {
     
        /// <summary>
        /// Method for comparing two dictionaries.
        /// </summary>
        /// <param name="dict1">First dictionary to compare</param>
        /// <param name="dict2">Second dictionary to compare</param>
        /// <typeparam name="TKey">Type of the keys in the dictionaries</typeparam>
        /// <typeparam name="TValue">Type of the values in the dictionaries</typeparam>
        /// <returns><c>true</c> if the dictionaries are equal, otherwise <c>false</c></returns>
        public static bool CompareDictionaries<TKey, TValue>(
            IDictionary<TKey, TValue> dict1, 
            IDictionary<TKey, TValue> dict2)
        {
            if (ReferenceEquals(dict1, dict2))
            {
                return true;
            } 
            if (ReferenceEquals(dict1, null) || ReferenceEquals(dict2, null))
            {
                return false;
            } 
            if (dict1.Count != dict2.Count)
            {
                return false; // Count check
            }

            foreach (var kvp in dict1)
            {
                if (!dict2.TryGetValue(kvp.Key, out var value2) || !EqualityComparer<TValue>.Default.Equals(kvp.Value, value2))
                {
                    return false; 
                }
            }
            return true; 
        }
        
        /// <summary>
        ///     Method for matching Fluent types.
        ///     Should be overriden if the type has custom value matching. E.g. for datetime
        ///     one could compare by the long value of their timestamp or by their representations.
        /// </summary>
        /// <param name="self">Currently called value</param>
        /// <param name="other">The other FluentType to compare this value with</param>
        /// <param name="scope">Current scope of the fluent bundle</param>
        /// <returns><c>true</c> if the values match and <c>false</c> otherwise</returns>
        public static bool Matches(IFluentType self, IFluentType other, IScope scope)
        {
            return (self, other) switch
            {
                (FluentString fs1, FluentString fs2)       => fs1.Equals(fs2),
                (FluentNumber fn1, FluentNumber fn2)       => fn1.Equals(fn2),
                (FluentReference fn1, FluentReference fn2) => fn1.Equals(fn2),
                (FluentString fs1, FluentNumber fn2)       => scope.MatchByPluralCategory(fs1, fn2),
                _                                          => false
            };
        }

        /// <summary>
        ///     Returns if the <see cref="ulong" /> is in the range of <see cref="int" />s provided. Range is inclusive on both
        ///     sides.
        /// </summary>
        /// <param name="self">number being checked for being in range</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>true if <c>self</c> is in range of <c>start</c> and <c>end</c> (both inclusive); false otherwise</returns>
        public static bool InRange(this ulong self, int start, int end)
        {
            return self >= (ulong)start && self <= (ulong)end;
        }

        /// <summary>
        ///     Returns if the <see cref="long" /> is in the range of <see cref="int" />s provided. Range is inclusive on both
        ///     sides.
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
        ///     Returns if the <see cref="double" /> is in the range of <see cref="int" />s provided. Range is inclusive on both
        ///     sides.
        /// </summary>
        /// <param name="value">number being checked for being in range</param>
        /// <param name="start">start of range</param>
        /// <param name="end">end of range</param>
        /// <returns>true if <c>self</c> is in range of <c>start</c> and <c>end</c> (both inclusive); false otherwise</returns>
        public static bool InRange(this double value, int start, int end)
        {
            for (var x = Convert.ToDouble(start); x <= end; x++)
                if (value.Equals(x))
                {
                    return true;
                }

            return false;
        }
    }
}