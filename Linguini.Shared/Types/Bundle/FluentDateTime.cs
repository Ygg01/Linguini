using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Represents a specific date and time value that can be formatted based on localization or other formatting options.
    /// </summary>
    public class FluentDateTime : IFluentType
    {
        /// <summary>
        /// Determines whether the current instance is equal to the specified <see cref="FluentDateTime"/> object.
        /// </summary>
        /// <param name="other">The other <see cref="FluentDateTime"/> instance to compare with the current instance.</param>
        /// <returns><c>true</c> if the current instance is equal to the specified instance; otherwise, <c>false</c>.</returns>
        protected bool Equals(FluentDateTime other)
        {
            return Date.Equals(other.Date);
        }

        /// <summary>
        /// DateTimeOffset value of the fluent date-time.
        /// </summary>
        public readonly DateTimeOffset Date;

        /// <summary>
        /// Formatting options of this fluent date-time.
        /// </summary>
        public readonly FluentDateTimeOptions? Options;

        private FluentDateTime(DateTimeOffset date, FluentDateTimeOptions? options)
        {
            Date = date;
            Options = options;
        }

        /// <inheritdoc/>
        public string AsString(IFluentContext context)
        {
            // Can we reuse a formatter?
            if (context.DateTimeOptions.CanUseDefaultFormatter)
            {
                var fmt = new StringBuilder();

                if (context.DateTimeOptions.GetStyleFormat == DateTimeZoneFormat.Date)
                {
                    FormatDate(fmt, context.DateFormatInfo, context.DateTimeOptions.DateStyle);
                }
                else if (context.DateTimeOptions.GetStyleFormat == DateTimeZoneFormat.Time)
                {
                    FormatTime(fmt, context.DateFormatInfo, context.DateTimeOptions.TimeStyle);
                }
                else if (context.DateTimeOptions.GetStyleFormat == DateTimeZoneFormat.DateTime &&
                         (context.DateTimeOptions.DateStyle == DateTimeRepresentation.Full ||
                          context.DateTimeOptions.TimeStyle == DateTimeRepresentation.Full))
                {
                    fmt.Append(context.DateFormatInfo.FullDateTimePattern);
                }
                else
                {
                    FormatDate(fmt, context.DateFormatInfo, context.DateTimeOptions.DateStyle);
                    fmt.Append(" ");
                    FormatTime(fmt, context.DateFormatInfo, context.DateTimeOptions.TimeStyle);
                }
                
                return Date.ToString(fmt.ToString(), context.DateFormatInfo);
            }

            return FullFormatDateTime(context);
        }
        
        
        private string FullFormatDateTime(IFluentContext context)
        {
            if (context.DateFormatStr != null)
            {
                return Date.ToString(context.DateFormatInfo.FullDateTimePattern, context.DateFormatInfo);
            }
            
            if (context.DateTimeOptions.GetStyleFields == DateTimeZoneFormat.Time)
            {
                context.DateFormatInfo.FullDateTimePattern = ExtractTimeFmt(context);
            }

            return Date.ToString(context.DateFormatInfo.FullDateTimePattern, context.DateFormatInfo);
        }

        private static string ExtractTimeFmt(IFluentContext context)
        {
            var timeFmt = context.DateFormatInfo.LongTimePattern;
            var h = context.DateTimeOptions.Hour12  == true 
                ? "h" : "H";
            var hourStr = context.DateTimeOptions.Hour == NumericDateFormat.TwoDigit ? $"{h}{h}" : $"{h}";
            var minStr = context.DateTimeOptions.Minute == NumericDateFormat.TwoDigit ? "mm" : "m";
            var secStr = context.DateTimeOptions.Minute == NumericDateFormat.TwoDigit ? "ss" : "s";
            var fracStr = context.DateTimeOptions.FractionalSecondsDigit switch
            {
                FractionalSecodsDigit.OneDigit => ".f",
                FractionalSecodsDigit.TwoDigits => ".f",
                FractionalSecodsDigit.ThreeDigits => ".f",
                _ => ""
            };
            var newTimeFmt = Regex.Replace(timeFmt, "(h|H){1,2}", hourStr);
            newTimeFmt = Regex.Replace(newTimeFmt, "m{1,2}", minStr);
            newTimeFmt = Regex.Replace(newTimeFmt, "s{1,2}", secStr + fracStr);
            if (context.DateTimeOptions.Hour12 == true)
            {
                newTimeFmt += " tt";
            }

            return newTimeFmt;
        }

        private void FormatDate(StringBuilder sb, DateTimeFormatInfo dateTimeFormatInfo,
            DateTimeRepresentation? dateStyle)
        {
            switch (dateStyle)
            {
                case DateTimeRepresentation.Full:
                case DateTimeRepresentation.Long:
                    sb.Append(dateTimeFormatInfo.LongDatePattern);
                    break;
                case DateTimeRepresentation.Medium:
                    sb.Append(Date.ToString(dateTimeFormatInfo));
                    break;
                case DateTimeRepresentation.Short:
                    sb.Append(Date.ToString(dateTimeFormatInfo.ShortDatePattern, dateTimeFormatInfo));
                    break;
            }
        }

        private void FormatTime(StringBuilder sb, DateTimeFormatInfo dateTimeFormatInfo,
            DateTimeRepresentation? timeStyle)
        {
            switch (timeStyle)
            {
                case DateTimeRepresentation.Full:
                case DateTimeRepresentation.Long:
                    sb.Append(Date.ToString(dateTimeFormatInfo.LongTimePattern, dateTimeFormatInfo));
                    break;
                case DateTimeRepresentation.Medium:
                    sb.Append(Date.ToString(dateTimeFormatInfo));
                    break;
                case DateTimeRepresentation.Short:
                    sb.Append(Date.ToString(dateTimeFormatInfo.ShortDatePattern, dateTimeFormatInfo));
                    break;
            }
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

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return new FluentDateTime(Date, Options);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((FluentDateTime)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Date.GetHashCode();
        }
    }

    /// <summary>
    /// Number options as defined in
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat">MDN Intl.DateTimeFormat()</see>
    /// </summary>
    public record FluentDateTimeOptions
    {
        /// <summary>
        /// The time zone to use for time formatting, such as "America/New_York" or "UTC".
        /// </summary>
        public string? TimeZone;

        #region LocaleOptions

        /// <summary>
        /// Whether to use 12-hour time (as opposed to 24-hour time). Possible values are true and false; the default is
        /// locale-dependent. When true, this option sets hourCycle to either "h11" or "h12"
        /// </summary>
        public bool? Hour12;
        

        /// <summary>
        /// Determines the representation style of the day of the week in a formatted date.
        /// Possible values are "Long", "Short", and "Narrow", as defined in the <see cref="DateTextFormat"/> enumeration.
        /// </summary>
        public DateTextFormat? Weekday;


        /// <summary>
        /// Specifies the representation style for the year value.
        /// Possible values include "Numeric" and "TwoDigit", determining how the year should be displayed.
        /// E.g., for the year 1999, "Numeric" would display <c>1999</c> while "TwoDigit" would display <c>99</c>.
        /// </summary>
        public NumericDateFormat? Year;

        /// <summary>
        /// Specifies the representation of the month in date/time formatting.
        /// Possible values are "Numeric", "TwoDigit", "Long", "Short", and "Narrow". 
        /// </summary>
        public MonthFormat? Month;

        /// <summary>
        /// Specifies the representation style for the day value.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 9th day of a month, "Numeric" would display <c>9</c> while "TwoDigit" would display <c>09</c>.
        /// </summary>
        public NumericDateFormat? Day;

        /// <summary>
        /// Specifies the formatting style for the hour in date and time representations.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 9th hour of the day, "Numeric" would display <c>9</c> while "TwoDigit" would display <c>09</c>.
        /// </summary>
        public NumericDateFormat? Hour;

        /// <summary>
        /// Specifies the formatting style for the minute in date and time representations.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// </summary>
        public NumericDateFormat? Minute;

        /// <summary>
        /// Specifies the formatting style for the seconds in date and time representations.
        /// Possible values include "Numeric" and "TwoDigit", determining how the day should be displayed.
        /// E.g., for the 2nd hour of the day, "Numeric" would display <c>2</c> while "TwoDigit" would display <c>02</c>.
        /// </summary>
        public NumericDateFormat? Second;

        /// <summary>
        /// The number of digits used to represent fractions of a second (any additional digits are truncated). Possible values are from 1 to 3.
        /// </summary>
        public FractionalSecodsDigit? FractionalSecondsDigit;

        /// <summary>
        /// Represents the format in which the time zone name is displayed.
        /// Possible values include "Long", "Short", "ShortOffset", "LongOffset", "ShortGeneric", and "LongGeneric".
        /// </summary>
        public TimeZoneRepresentation? TimeZoneName;

        #endregion


        #region Style

        /// <summary>
        /// Specifies the style used to represent a date in text formatting.
        /// </summary>
        public DateTimeRepresentation? DateStyle;

        /// <summary>
        /// Specifies the style used to represent a time in text formatting.
        /// </summary>
        public DateTimeRepresentation? TimeStyle;

        #endregion

        /// <summary>
        /// Can use the default formatter.
        /// </summary>
        public bool CanUseDefaultFormatter => Year == null && Weekday == null && Month == null
                                                && Day == null && Hour == null &&  Hour12 == null
                                                && Minute == null && Second == null && FractionalSecondsDigit == null;

        /// <summary>
        /// If <see cref="DateStyle"/> and/or <see cref="TimeStyle"/> is populated>.
        /// </summary>
        public DateTimeZoneFormat GetStyleFormat =>
            DateStyle != null && TimeStyle != null
                ? DateTimeZoneFormat.DateTime
                : DateStyle != null
                    ? DateTimeZoneFormat.Date
                    : DateTimeZoneFormat.Time;

        /// <summary>
        /// Helper to pick a date/time format to customize.
        /// </summary>
        public DateTimeZoneFormat GetStyleFields
        {
            get
            {
                var time = (Hour != null || Minute != null || Second != null || FractionalSecondsDigit != null) ? 2 : 0;
                var date = (Year != null || Month != null || Day != null || Weekday != null) ? 1 : 0;
                return (DateTimeZoneFormat)(date + time);
            }
        }

        /// <summary>
        /// Converts a dictionary of named arguments into an instance of <see cref="FluentDateTimeOptions"/>.
        /// </summary>
        /// <param name="namedArgs">A dictionary containing named arguments, where keys are strings representing parameter names, and values are <see cref="IFluentType"/> instances.</param>
        /// <returns>An instance of <see cref="FluentDateTimeOptions"/> configured based on the provided named arguments.</returns>
        public static FluentDateTimeOptions ToDateOptions(IDictionary<string, IFluentType> namedArgs)
        {
            var dateTimeOptions = new FluentDateTimeOptions();
            if (namedArgs.TryGetValue("hour12", out var ft1) &&
                ft1 is FluentString boolStr)
            {
                switch ((string)boolStr)
                {
                    case "true":
                        dateTimeOptions.Hour12 = true;
                        break;
                    case "false":
                        dateTimeOptions.Hour12 = false;
                        break;
                }
            }


            if (namedArgs.TryGetValue("weekday", out var ft3) &&
                ft3 is FluentString weekdayStr && weekdayStr.TryIntoDateTextFormat(out var weekday))
            {
                dateTimeOptions.Weekday = weekday;
            }


            if (namedArgs.TryGetValue("year", out var ft5) &&
                ft5 is FluentString yearStr && yearStr.TryIntoNumericFormat(out var year))
            {
                dateTimeOptions.Year = year;
            }

            if (namedArgs.TryGetValue("month", out var ft6) &&
                ft6 is FluentString monthStr && monthStr.TryIntoMonthFormat(out var month))
            {
                dateTimeOptions.Month = month;
            }

            if (namedArgs.TryGetValue("day", out var ft7) &&
                ft7 is FluentString dayStr && dayStr.TryIntoNumericFormat(out var day))
            {
                dateTimeOptions.Day = day;
            }

            if (namedArgs.TryGetValue("hour", out var ft8) &&
                ft8 is FluentString hourStr && hourStr.TryIntoNumericFormat(out var hour))
            {
                dateTimeOptions.Hour = hour;
            }

            if (namedArgs.TryGetValue("minute", out var ft9) &&
                ft9 is FluentString minuteStr && minuteStr.TryIntoNumericFormat(out var minute))
            {
                dateTimeOptions.Minute = minute;
            }

            if (namedArgs.TryGetValue("second", out var ft10) &&
                ft10 is FluentString secondStr && secondStr.TryIntoNumericFormat(out var second))
            {
                dateTimeOptions.Second = second;
            }

            if (namedArgs.TryGetValue("fractionalSecondsDigit", out var ft11) &&
                ft11.TryIntoFractionalSecond(out var fractionalSecond))
            {
                dateTimeOptions.FractionalSecondsDigit = fractionalSecond;
            }

            if (namedArgs.TryGetValue("timeZoneName", out var ft12) &&
                ft12 is FluentString timeZoneReprStr && timeZoneReprStr.TryIntoTimeZoneNameFormat(out var timeZoneName))
            {
                dateTimeOptions.TimeZoneName = timeZoneName;
            }

            if (namedArgs.TryGetValue("dateStyle", out var ft13) &&
                ft13 is FluentString dateStyleReprStr &&
                dateStyleReprStr.TryIntoDateTimeRepresentation(out var dateStyleRepr))
            {
                dateTimeOptions.DateStyle = dateStyleRepr;
            }

            if (namedArgs.TryGetValue("timeStyle", out var ft14) &&
                ft14 is FluentString timeStyleReprStr &&
                timeStyleReprStr.TryIntoDateTimeRepresentation(out var timeStyleRepr))
            {
                dateTimeOptions.TimeStyle = timeStyleRepr;
            }


            return dateTimeOptions;
        }
    }


    /// <summary>
    /// The hour cycle to use. Possible values are "h11", "h12", "h23", and "h24";
    /// the default is inferred from hour12 and locale. 
    /// </summary>
    public enum HourCycle : byte
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
        H24
    }

    /// <summary>
    /// Specifies the representation style for date and time values.
    /// </summary>
    public enum DateTextFormat : byte
    {
        /// <summary>
        /// Long representation style for time unit. E.g. `Thursday` or `Anno Domini`.
        /// </summary>
        Long = 1,

        /// <summary>
        /// Short representation style for time unit. E.g. `Thu` or `AD`.
        /// </summary>
        Short = 2,

        /// <summary>
        /// Extremely short representation style for time unit. E.g. `T` or `A`.
        /// </summary>
        Narrow = 3
    }

    /// <summary>
    /// Specifies the representation style for date and time values.
    /// </summary>
    public enum NumericDateFormat
    {
        /// <summary>
        /// Full numeric representation of temporal unit.
        /// </summary>
        Numeric,

        /// <summary>
        /// Numeric representation of a temporal unit with two digits.
        /// </summary>
        TwoDigit
    }

    /// <summary>
    /// Specifies the representation style for date and time values.
    /// </summary>
    public enum MonthFormat : byte
    {
        /// <summary>
        /// Full numeric representation of the month. E.g. <c>1</c> for <c>January</c>.
        /// </summary>
        Numeric = 1,

        /// <summary>
        /// Numeric representation of a month with two digits. E.g. <c>01</c> for <c>January</c>.
        /// </summary>
        TwoDigit = 2,

        /// <summary>
        /// Textual representation of a month. E.g. <c>January</c>.
        /// </summary>
        Long = 3,

        /// <summary>
        /// Abbreviate textual representation of a month. E.g.<c>Jan</c> for <c>January</c>.
        /// </summary>
        Short = 4,

        /// <summary>
        /// Extremely short textual representation of a month. E.g.<c>J</c> for <c>January</c>.
        /// </summary>
        Narrow = 5
    }

    /// <summary>
    /// Specifies the representation of fractional seconds for date and time values.
    /// </summary>
    public enum FractionalSecodsDigit : byte
    {
        /// <summary>
        /// One fractional second digit.
        /// </summary>
        OneDigit = 1,

        /// <summary>
        /// Two fractional second digits.
        /// </summary>
        TwoDigits = 2,

        /// <summary>
        /// Three fractional second digits.
        /// </summary>
        ThreeDigits = 3
    }

    /// <summary>
    /// The localized representation of the time zone name.
    /// </summary>
    public enum TimeZoneRepresentation : byte
    {
        /// <summary>
        /// Long localized form (e.g., <c>Pacific Standard Time</c>, <c>Nordamerikanische Westküsten-Normalzeit</c>)
        /// </summary>
        Long = 1,

        /// <summary>
        /// Short localized form (e.g., <c>PST</c>, <c>GMT-8</c>)
        /// </summary>
        Short = 2,

        /// <summary>
        /// Short localized GMT format (e.g.,<c>GMT-8</c>)
        /// </summary>
        ShortOffset = 3,

        /// <summary>
        /// Long localized GMT format (e.g., <c>GMT-08:00</c>)
        /// </summary>
        LongOffset = 4,

        /// <summary>
        /// Short generic non-location format (e.g., <c>PT</c>, <c>Los Angeles Zeit</c>)
        /// </summary>
        ShortGeneric = 5,

        /// <summary>
        /// Long generic non-location format (e.g., <c>Pacific Time</c>, <c>Nordamerikanische Westküstenzeit</c>)
        /// </summary>
        LongGeneric = 6
    }

    /// <summary>
    /// Which format to use when representing <see cref="FluentDateTime"/>
    /// </summary>
    public enum DateTimeZoneFormat : byte
    {
        /// <summary>
        /// The date part.
        /// </summary>
        Date = 1,

        /// <summary>
        /// The time part.
        /// </summary>
        Time = 2,

        /// <summary>
        /// Date and time part.
        /// </summary>
        DateTime = 3,
    }

    /// <summary>
    /// The localized representation of the date/time.
    /// </summary>
    public enum DateTimeRepresentation : byte
    {
        /// <summary>
        /// Full representation of date/time.
        /// </summary>
        Full = 1,

        /// <summary>
        /// Long representation of date/time.
        /// </summary>
        Long = 2,

        /// <summary>
        /// Medium representation of date/time.
        /// </summary>
        Medium = 3,

        /// <summary>
        /// Short representation of date/time.
        /// </summary>
        Short = 4,
    }

    /// <summary>
    /// Extensions from converting <see cref="FluentString"/> into <see cref="FluentNumberOptions"/> fields.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Attempts to convert the current <see cref="FluentString"/> instance into an <see cref="HourCycle"/> enumeration value.
        /// </summary>
        /// <param name="fs">The <see cref="FluentString"/> instance to convert.</param>
        /// <param name="style">When this method returns, contains the equivalent <see cref="HourCycle"/> value if the conversion succeeded; otherwise, the default value of <see cref="HourCycle"/>.</param>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public static bool TryIntoHourCycle(this FluentString fs, out HourCycle style)
        {
            bool conversionSuccess;
            switch ((string)fs)
            {
                case "h11":
                    conversionSuccess = true;
                    style = HourCycle.H11;
                    break;
                case "h12":
                    conversionSuccess = true;
                    style = HourCycle.H12;
                    break;
                case "h23":
                    conversionSuccess = true;
                    style = HourCycle.H23;
                    break;
                case "h24":
                    conversionSuccess = true;
                    style = HourCycle.H24;
                    break;
                default:
                    conversionSuccess = false;
                    style = default;
                    break;
            }

            return conversionSuccess;
        }

        /// <summary>
        /// Attempts to convert the current <see cref="FluentString"/> instance into a corresponding <see cref="DateTextFormat"/> value.
        /// </summary>
        /// <param name="fs">The <see cref="FluentString"/> instance to convert.</param>
        /// <param name="style">When the method returns, contains the corresponding <see cref="DateTextFormat"/> value if the conversion succeeded; otherwise, the default value of <see cref="DateTextFormat"/>.</param>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public static bool TryIntoDateTextFormat(this FluentString fs, out DateTextFormat style)
        {
            bool conversionSuccess;
            switch ((string)fs)
            {
                case "long":
                    conversionSuccess = true;
                    style = DateTextFormat.Long;
                    break;
                case "short":
                    conversionSuccess = true;
                    style = DateTextFormat.Short;
                    break;
                case "narrow":
                    conversionSuccess = true;
                    style = DateTextFormat.Narrow;
                    break;
                default:
                    conversionSuccess = false;
                    style = default;
                    break;
            }

            return conversionSuccess;
        }

        /// <summary>
        /// Attempts to convert the current <see cref="FluentString"/> instance into a <see cref="NumericDateFormat"/> value.
        /// </summary>
        /// <param name="fs">The <see cref="FluentString"/> instance to be converted.</param>
        /// <param name="style">When the method returns, contains the converted <see cref="NumericDateFormat"/> value if the conversion was successful; otherwise, contains the default value of <see cref="NumericDateFormat"/>.</param>
        /// <returns><c>true</c> if the conversion is successful; otherwise, <c>false</c>.</returns>
        public static bool TryIntoNumericFormat(this FluentString fs, out NumericDateFormat style)
        {
            bool conversionSuccess;
            switch ((string)fs)
            {
                case "numeric":
                    conversionSuccess = true;
                    style = NumericDateFormat.Numeric;
                    break;
                case "2-digit":
                    conversionSuccess = true;
                    style = NumericDateFormat.TwoDigit;
                    break;
                default:
                    conversionSuccess = false;
                    style = default;
                    break;
            }

            return conversionSuccess;
        }

        /// <summary>
        /// Attempts to convert the current <see cref="FluentString"/> instance into a corresponding <see cref="MonthFormat"/> enumeration value.
        /// </summary>
        /// <param name="fs">The <see cref="FluentString"/> to be converted to <see cref="MonthFormat"/>.</param>
        /// <param name="style">When this method returns, contains the converted <see cref="MonthFormat"/> enumeration value if the conversion succeeded, or the default value of <see cref="MonthFormat"/> if the conversion failed.</param>
        /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryIntoMonthFormat(this FluentString fs, out MonthFormat style)
        {
            bool conversionSuccess;
            switch ((string)fs)
            {
                case "numeric":
                    conversionSuccess = true;
                    style = MonthFormat.Numeric;
                    break;
                case "2-digit":
                    conversionSuccess = true;
                    style = MonthFormat.TwoDigit;
                    break;
                case "long":
                    conversionSuccess = true;
                    style = MonthFormat.Long;
                    break;
                case "short":
                    conversionSuccess = true;
                    style = MonthFormat.Short;
                    break;
                case "narrow":
                    conversionSuccess = true;
                    style = MonthFormat.Narrow;
                    break;
                default:
                    conversionSuccess = false;
                    style = default;
                    break;
            }

            return conversionSuccess;
        }

        /// <summary>
        /// Attempts to convert the current <see cref="IFluentType"/> instance into a <see cref="FractionalSecodsDigit"/> representation.
        /// </summary>
        /// <param name="ft">The <see cref="IFluentType"/> instance to convert.</param>
        /// <param name="style">
        /// When this method returns, contains the corresponding <see cref="FractionalSecodsDigit"/> value
        /// if the conversion was successful; otherwise, the default value of <see cref="FractionalSecodsDigit"/>.
        /// </param>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        public static bool TryIntoFractionalSecond(this IFluentType ft, out FractionalSecodsDigit style)
        {
            if (ft is FluentNumber fn && fn.TryPluralOperands(out var o))
            {
                switch (o.I)
                {
                    case 1:
                        style = FractionalSecodsDigit.OneDigit;
                        return true;
                    case 2:
                        style = FractionalSecodsDigit.TwoDigits;
                        return true;
                    case 3:
                        style = FractionalSecodsDigit.ThreeDigits;
                        return true;
                }
            }

            style = default;
            return false;
        }

        /// <summary>
        /// Attempts to convert the current <see cref="FluentString"/> instance into a <see cref="TimeZoneRepresentation"/> enumeration value.
        /// </summary>
        /// <param name="fs">The <see cref="FluentString"/> instance to convert.</param>
        /// <param name="style">
        /// When this method returns, contains the corresponding <see cref="TimeZoneRepresentation"/> value if the conversion is successful;
        /// otherwise, contains the default value of <see cref="TimeZoneRepresentation"/>.
        /// </param>
        /// <returns><c>true</c> if the conversion is successful; otherwise, <c>false</c>.</returns>
        public static bool TryIntoTimeZoneNameFormat(this FluentString fs, out TimeZoneRepresentation style)
        {
            bool conversionSuccess;
            switch ((string)fs)
            {
                case "long":
                    style = TimeZoneRepresentation.Long;
                    conversionSuccess = true;
                    break;
                case "short":
                    style = TimeZoneRepresentation.Short;
                    conversionSuccess = true;
                    break;
                case "shortOffset":
                    style = TimeZoneRepresentation.ShortOffset;
                    conversionSuccess = true;
                    break;
                case "longOffset":
                    style = TimeZoneRepresentation.LongOffset;
                    conversionSuccess = true;
                    break;
                case "shortGeneric":
                    style = TimeZoneRepresentation.ShortGeneric;
                    conversionSuccess = true;
                    break;
                case "longGeneric":
                    style = TimeZoneRepresentation.LongGeneric;
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
        /// Attempts to convert the current <see cref="FluentString"/> instance into a corresponding <see cref="DateTimeRepresentation"/> value.
        /// </summary>
        /// <param name="fs">The <see cref="FluentString"/> instance to convert.</param>
        /// <param name="style">When the method returns <c>true</c>, contains the <see cref="DateTimeRepresentation"/> value that corresponds to the string representation, if the conversion was successful; otherwise, contains the default value of <see cref="DateTimeRepresentation"/>.</param>
        /// <returns><c>true</c> if the conversion is successful; otherwise, <c>false</c>.</returns>
        public static bool TryIntoDateTimeRepresentation(this FluentString fs, out DateTimeRepresentation style)
        {
            bool conversionSuccess;
            switch ((string)fs)
            {
                case "full":
                    style = DateTimeRepresentation.Full;
                    conversionSuccess = true;
                    break;
                case "long":
                    style = DateTimeRepresentation.Long;
                    conversionSuccess = true;
                    break;
                case "short":
                    style = DateTimeRepresentation.Short;
                    conversionSuccess = true;
                    break;
                case "medium":
                    style = DateTimeRepresentation.Medium;
                    conversionSuccess = true;
                    break;
                default:
                    style = default;
                    conversionSuccess = false;
                    break;
            }

            return conversionSuccess;
        }
    }
}