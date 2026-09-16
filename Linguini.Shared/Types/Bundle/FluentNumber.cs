using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a number in double precision
    /// </summary>
    public record FluentNumber : IFluentType
    {
        /// <inheritdoc/>
        public virtual bool Equals(FluentNumber? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Value.Equals(other.Value);
        }

        /// <summary>
        /// Numerical value of fluent number, depicted using IEEE 754 64-bit floating number.
        /// </summary>
        public readonly double Value;

        private readonly PluralOperands? _operands;


        private FluentNumber(double value)
        {
            var parsedDbl = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
            if (parsedDbl.TryPluralOperands(out var ops))
            {
                _operands = ops;
            }

            Value = value;
        }

        private FluentNumber(ReadOnlySpan<char> input)
        {
            if (!input.ToString().TryPluralOperands(out var ops))
            {
                throw new ArgumentException("Invalid input for plural operands");
            }

            _operands = ops;
            Value = ops.N;
        }


        /// <inheritdoc/>
        public string AsString()
        {
            return AsString(InvariantContext.Default);
        }

        /// <inheritdoc/>
        public string AsString(IFluentContext context)
        {
            return context.NumFormatStr == null
                ? Value.ToString(context.NumberOptions.Style.ToFormat(), context.NumberFormatInfo)
                : Value.ToString(context.NumFormatStr, context.NumberFormatInfo);
        }

        /// <inheritdoc/>
        public bool IsError()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
        }


        /// <summary>
        /// Method that converts characters span into a <see cref="FluentNumber"/>
        /// </summary>
        /// <param name="input">Span of <see cref="char"/> that represents a number.</param>
        /// <exception cref="FormatException"><c>input</c> doesn't represent a number in a valid format.</exception>
        /// <returns>extracted <see cref="FluentNumber"/></returns>
        public static FluentNumber FromString(ReadOnlySpan<char> input)
        {
            return new FluentNumber(input);
        }

        /// <summary>
        /// Converts the string into a <see cref="FluentNumber"/>
        /// </summary>
        /// <param name="input">string being converted to number</param>
        /// <returns>valid <see cref="FluentNumber"/> or throws an exception</returns>
        public static FluentNumber FromString(string input)
        {
            return FromString(input.AsSpan());
        }

        /// <summary>
        /// Attempts to parse a number from the provided character span.
        /// If parsing fails, returns a new instance of <see cref="FluentString"/> containing the original input.
        /// </summary>
        /// <param name="valueSpan">The read-only span of characters representing the number to parse.</param>
        /// <returns>An instance of <see cref="FluentNumber"/> if parsing is successful; otherwise, a <see cref="FluentString"/> containing the unparsed input.</returns>
        public static IFluentType TryNumber(ReadOnlySpan<char> valueSpan)
        {
            try
            {
                return FromString(valueSpan);
            }
            catch (Exception)
            {
                return new FluentString(valueSpan);
            }
        }

        /// <summary>
        /// Overloads an operator to convert a <see cref="FluentNumber"/> to <see cref="double"/>.
        /// </summary>
        public static implicit operator double(FluentNumber fs)
        {
            return fs.Value;
        }

        /// <summary>
        /// Overloads an operator to convert a <see cref="double"/> to <see cref="FluentNumber"/>.
        /// </summary>
        public static implicit operator FluentNumber(double db)
        {
            return new FluentNumber(db);
        }

        /// <summary>
        /// Overloads an operator to convert a <see cref="float"/> to <see cref="FluentNumber"/>.
        /// </summary>
        public static implicit operator FluentNumber(float fl)
        {
            return new FluentNumber((double)fl);
        }

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return new FluentNumber(Value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// For given <see cref="FluentNumber"/> input, will try to find its <see cref="PluralOperands"/>
        /// necessary for determining plural forms for a given language.
        /// </summary>
        /// <param name="operands"><c>out</c> parameter that is present when true, it describes number as a <see cref="PluralOperands"/></param>
        /// <returns>true</returns>
        public bool TryPluralOperands([NotNullWhen(true)] out PluralOperands? operands)
        {
            operands = _operands;
            return operands != null;
        }
    }

    /// <summary>
    /// Number options as defined in <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/NumberFormat/NumberFormat">MDN Intl.NumberFormat()</see>
    /// </summary>
    public record FluentNumberOptions
    {
        #region StyleOption

        /// <summary>
        /// Number style used. <seealso cref="FluentNumberStyle"/>
        /// </summary>
        public FluentNumberStyle Style;

        /// <summary>
        /// Currency string
        /// </summary>
        public string? Currency;

        #endregion

        #region DigitOption

        /// <summary>
        /// The minimum number of integer digits to use. A value with a smaller number of integer digits than this
        /// number will be left-padded with zeros (to the specified length) when formatted.
        /// Defaults to Culture's <see cref="NumberFormatInfo"/>.
        /// </summary>
        public byte? MinimumIntegerDigits;

        /// <summary>
        /// The minimum number of fraction digits to use. Possible values are from <c>0</c> to <c>100</c>;
        /// Defaults to Culture's <see cref="NumberFormatInfo"/>.
        /// </summary>
        public byte? MinimumFractionDigits;

        /// <summary>
        /// The maximum number of fraction digits to use. Possible values are from <c>0</c> to <c>100</c>.
        /// Defaults to Culture's <see cref="NumberFormatInfo"/>.
        /// </summary>
        public byte? MaximumFractionDigits;

        /// <summary>
        /// The minimum number of significant digits to use. Possible values are from <c>1</c> to <c>21</c>.
        /// Defaults to Culture's <see cref="NumberFormatInfo"/>.
        /// </summary>
        public byte? MinimumSignificantDigits;

        /// <summary>
        /// The maximum  number of significant digits to use. Possible values are from <c>1</c> to <c>21</c>;
        /// Defaults to Culture's <see cref="NumberFormatInfo"/>.
        /// </summary>
        public byte? MaximumSignificantDigits;

        #endregion

        #region OtherOption

        /// <summary>
        /// Whether to use digits grouping in number display.
        /// </summary>
        public UseGrouping UseGrouping;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public FluentNumberOptions()
        {
            Style = FluentNumberStyle.Decimal;
            Currency = null;
            UseGrouping = UseGrouping.Auto;
            MinimumIntegerDigits = null;
            MinimumFractionDigits = null;
            MaximumFractionDigits = null;
            MinimumSignificantDigits = null;
            MaximumSignificantDigits = null;
        }

        /// <summary>
        /// If the number can be formatted by just default formatter.
        /// </summary>
        public bool CanUseDefaultFormatter => !MinimumIntegerDigits.HasValue
                                              && !MaximumFractionDigits.HasValue
                                              && !MinimumSignificantDigits.HasValue
                                              && !MaximumSignificantDigits.HasValue
                                              && Currency == null;

        /// <summary>
        /// Converts a dictionary of options into a <see cref="FluentNumberOptions"/> object.
        /// </summary>
        /// <param name="options">
        /// A dictionary containing key-value pairs where the keys represent number formatting options,
        /// and the values are instances of <see cref="IFluentType"/>.
        /// </param>
        /// <returns>
        /// A <see cref="FluentNumberOptions"/> object populated with the provided options.
        /// </returns>
        public static FluentNumberOptions ToNumberOption(IDictionary<string, IFluentType> options)
        {
            var numberOption = new FluentNumberOptions();
            if (options.TryGetValue("style", out var ft)
                && ft is FluentString styleStr && styleStr.TryIntoNumberOption(out var style))
            {
                numberOption.Style = style;
            }

            if (options.TryGetValue("currency", out var ft2)
                && ft2 is FluentString currencyStr)
            {
                numberOption.Currency = currencyStr;
            }

            if (options.TryGetValue("useGrouping", out var ft4) &&
                ft4 is FluentString useGroupingStr && useGroupingStr.TryIntoUseGrouping(out var useGrouping))
            {
                numberOption.UseGrouping = useGrouping;
            }

            if (options.TryGetValue("minimumIntegerDigits", out var minIntDig) &&
                minIntDig is FluentNumber minIntDigNum)
            {
                numberOption.MinimumIntegerDigits = (byte)minIntDigNum;
            }

            if (options.TryGetValue("minimumFractionDigits", out var minFracDig) &&
                minFracDig is FluentNumber minFracDigNum)
            {
                numberOption.MinimumFractionDigits = (byte)minFracDigNum;
            }

            if (options.TryGetValue("maximumFractionDigits", out var maxFracDig) &&
                maxFracDig is FluentNumber maxFracDigNum)
            {
                numberOption.MaximumFractionDigits = (byte)maxFracDigNum;
            }

            if (options.TryGetValue("minimumSignificantDigits", out var minSigDig) &&
                minSigDig is FluentNumber minSigDigNum)
            {
                numberOption.MinimumSignificantDigits = (byte)minSigDigNum;
            }

            if (options.TryGetValue("maximumSignificantDigits", out var maxSigDig) &&
                maxSigDig is FluentNumber maxSigDigNum)
            {
                numberOption.MaximumSignificantDigits = (byte)maxSigDigNum;
            }

            return numberOption;
        }
    }


    /// <summary>
    /// Represents which formatting style of a fluent number, specifying how the number is formatted.
    /// </summary>
    public enum FluentNumberStyle : byte
    {
        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a number e.g. <c>1 000</c>.
        /// </summary>
        Decimal = 1,

        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a currency, with provided currency e.g. <c>$100</c>.
        /// </summary>
        Currency = 2,

        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a number, with percent symbol e.g. <c>19%</c>
        /// </summary>
        Percent = 3,
    }


    /// <summary>
    /// How decimals should be rounded. 
    /// </summary>
    public enum UseGrouping : byte
    {
        /// <summary>
        /// Display grouping separators based on the locale preference, which may also be dependent on the currency.
        /// </summary>
        Auto,

        /// <summary>
        /// Display grouping separators even if the locale prefers otherwise
        /// </summary>
        Always,

        /// <summary>
        /// Display grouping separators when there are at least 2 digits in a group.
        /// </summary>
        True,

        /// <summary>
        /// Display no grouping separators.
        /// </summary>
        False
    }

    /// <summary>
    /// Utility for converting a <see cref="FluentString"/> to a corresponding enum.
    /// </summary>
    public static class FluentNumberExtensions
    {
        /// <summary>
        /// Attempts to convert a <see cref="FluentString"/> to a corresponding <see cref="FluentNumberStyle"/>.
        /// </summary>
        /// <param name="options">The <see cref="FluentString"/> representing the desired currency display style.</param>
        /// <param name="style">When this method returns, contains the resulting <see cref="FluentNumberStyle"/> if
        /// the conversion succeeded; otherwise, contains the default value of currency.</param>
        /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryIntoNumberOption(this FluentString options, out FluentNumberStyle style)
        {
            bool conversionSuccess;
            switch ((string)options)
            {
                case "decimal":
                    style = FluentNumberStyle.Decimal;
                    conversionSuccess = true;
                    break;
                case "currency":
                    style = FluentNumberStyle.Currency;
                    conversionSuccess = true;
                    break;
                case "percent":
                    style = FluentNumberStyle.Percent;
                    conversionSuccess = true;
                    break;
                default:
                    style = default;
                    conversionSuccess = false;
                    break;
            }

            return conversionSuccess;
        }

        /// <summary>
        /// Attempts to convert a <see cref="FluentString"/> to a corresponding <see cref="UseGrouping"/>.
        /// </summary>
        /// <param name="options">The <see cref="FluentString"/> representing the desired currency display style.</param>
        /// <param name="style">When this method returns, contains the resulting <see cref="UseGrouping"/> if the
        /// conversion succeeded; otherwise, contains the default value of <see cref="UseGrouping"/>.</param>
        /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryIntoUseGrouping(this FluentString options, out UseGrouping style)
        {
            bool conversionSuccess;
            switch ((string)options)
            {
                case "always":
                case "true":
                    style = UseGrouping.Always;
                    conversionSuccess = true;
                    break;
                case "false":
                    style = UseGrouping.False;
                    conversionSuccess = true;
                    break;
                case "auto":
                    style = UseGrouping.Auto;
                    conversionSuccess = true;
                    break;
                default:
                    style = UseGrouping.Auto;
                    conversionSuccess = false;
                    break;
            }

            return conversionSuccess;
        }

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
        /// Converts a <see cref="FluentNumberStyle"/> enumeration to its corresponding format string representation.
        /// </summary>
        /// <param name="style">The <see cref="FluentNumberStyle"/> value to be converted.</param>
        /// <returns>A format string representing the specified <see cref="FluentNumberStyle"/>.</returns>
        public static string ToFormat(this FluentNumberStyle style)
        {
            return style switch
            {
                FluentNumberStyle.Currency => "C",
                FluentNumberStyle.Percent => "P",
                _ => "F"
            };
        }
    }
}