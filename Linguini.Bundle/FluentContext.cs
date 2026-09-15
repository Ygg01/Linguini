using System;
using System.Globalization;
using System.Text;
using System.Xml;
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

        protected static string? ProcessNumberOptions(FluentNumberOptions? numberOptions,
            ref NumberFormatInfo numberFormatInfo)
        {
            if (numberOptions == null)
                return null;

            var stringBuilder = new StringBuilder();

            if (numberOptions.CanBeFormattedSimply)
            {
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

            return stringBuilder.ToString();
        }
    }
}