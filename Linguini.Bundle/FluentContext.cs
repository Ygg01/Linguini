using System;
using System.Globalization;
using System.Text;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle
{
    /// <summary>
    /// Represents a context for Fluent localization.
    /// Provides access to locale, culture-specific options, and number/date formatting settings.
    /// </summary>
    public class FluentContext : IFluentContext
    {
        /// <inheritdoc />
        public LangLocId Locale { get; }

        /// <inheritdoc />
        public CultureInfo Culture { get; }

        /// <inheritdoc />
        public FluentNumberOptions? NumberOptions { get; }

        /// <inheritdoc />
        public FluentDateTimeOptions? DateTimeOptions { get; }

        /// <inheritdoc />
        public string? NumFormatStr { get; }

        /// <inheritdoc />
        public NumberFormatInfo NumberFormatInfo { get; }


        /// <summary>
        /// Constructs a FluentContext from a CultureInfo.
        /// <see cref="LangLocId"/> will be instatiated from the culture parameter. Other fields will be derived from the
        /// given CultureInfo.
        /// </summary>
        /// <param name="culture">Culture upon which the other fields will be set.</param>
        public FluentContext(CultureInfo culture)
        {
            Culture = culture;
            Locale = LangLocId.FromCultureInfo(Culture);
            NumberOptions = new FluentNumberOptions();
            DateTimeOptions = new FluentDateTimeOptions();
            NumFormatStr = null;
            NumberFormatInfo = Culture.NumberFormat;
        }

        /// <summary>
        /// Represents a context that encapsulates culture-specific localization settings and options
        /// for Fluent localization. This includes locale information, number and date formatting options,
        /// as well as the underlying culture settings.
        /// </summary>
        /// <param name="locale"><see cref="LangLocId"/> that determines the culture upon which the other fields will be set.</param>
        /// <param name="numberOptions">Optional <see cref="FluentNumberOptions"/> that determines the number's formatting options.</param>
        /// <param name="dateTimeOptions">Optional <see cref="FluentDateTimeOptions"/> that determines the date's formatting options.</param>
        public FluentContext(LangLocId locale, FluentNumberOptions? numberOptions = null,
            FluentDateTimeOptions? dateTimeOptions = null)
        {
            Culture = (CultureInfo)CultureInfo.GetCultureInfo(locale.ToString()).Clone();
            Locale = locale;
            NumberOptions = numberOptions;
            DateTimeOptions = dateTimeOptions;
            var info = Culture.NumberFormat;
            NumberFormatInfo = Culture.NumberFormat;
            // TODO set from Fluent options
            NumFormatStr = ProcessNumberOptions(numberOptions, ref info);
        }

        /// <summary>
        /// Processes the given number formatting options and updates the provided NumberFormatInfo accordingly.
        /// Returns a string representation of the number format pattern if applicable based on the options and style.
        /// </summary>
        /// <param name="numberOptions">The FluentNumberOptions that contain the desired formatting preferences.</param>
        /// <param name="numberFormatInfo">A reference to the NumberFormatInfo that will be updated according to the specified options.</param>
        /// <returns>
        /// A string that represents the number format pattern when applicable, or null if the given options
        /// require no specific pattern.
        /// </returns>
        protected static string? ProcessNumberOptions(FluentNumberOptions? numberOptions,
            ref NumberFormatInfo numberFormatInfo)
        {
            if (numberOptions == null)
            {
                return null;
            }

            if (!numberOptions.CanBeFormattedSimply)
            {
                return numberOptions.Style switch
                {
                    FluentNumberStyle.Decimal => FormatDecimal(numberOptions, ref numberFormatInfo),
                    FluentNumberStyle.Currency => FormatCurrency(numberOptions, ref numberFormatInfo),
                    FluentNumberStyle.Percent => FormatPercent(numberOptions, ref numberFormatInfo),
                    _ => null,
                };
            }

            if (numberOptions.MinimumFractionDigits != null)
            {
                switch (numberOptions.Style)
                {
                    case FluentNumberStyle.Decimal:
                        numberFormatInfo.NumberDecimalDigits = (int)numberOptions.MinimumFractionDigits;
                        break;
                    case FluentNumberStyle.Currency:
                        numberFormatInfo.CurrencyDecimalDigits = (int)numberOptions.MinimumFractionDigits;
                        break;
                    case FluentNumberStyle.Percent:
                        numberFormatInfo.PercentDecimalDigits = (int)numberOptions.MinimumFractionDigits;
                        break;
                }
            }

            return null;

        }

        private static string FormatDecimal(FluentNumberOptions numberOptions, ref NumberFormatInfo numberFormatInfo)
        {
            var stringBuild = new StringBuilder();
            var decimalSeparator = numberFormatInfo.NumberDecimalSeparator;

            GenerateDecimalFormatString(stringBuild, numberOptions, decimalSeparator, 0);

            return stringBuild.ToString();
        }

        private static string FormatCurrency(FluentNumberOptions numberOptions, ref NumberFormatInfo numberFormatInfo)
        {
            var currencyBuilder = new StringBuilder();
            var n = new StringBuilder();
            var dollar = numberOptions.Currency ?? numberFormatInfo.CurrencySymbol;
            
            var decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            GenerateDecimalFormatString(n, numberOptions, decimalSeparator, 2);
            
            switch (numberFormatInfo.CurrencyPositivePattern)
            {
                case 0:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(n);
                    break;
                case 1:
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(dollar);
                    break;
                case 2:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(n);
                    break;
                default:
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(dollar);
                    break;
            }

            currencyBuilder.Append(';');

            
            var minus = numberFormatInfo.NegativeSign;
            switch (numberFormatInfo.CurrencyNegativePattern)
            {
                case 0:
                    currencyBuilder.Append('(');
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(')');
                    break;
                case 1:
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(n);
                    break;
                case 2:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(n);
                    break;
                case 3:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(minus);
                    break;
                case 4:
                    currencyBuilder.Append('(');
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(')');
                    break;
                case 5:
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(dollar);
                    break;
                case 6:
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(dollar);
                    break;
                case 7:
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(minus);
                    break;
                case 8:
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(dollar);
                    break;
                case 9:
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(n);
                    break;
                case 10:
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(minus);
                    break;
                case 11:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(minus);
                    break;
                case 12:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(n);
                    break;
                case 13:
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(dollar);
                    break;
                case 14:
                    currencyBuilder.Append('(');
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(')');
                    break;
                case 15:
                    currencyBuilder.Append('(');
                    currencyBuilder.Append(n);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(')');
                    break;
                default:
                    currencyBuilder.Append(dollar);
                    currencyBuilder.Append(minus);
                    currencyBuilder.Append(' ');
                    currencyBuilder.Append(n);
                    break;
            }

            return currencyBuilder.ToString();
        }

        private static string FormatPercent(FluentNumberOptions numberOptions, ref NumberFormatInfo numberFormatInfo)
        {
            var stringBuild = new StringBuilder();

            return stringBuild.ToString();
        }

        private static void GenerateDecimalFormatString(StringBuilder stringBuild,
            FluentNumberOptions numberOptions,
            string decimalSeparator,
            byte defaultMinimalFrac)
        {
            var mandatoryFrac = numberOptions.MinimumFractionDigits ?? defaultMinimalFrac;
            var maxFracDigit = Math.Max(mandatoryFrac,
                numberOptions.MaximumFractionDigits ?? 0);
            var optionalFrac = SaturatingSubtract(maxFracDigit, mandatoryFrac);

            stringBuild.Append('0', numberOptions.MinimumIntegerDigits ?? 0);
            stringBuild.Append(decimalSeparator);
            stringBuild.Append('0', mandatoryFrac);
            stringBuild.Append('#', optionalFrac);
        }


        private static int SaturatingSubtract(byte a, byte b)
        {
            return a < b
                ? 0
                : a - b;
        }
    }
}