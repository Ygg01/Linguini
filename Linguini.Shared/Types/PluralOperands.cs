using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Linguini.Shared.Types
{
    /// <summary>
    /// Represents the operands used in plural rule calculations to determine plural forms.
    ///
    /// See <a href="https://unicode.org/reports/tr35/tr35-numbers.html#Operands">CLDR Plural Operands</a> for more information.
    /// </summary>
    public class PluralOperands
    {
        /// <summary>
        /// Compares this <see cref="PluralOperands"/> instance with other <see cref="PluralOperands"/> instance.
        /// </summary>
        /// <param name="other">Another instance to compare it to</param>
        /// <returns><c>true</c> if the operands are equal; otherwise, <c>false</c>.</returns>
        protected bool Equals(PluralOperands other)
        {
            return N.Equals(other.N) && I == other.I && V == other.V && W == other.W && F == other.F && T == other.T && C == other.C;
        }

        
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PluralOperands)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(N, I, V, W, F, T, C);
        }

        /// <summary>
        /// Determines whether two <see cref="PluralOperands"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance of <see cref="PluralOperands"/> to compare.</param>
        /// <param name="right">The second instance of <see cref="PluralOperands"/> to compare.</param>
        /// <returns><c>true</c> if the specified instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(PluralOperands? left, PluralOperands? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two <see cref="PluralOperands"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance of <see cref="PluralOperands"/> to compare.</param>
        /// <param name="right">The second instance of <see cref="PluralOperands"/> to compare.</param>
        /// <returns><c>true</c> if the specified instances are inequal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(PluralOperands? left, PluralOperands? right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Absolute value of input
        /// </summary>
        public readonly double N;

        /// <summary>
        /// Integer value of input
        /// </summary>
        public readonly ulong I;

        /// <summary>
        /// Number of visible fraction digits with trailing zeros
        /// </summary>
        public readonly int V;

        /// <summary>
        /// Number of visible fraction digits without trailing zeros
        /// </summary>
        public readonly int W;

        /// <summary>
        /// Visible fraction digits with trailing zeros
        /// </summary>
        public readonly long F;

        /// <summary>
        /// Visible fraction digits without trailing zeros
        /// </summary>
        public readonly long T;
        
        /// <summary>
        /// Compact decimal exponent value: exponent of the power of 10 used in compact decimal formatting.
        /// </summary>
        public readonly long C;


        /// Represents the operands used for pluralization rules.
        /// This class encapsulates numeric values in different formats which are
        /// used in determining plural forms in linguistic contexts.
        /// <param name="n">The complete numeric value, represented as a double.</param>
        /// <param name="i">The integer digits of <c>N</c>.</param>
        /// <param name="v">The number of visible fraction digits in <c>N</c>, with trailing zeros.</param>
        /// <param name="w">The number of visible fraction digits in <c>N</c>, without trailing zeros.</param>
        /// <param name="f">The visible fraction digits in <c>N</c> with trailing zeros, expressed as an integer.</param>
        /// <param name="t">The visible fraction digits in <c>N</c> without trailing zeros, expressed as an integer.</param>
        /// <param name="c">Compact decimal exponent value.</param>
        public PluralOperands(double n, ulong i, int v, int w, long f, long t, long c)
        {
            N = n;
            I = i;
            V = v;
            W = w;
            F = f;
            T = t;
            C = c;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"PluralOperands (N: {N}, I: {I}, V: {V}, W: {W}, F: {F}, T: {T}, C: {C})";
        }
    }

    /// <summary>
    /// Provides utility methods for converting various numeric and string types into instances of the
    /// <see cref="PluralOperands"/> class for use in plural rule calculations.
    /// </summary>
    public static class PluralOperandsHelpers
    {
        /// <summary>
        /// For given <see cref="string"/> input, will convert it to number and then try to find it's <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="strInput">number as a string, using <see cref="NumberFormatInfo.InvariantInfo"/> parsing rules.</param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true if the number is parsable to a <see cref="PluralOperands"/>; false otherwise.</returns>
        public static bool TryPluralOperands(this string strInput, [NotNullWhen(true)] out PluralOperands? operands)
        {
            // replace any 1c3 string to 1e3 which is a valid double
            var input = Regex.Replace(strInput, "[cC]", "e");
            var expPosition = input.IndexOf('e');
            var minusStart = input.StartsWith("-") ? 1 : 0;
            var absStr = input.AsSpan()[minusStart..];
 
            if (!double.TryParse(absStr.ToString(),
                    NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo,
                    out var absoluteValue))
            {
                operands = null;
                return false;
            }
            
            ulong intDigits = (uint)Math.Truncate(absoluteValue);
            var numFractionDigits0 = 0;
            var numFractionDigits = 0;
            var fractionDigits0 = 0 ;
            var fractionDigits = 0;
            var exp = 0;
            var decPos = absStr.IndexOf('.');
            var fixedDecPos = decPos + 1;
            var endDecPos = absStr.Length;
            if (expPosition != -1 && expPosition < input.Length)
            {
                exp = int.Parse(input[(expPosition + 1)..]);
                fixedDecPos += exp;
                endDecPos = expPosition;
            }
            
            if (decPos > -1 && fixedDecPos < absStr.Length)
            {
                var decStr = absStr[fixedDecPos ..endDecPos];
                var backTrace = decStr.TrimEnd('0');

                numFractionDigits0 = decStr.Length;
                numFractionDigits = backTrace.Length;
                fractionDigits0 = decStr.Length > 0 ? int.Parse(decStr) : 0;
                fractionDigits = backTrace.Length > 0 ? int.Parse(backTrace) : 0;
                // if (!int.TryParse(decStr.ToString(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out fractionDigits0))
                // {
                //     operands = null;
                //     return false;
                // }
                //
                // if (!int.TryParse(backTrace.ToString(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out fractionDigits))
                // {
                //     fractionDigits = 0;
                // }
            }

            operands = new(
                absoluteValue,
                intDigits,
                numFractionDigits0,
                numFractionDigits,
                fractionDigits0,
                fractionDigits,
                exp
            );
            return true;
        }

        #region SIGNED_INTS

        /// <summary>
        /// For given <see cref="sbyte"/> input, will convert it to number and then try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number as a string, using <see cref="NumberFormatInfo.InvariantInfo"/> parsing rules.</param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true if the number is parsable to a <see cref="PluralOperands"/>; false otherwise.</returns>
        public static bool TryPluralOperands(this sbyte input, out PluralOperands? operands)
        {
            return Convert.ToInt64(input).TryPluralOperands(out operands);
        }

        /// <summary>
        /// For given <see cref="short"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this short input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return Convert.ToInt64(input).TryPluralOperands(out operands);
        }

        /// <summary>
        /// For given <see cref="int"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this int input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return Convert.ToInt64(input).TryPluralOperands(out operands);
        }

        /// <summary>
        /// For given <see cref="long"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this long input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            operands = new(
                Convert.ToDouble(Math.Abs(input)),
                Convert.ToUInt64(Math.Abs(input)),
                0,
                0,
                0,
                0,
                0
            );
            return true;
        }

        #endregion

        #region UNSIGNED_INTS

        /// <summary>
        /// For given <see cref="byte"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this byte input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            operands = new(
                Convert.ToDouble(input),
                Convert.ToUInt64(input),
                0,
                0,
                0,
                0,
                0
            );
            return true;
        }

        /// <summary>
        /// For given <see cref="ushort"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this ushort input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            operands = new(
                Convert.ToDouble(input),
                Convert.ToUInt64(input),
                0,
                0,
                0,
                0,
                0
            );
            return true;
        }

        /// <summary>
        /// For given <see cref="uint"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this uint input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            operands = new(
                Convert.ToDouble(input),
                Convert.ToUInt64(input),
                0,
                0,
                0,
                0,
                0
            );
            return true;
        }

        /// <summary>
        /// For given <see cref="ulong"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input">number to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this ulong input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            operands = new(
                Convert.ToDouble(input),
                Convert.ToUInt64(input),
                0,
                0,
                0,
                0,
                0
            );
            return true;
        }

        #endregion

    }
}