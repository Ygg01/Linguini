using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Shared.Types
{
    /// <summary>
    /// Represents the operands used in plural rule calculations to determine plural forms.
    /// </summary>
    public class PluralOperands
    {
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

        /// Represents the operands used for pluralization rules.
        /// This class encapsulates numeric values in different formats which are
        /// used in determining plural forms in linguistic contexts.
        /// <param name="n">The complete numeric value, represented as a double.</param>
        /// <param name="i">The integral part of the numeric value, represented as an unsigned long.</param>
        /// <param name="v">The number of visible fraction digits in the numeric value, without trailing zeros.</param>
        /// <param name="w">The number of significant fraction digits in the numeric value.</param>
        /// <param name="f">The numeric value of the visible fraction digits, without trailing zeros.</param>
        /// <param name="t">Similar to F but includes significant fractional digits only.</param>
        public PluralOperands(double n, ulong i, int v, int w, long f, long t)
        {
            N = n;
            I = i;
            V = v;
            W = w;
            F = f;
            T = t;
        }

        /// <summary>
        /// The exponent of the value.
        /// </summary>
        /// <returns>Exponent of the value e.g. for <c>100</c> it returns <c>2</c></returns>
        public int Exp()
        {
            return (int)Math.Floor(Math.Log10(N));
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
        /// <param name="input">number as a string, using <see cref="NumberFormatInfo.InvariantInfo"/> parsing rules.</param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true if the number is parsable to a <see cref="PluralOperands"/>; false otherwise.</returns>
        public static bool TryPluralOperands(this string input, out PluralOperands? operands)
        {
            var absStr = input.StartsWith("-")
                ? input.AsSpan()[1..]
                : input.AsSpan();

            if (!double.TryParse(absStr.ToString(),
                    NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo,
                    out var absoluteValue))
            {
                operands = null;
                return false;
            }
            
            ulong intDigits;
            int numFractionDigits0;
            int numFractionDigits;
            long fractionDigits0;
            long fractionDigits;
            var decPos = absStr.IndexOf('.');
            if (decPos > -1)
            {
                var intStr = absStr[..decPos];
                var decStr = absStr[(decPos + 1) ..];

                if (!ulong.TryParse(intStr.ToString(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out intDigits))
                {
                    operands = null;
                    return false;
                }

                var backTrace = decStr.TrimEnd('0');

                numFractionDigits0 = decStr.Length;
                numFractionDigits = backTrace.Length;
                if (!long.TryParse(decStr.ToString(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out fractionDigits0))
                {
                    operands = null;
                    return false;
                }

                if (!long.TryParse(backTrace.ToString(), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out fractionDigits))
                {
                    fractionDigits = 0;
                }
            }
            else
            {
                intDigits = Convert.ToUInt64(absoluteValue);
                numFractionDigits0 = 0;
                numFractionDigits = 0;
                fractionDigits0 = 0;
                fractionDigits = 0;
            }

            operands = new(
                absoluteValue,
                intDigits,
                numFractionDigits0,
                numFractionDigits,
                fractionDigits0,
                fractionDigits
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
                0
            );
            return true;
        }

        #endregion

        #region FLOATS

        /// <summary>
        /// For given <see cref="float"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input"><see cref="FluentNumber"/> to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true if conversion succeeds; otherwise false</returns>
        public static bool TryPluralOperands(this float input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return input.ToString(CultureInfo.InvariantCulture).TryPluralOperands(out operands);
        }

        /// <summary>
        /// For given <see cref="float"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input"><see cref="FluentNumber"/> to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true if conversion succeeds; otherwise false</returns>
        public static bool TryPluralOperands(this double input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return input.ToString(CultureInfo.InvariantCulture).TryPluralOperands(out operands);
        }

        /// <summary>
        /// For given <see cref="FluentNumber"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="input"><see cref="FluentNumber"/> to convert to <see cref="PluralOperands"/></param>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public static bool TryPluralOperands(this FluentNumber input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return input.AsString().TryPluralOperands(out operands);
        }

        #endregion
    }
}