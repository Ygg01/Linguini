using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
        public FluentNumberOptions NumberOptions { get; }

        /// <inheritdoc />
        public FluentDateTimeOptions DateTimeOptions { get; }

        /// <inheritdoc />
        public string? NumFormatStr { get; set; }

        /// <inheritdoc />
        public string? DateFormatStr { get; set; }

        /// <inheritdoc />
        public NumberFormatInfo NumberFormatInfo { get; }

        /// <inheritdoc />
        public DateTimeFormatInfo DateFormatInfo { get; }

        

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
            var numInfo = Culture.NumberFormat;
            var dateInfo = Culture.DateTimeFormat;
            NumberOptions = numberOptions ?? new FluentNumberOptions();
            DateTimeOptions = dateTimeOptions ?? new FluentDateTimeOptions();

            NumFormatStr = ProcessNumberOptions(NumberOptions, ref numInfo);
            DateFormatStr = ProcessDateTimeOptions(DateTimeOptions, ref dateInfo);
            Culture.DateTimeFormat = dateInfo;
            Culture.NumberFormat = numInfo;
            DateFormatInfo = dateInfo;
            NumberFormatInfo = numInfo;
        }

        private static string? ProcessDateTimeOptions(FluentDateTimeOptions dateTimeOptions, ref DateTimeFormatInfo info)
        {
            if (dateTimeOptions.CanUseDefaultFormatter)
            {
                var fmt = new StringBuilder();

                if (dateTimeOptions.GetStyleFormat == DateTimeZoneFormat.Date)
                {
                    FormatDate(fmt, info, dateTimeOptions.DateStyle);
                }
                else if (dateTimeOptions.GetStyleFormat == DateTimeZoneFormat.Time)
                {
                    FormatTime(fmt, info, dateTimeOptions.TimeStyle);
                }
                else if (dateTimeOptions.GetStyleFormat == DateTimeZoneFormat.DateTime &&
                         (dateTimeOptions.DateStyle == DateTimeRepresentation.Full ||
                          dateTimeOptions.TimeStyle == DateTimeRepresentation.Full))
                {
                    fmt.Append(info.FullDateTimePattern);
                }
                else
                {
                    FormatDate(fmt, info, dateTimeOptions.DateStyle);
                    fmt.Append(" ");
                    FormatTime(fmt, info, dateTimeOptions.TimeStyle);
                }
                
                return fmt.ToString();
            }
            return FullFormatDateTime(dateTimeOptions, info);
        }

        private static string? FullFormatDateTime(FluentDateTimeOptions dateTimeOptions, DateTimeFormatInfo info)
        {
            
            switch (dateTimeOptions.GetStyleFields)
            {
                case DateTimeZoneFormat.Time:
                    return FluentDateTime.ExtractTimeFmt(dateTimeOptions, info);
                case DateTimeZoneFormat.Date:
                    return FluentDateTime.ExtractDateFmt(dateTimeOptions, info);
                case DateTimeZoneFormat.DateTime:
                    var time = FluentDateTime.ExtractTimeFmt(dateTimeOptions, info);;
                    var dateFmt = FluentDateTime.ExtractDateFmt(dateTimeOptions, info);
                    return $"{dateFmt} {time}";
            }

            return null;
        }
        

        private static void FormatDate(StringBuilder sb, DateTimeFormatInfo dateTimeFormatInfo,
            DateTimeRepresentation? dateStyle)
        {
            switch (dateStyle)
            {
                case DateTimeRepresentation.Full:
                case DateTimeRepresentation.Long:
                    sb.Append(dateTimeFormatInfo.LongDatePattern);
                    break;
                case DateTimeRepresentation.Medium:
                    sb.Append(dateTimeFormatInfo.RFC1123Pattern);
                    break;
                case DateTimeRepresentation.Short:
                    sb.Append(dateTimeFormatInfo.ShortDatePattern);
                    break;
            }
        }

        private static void FormatTime(StringBuilder sb, DateTimeFormatInfo dateTimeFormatInfo,
            DateTimeRepresentation? timeStyle)
        {
            switch (timeStyle)
            {
                case DateTimeRepresentation.Full:
                case DateTimeRepresentation.Long:
                    sb.Append(dateTimeFormatInfo.LongTimePattern);
                    break;
                case DateTimeRepresentation.Medium:
                case DateTimeRepresentation.Short:
                    sb.Append(dateTimeFormatInfo.ShortTimePattern);
                    break;
            }
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

            if (!numberOptions.CanUseDefaultFormatter)
            {
                return numberOptions.Style switch
                {
                    FluentNumberStyle.Decimal => FormatDecimal(numberOptions, ref numberFormatInfo),
                    FluentNumberStyle.Currency => FormatCurrency(numberOptions, ref numberFormatInfo),
                    FluentNumberStyle.Percent => FormatPercent(numberOptions, ref numberFormatInfo),
                    _ => null,
                };
            }

            if (numberOptions.UseGrouping == UseGrouping.False)
            {
                return numberOptions.MinimumFractionDigits != null || numberOptions.MinimumFractionDigits > 0
                    ? $"G{numberOptions.MinimumFractionDigits}" 
                    : "G";
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
            var numberBuilder = new StringBuilder();
            var n = new StringBuilder();
            var decimalSeparator = numberFormatInfo.NumberDecimalSeparator;

            GenerateDecimalFormatString(n, numberOptions, decimalSeparator, 0);

            numberBuilder.Append(n);
            numberBuilder.Append(';');

            var minus = numberFormatInfo.NegativeSign;
            switch (numberFormatInfo.NumberNegativePattern)
            {
                case 0:
                    numberBuilder.Append('(');
                    numberBuilder.Append(n);
                    numberBuilder.Append(')');
                    break;
                case 1:
                    numberBuilder.Append(minus);
                    numberBuilder.Append(n);
                    break;
                case 2:
                    numberBuilder.Append(minus);
                    numberBuilder.Append(' ');
                    numberBuilder.Append(n);
                    break;
                case 3:
                    numberBuilder.Append(n);
                    numberBuilder.Append(minus);
                    break;
                default:
                    numberBuilder.Append(n);
                    numberBuilder.Append(' ');
                    numberBuilder.Append(minus);
                    break;
            }

            return numberBuilder.ToString();
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
            var percentBuilder = new StringBuilder();
            var n = new StringBuilder();
            var percent = numberFormatInfo.PercentSymbol;

            var decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            GenerateDecimalFormatString(n, numberOptions, decimalSeparator, 2);

            switch (numberFormatInfo.PercentPositivePattern)
            {
                case 0:
                    percentBuilder.Append(n);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(percent);
                    break;
                case 1:
                    percentBuilder.Append(n);
                    percentBuilder.Append(percent);
                    break;
                case 2:
                    percentBuilder.Append(percent);
                    percentBuilder.Append(n);
                    break;
                default:
                    percentBuilder.Append(percent);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(n);
                    break;
            }

            percentBuilder.Append(';');


            var minus = numberFormatInfo.NegativeSign;
            switch (numberFormatInfo.PercentNegativePattern)
            {
                case 0:
                    percentBuilder.Append(minus);
                    percentBuilder.Append(n);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(percent);
                    break;
                case 1:
                    percentBuilder.Append(minus);
                    percentBuilder.Append(n);
                    percentBuilder.Append(percent);
                    break;
                case 2:
                    percentBuilder.Append(minus);
                    percentBuilder.Append(percent);
                    percentBuilder.Append(n);
                    break;
                case 3:
                    percentBuilder.Append(percent);
                    percentBuilder.Append(minus);
                    percentBuilder.Append(n);
                    break;
                case 4:
                    percentBuilder.Append(percent);
                    percentBuilder.Append(n);
                    percentBuilder.Append(minus);
                    break;
                case 5:
                    percentBuilder.Append(n);
                    percentBuilder.Append(minus);
                    percentBuilder.Append(percent);
                    break;
                case 6:
                    percentBuilder.Append(n);
                    percentBuilder.Append(percent);
                    percentBuilder.Append(minus);
                    break;
                case 7:
                    percentBuilder.Append(minus);
                    percentBuilder.Append(percent);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(n);
                    break;
                case 8:
                    percentBuilder.Append(n);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(percent);
                    percentBuilder.Append(minus);
                    break;
                case 9:
                    percentBuilder.Append(percent);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(n);
                    percentBuilder.Append(minus);
                    break;
                case 10:
                    percentBuilder.Append(percent);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(minus);
                    percentBuilder.Append(n);
                    break;
                default:
                    percentBuilder.Append(n);
                    percentBuilder.Append(minus);
                    percentBuilder.Append(' ');
                    percentBuilder.Append(percent);
                    break;
            }

            return percentBuilder.ToString();
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