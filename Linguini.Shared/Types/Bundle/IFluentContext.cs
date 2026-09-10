using System.Globalization;


namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Represents a context interface for Fluent localization which provides primary
    /// access to the locale and culture-specific options used during localization.
    /// </summary>
    public interface IFluentContext
    {
        /// <summary>
        /// Locale represents the triplet (<c>language</c>, <c>script?</c>, <c>region?</c>) identifier used for localization.
        /// It's similar to BCP-47 language tag but doesn't implement the full specification, like the variant subtag.
        /// </summary>
        public LangLocId Locale { get; }
        /// <summary>
        /// Culture represents the culture-specific options used during localization. It's set based on the <see cref="Locale"/>.
        /// </summary>
        public CultureInfo Culture { get; }
        
        /// <summary>
        /// NumberOptions represents the Localization-specific options used during number formatting. If present,
        /// it overrides the default options set by the culture.
        /// </summary>
        public FluentNumberOptions? NumberOptions { get; }
        
        /// <summary>
        /// DateTimeOptions represents the Localization-specific options used during date-time formatting. If present,
        /// it overrides the default options set by the culture.
        /// </summary>
        public FluentDateTimeOptions? DateTimeOptions { get; }
        
        /// <summary>
        /// Formatter used to format numbers.
        /// </summary>
        public NumberFormatInfo NumberFormatInfo { get; }
        
        /// <summary>
        /// Formatter used to format date-times.
        /// </summary>
        public DateTimeFormatInfo DateTimeFormatInfo  { get; }

    }

    /// <summary>
    /// Presents a default <see cref="IFluentContext"/> implementation that uses invariant culture and sets most
    /// localization options to defaults. To use it just instantiate <see cref="Default"/>.
    /// </summary>
    public sealed class InvariantContext : IFluentContext
    {
        /// <summary>
        /// Only way to use this class
        /// </summary>
        public static readonly InvariantContext Default = new();

        /// <inheritdoc />
        public LangLocId Locale { get;  }

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
        /// Private constructor to prevent instantiation from outside.
        /// </summary>
        private InvariantContext()
        {
            Locale = LangLocParser.Parse("root");
            Culture = CultureInfo.InvariantCulture;
            NumberOptions = null;
            DateTimeOptions = null;
            NumberFormatInfo = Culture.NumberFormat;
            DateTimeFormatInfo = Culture.DateTimeFormat;
        }
    }
}