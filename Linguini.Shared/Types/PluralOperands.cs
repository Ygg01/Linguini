﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Shared.Types
{
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

        public PluralOperands(double n, ulong i, int v, int w, long f, long t)
        {
            N = n;
            I = i;
            V = v;
            W = w;
            F = f;
            T = t;
        }

        public int Exp()
        {
            return (int)Math.Floor(Math.Log10(N));
        }
    }

    public static class PluralOperandsHelpers
    {
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

        public static bool TryPluralOperands(this sbyte input, out PluralOperands? operands)
        {
            return Convert.ToInt64(input).TryPluralOperands(out operands);
        }

        public static bool TryPluralOperands(this short input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return Convert.ToInt64(input).TryPluralOperands(out operands);
        }

        public static bool TryPluralOperands(this int input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return Convert.ToInt64(input).TryPluralOperands(out operands);
        }

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

        public static bool TryPluralOperands(this float input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return input.ToString(CultureInfo.InvariantCulture).TryPluralOperands(out operands);
        }

        public static bool TryPluralOperands(this double input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return input.ToString(CultureInfo.InvariantCulture).TryPluralOperands(out operands);
        }

        public static bool TryPluralOperands(this FluentNumber input, [NotNullWhen(true)] out PluralOperands? operands)
        {
            return input.AsString().TryPluralOperands(out operands);
        }

        #endregion
    }
}