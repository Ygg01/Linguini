using System;
using Linguini.Shared.Algorithm;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Represents a specific date and time value that can be formatted based on localization or other formatting options.
    /// </summary>
    public class FluentDateTime
    {
    }

    /// <summary>
    /// Number options as defined in
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat">MDN Intl.DateTimeFormat()</see>
    /// </summary>
    public record FluentDateTimeOptions
    {
        #region LocaleOptions

        /// <summary>
        /// The locale matching algorithm to use.
        /// Possible values are "lookup" and "matching"; the default is "matching".
        /// </summary>
        public NegotiationStrategy LocaleMatcher;

        /// <summary>
        /// The calendar to use, such as "chinese", "gregory", "persian", and so on. 
        /// </summary>
        public CalendarOption Calendar;

        /// <summary>
        /// The numbering system to use for number formatting, such as "arab", "hans", "mathsans", and so on
        /// </summary>
        public NumberingSystemOption NumberingSystem;

        /// <summary>
        /// Whether to use 12-hour time (as opposed to 24-hour time). Possible values are true and false; the default is
        /// locale-dependent. When true, this option sets hourCycle to either "h11" or "h12"
        /// </summary>
        public bool Hour12;

        /// <summary>
        /// How hours are displayed depends on the hourCycle option.
        /// </summary>
        public HourCycle HourCycle;

        /// <summary>
        /// The time zone to use for time formatting, such as "America/New_York" or "UTC".
        /// </summary>
        public TimeZoneInfo TimeZone;

        #endregion

        #region DateTimeOptions

        /// <summary>
        /// Determines the representation style of the day of the week in a formatted date.
        /// Possible values are "Long", "Short", and "Narrow", as defined in the <see cref="DateTimeRepresentation"/> enumeration.
        /// </summary>
        public DateTimeRepresentation Weekday;

        /// <summary>
        /// Specifies how the era is displayed in a formatted date output.
        /// Possible values are "Long", "Short", and "Narrow", as defined in the <see cref="DateTimeRepresentation"/> enumeration.
        /// </summary>
        public DateTimeRepresentation Era;

        /// <summary>
        /// Specifies the representation style for the year value.
        /// Possible values include "Numeric" and "TwoDigit", determining how the year should be displayed.
        /// E.g., for the year 1999, "Numeric" would display <c>1999</c> while "TwoDigit" would display <c>99</c>.
        /// </summary>
        public NumericRepresentation Year;

        /// <summary>
        /// Specifies the representation of the month in date/time formatting.
        /// Possible values are "Numeric", "TwoDigit", "Long", "Short", and "Narrow". 
        /// </summary>
        public MonthRepresentation Month;

        /// <summary>
        /// Specifies the representation style for the day value.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 9th day of month, "Numeric" would display <c>9</c> while "TwoDigit" would display <c>09</c>.
        /// </summary>
        public NumericRepresentation Day;

        /// <summary>
        /// Specifies the formatting style for the hour in date and time representations.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 9th hour of the day, "Numeric" would display <c>9</c> while "TwoDigit" would display <c>09</c>.
        /// </summary>
        public NumericRepresentation Hour;

        /// <summary>
        /// Specifies the formatting style for the minute in date and time representations.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 3rd minute of the day, "Numeric" would display <c>3</c> while "TwoDigit" would display <c>03</c>.
        /// </summary>
        public NumericRepresentation Minute;

        /// <summary>
        /// Specifies the formatting style for the seconds in date and time representations.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 2nd hour of the day, "Numeric" would display <c>2</c> while "TwoDigit" would display <c>02</c>.
        /// </summary>
        public NumericRepresentation Second;

        /// <summary>
        /// The number of digits used to represent fractions of a second (any additional digits are truncated). Possible values are from 1 to 3.
        /// </summary>
        public FractionalSecodsDigit FractionalSecodsDigit;

        /// <summary>
        /// Represents the format in which the time zone name is displayed.
        /// Possible values include "Long", "Short", "ShortOffset", "LongOffset", "ShortGeneric", and "LongGeneric".
        /// </summary>
        public TimeZoneRepresentation TimeZoneName;

        #endregion
    }

    /// <summary>
    /// The calendar to use, such as "chinese", "gregory", "persian", and so on. 
    /// </summary>
    public enum CalendarOption
    {
    }

    /// <summary>
    /// The hour cycle to use. Possible values are "h11", "h12", "h23", and "h24";
    /// the default is inferred from hour12 and locale. 
    /// </summary>
    public enum HourCycle
    {
        /// <summary>
        /// Represents a 12-hour clock with the hour ranging from 0 to 11.
        /// Commonly used in locales or formats where the ante meridiem (AM) and post meridiem (PM) designations are separate.
        /// </summary>
        H11,

        /// <summary>
        /// Represents a 12-hour clock with the hour ranging from 1 to 12.
        /// Commonly used in locales or formats with ante meridiem (AM) and post meridiem (PM) designations.
        /// </summary>
        H12,

        /// <summary>
        /// Represents a 24-hour clock with the hour ranging from 0 to 23.
        /// Commonly used in locales or formats where the day is divided into 24 distinct hours without an ante meridiem (AM) or post meridiem (PM) designation.
        /// </summary>
        H23,

        /// <summary>
        /// Represents a 24-hour clock with the hour ranging from 1 to 24.
        /// Commonly used in locales or formats that follow a military or standard time representation without AM/PM markers.
        /// </summary>
        H24,
    }

    /// <summary>
    /// Specifies the representation style for date and time values.
    /// </summary>
    public enum DateTimeRepresentation
    {
        /// <summary>
        /// Long representation style for time unit. E.g. `Thursday` or `Anno Domini`.
        /// </summary>
        Long,

        /// <summary>
        /// Short representation style for time unit. E.g. `Thu` or `AD`.
        /// </summary>
        Short,

        /// <summary>
        /// Extremely short representation style for time unit. E.g. `T` or `A`.
        /// </summary>
        Narrow,
    }

    /// <summary>
    /// Specifies the representation style for date and time values.
    /// </summary>
    public enum NumericRepresentation
    {
        /// <summary>
        /// Full numeric represenation of temporal unit.
        /// </summary>
        Numeric,

        /// <summary>
        /// Numeric representation of temporal unit with two digits.
        /// </summary>
        TwoDigit,
    }

    /// <summary>
    /// Specifies the representation style for date and time values.
    /// </summary>
    public enum MonthRepresentation
    {
        /// <summary>
        /// Full numeric representation of month. E.g. <c>1</c> for <c>January</c>.
        /// </summary>
        Numeric,

        /// <summary>
        /// Numeric representation of month with two digits. E.g. <c>01</c> for <c>January</c>.
        /// </summary>
        TwoDigit,

        /// <summary>
        /// Textual representation of month. E.g. <c>January</c>.
        /// </summary>
        Long,

        /// <summary>
        /// Abbreviate textual representation of month. E.g.<c>Jan</c> for <c>January</c>.
        /// </summary>
        Short,

        /// <summary>
        /// Extremely short textual representation of month. E.g.<c>J</c> for <c>January</c>.
        /// </summary>
        Narrow,
    }

    /// <summary>
    /// Specifies the representation fractional seconds for date and time values.
    /// </summary>
    public enum FractionalSecodsDigit
    {
        /// <summary>
        /// One fractional second digit.
        /// </summary>
        OneDigit,

        /// <summary>
        /// Two fractional second digits.
        /// </summary>
        TwoDigits,

        /// <summary>
        /// Three fractional second digits.
        /// </summary>
        ThreeDigits,
    }

    /// <summary>
    /// The localized representation of the time zone name.
    /// </summary>
    public enum TimeZoneRepresentation
    {
        /// <summary>
        /// Long localized form (e.g., <c>Pacific Standard Time</c>, <c>Nordamerikanische Westküsten-Normalzeit</c>)
        /// </summary>
        Long,

        /// <summary>
        /// Short localized form (e.g., <c>PST</c>, <c>GMT-8</c>)
        /// </summary>
        Short,

        /// <summary>
        /// Short localized GMT format (e.g.,<c>GMT-8</c>)
        /// </summary>
        ShortOffset,

        /// <summary>
        /// Long localized GMT format (e.g., <c>GMT-08:00</c>)
        /// </summary>
        LongOffset,

        /// <summary>
        /// Short generic non-location format (e.g., <c>PT</c>, <c>Los Angeles Zeit</c>)
        /// </summary>
        ShortGeneric,

        /// <summary>
        /// Long generic non-location format (e.g., <c>Pacific Time</c>, <c>Nordamerikanische Westküstenzeit</c>)
        /// </summary>
        LongGeneric,
    }
}