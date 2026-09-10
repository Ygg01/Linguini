using System.Globalization;
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
        public NumberFormatInfo NumberFormatInfo { get; }

        /// <inheritdoc />
        public DateTimeFormatInfo DateTimeFormatInfo { get; }


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
            NumberFormatInfo = culture.NumberFormat;
            DateTimeFormatInfo = culture.DateTimeFormat;
        }

        /// <summary>
        /// Represents a context that encapsulates culture-specific localization settings and options
        /// for Fluent localization. This includes locale information, number and date formatting options,
        /// as well as the underlying culture settings.
        /// </summary>
        /// <param name="culture"><see cref="LangLocId"/> that determines the culture upon which the other fields will be set.</param>
        /// <param name="numberOptions">Optional <see cref="FluentNumberOptions"/> that determines the number's formatting options.</param>
        /// <param name="dateTimeOptions">Optional <see cref="FluentDateTimeOptions"/> that determines the date's formatting options.</param>
        public FluentContext(LangLocId culture, FluentNumberOptions? numberOptions = null,
            FluentDateTimeOptions? dateTimeOptions = null)
        {
            Culture = CultureInfo.GetCultureInfo(culture.ToString());
            Locale = LangLocId.FromCultureInfo(Culture);
            NumberOptions = numberOptions;
            DateTimeOptions = dateTimeOptions;
            // TODO set from Fluent options
            NumberFormatInfo = Culture.NumberFormat;
            DateTimeFormatInfo = Culture.DateTimeFormat;
        }
    }
}