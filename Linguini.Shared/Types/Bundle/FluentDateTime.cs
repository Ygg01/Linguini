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

        /// <summary>
        /// Creates a new <see cref="FluentDateTime"/> instance with the specified formatting options applied.
        /// </summary>
        /// <param name="options">The formatting options to apply to the new <see cref="FluentDateTime"/> instance.</param>
        /// <returns>A new <see cref="FluentDateTime"/> instance with the provided formatting options.</returns>
        public FluentDateTime WithFormatting(FluentDateTimeOptions options)
        {
            return new FluentDateTime(Date, options);
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
                return Date.ToString(context.DateFormatStr, context.DateFormatInfo);
            }
            
            switch (context.DateTimeOptions.GetStyleFields)
            {
                case DateTimeZoneFormat.Time:
                    context.DateFormatStr = ExtractTimeFmt(context.DateTimeOptions, context.DateFormatInfo);
                    break;
                case DateTimeZoneFormat.Date:
                    context.DateFormatStr = ExtractDateFmt(context.DateTimeOptions, context.DateFormatInfo);
                    break;
                case DateTimeZoneFormat.DateTime:
                    var time = ExtractDateFmt(context.DateTimeOptions, context.DateFormatInfo);
                    var date  = ExtractDateFmt(context.DateTimeOptions, context.DateFormatInfo);
                    context.DateFormatStr = $"{date} {time}";
                    break;
                default:
                    context.DateFormatStr = context.DateFormatInfo.FullDateTimePattern;
                    break;
            }

            return Date.ToString(context.DateFormatStr, context.DateFormatInfo);
        }

        public static string ExtractTimeFmt(FluentDateTimeOptions dateTimeOptions, DateTimeFormatInfo info)
        {
            var timeFmt = new StringBuilder();
            var h = dateTimeOptions.Hour12  == true 
                ? "h" : "H";


            var hourStr = dateTimeOptions.Hour switch
            {
                NumericDateFormat.Numeric => $"{h}",
                NumericDateFormat.TwoDigit => $"{h}{h}",
                _ => null,
            };
            var minStr = (dateTimeOptions.Minute, hourStr == null) switch
            {
                (NumericDateFormat.TwoDigit, true) => "mm",
                (NumericDateFormat.TwoDigit, false) => ":mm",
                (NumericDateFormat.Numeric, true) => "m",
                (NumericDateFormat.Numeric, false) => ":m",
                _ => null,
            };
            var secStr = (dateTimeOptions.Second, minStr == null) switch
            {
                (NumericDateFormat.TwoDigit, true) => "ss",
                (NumericDateFormat.TwoDigit, false) => ":ss",
                (NumericDateFormat.Numeric, true) => "s",
                (NumericDateFormat.Numeric, false) => ":s",
                _ => null,
            };
            
            var fracStr = dateTimeOptions.FractionalSecondsDigit switch
            {
                FractionalSecodsDigit.OneDigit => ".f",
                FractionalSecodsDigit.TwoDigits => ".f",
                FractionalSecodsDigit.ThreeDigits => ".f",
                _ => ""
            };
            timeFmt.Append(hourStr);
            timeFmt.Append(minStr);
            timeFmt.Append(secStr);
            timeFmt.Append(fracStr);
            if (dateTimeOptions.Hour12 == true)
            {
                timeFmt.Append(" tt");
            }

            return timeFmt.ToString();
        }

        public static string ExtractDateFmt(FluentDateTimeOptions dateTimeOptions, DateTimeFormatInfo info)
        {
            if (!dateTimeOptions.TryGetRecognizedDate(out var recognizedDate))
            {
                return "";
            }

            var startingFmt = recognizedDate switch
            {
                DateFormatRecognized.Year => "yyyy",
                DateFormatRecognized.Month => "MM",
                DateFormatRecognized.Day => "dd",
                DateFormatRecognized.YearMonth => info.YearMonthPattern,
                DateFormatRecognized.MonthDay => info.MonthDayPattern,
                DateFormatRecognized.Short => info.ShortDatePattern,
                DateFormatRecognized.Long => info.LongTimePattern,
                _ => "",
            };
            var yearFmt = dateTimeOptions.Year switch
            {
                NumericDateFormat.Numeric => "yyyy",
                NumericDateFormat.TwoDigit => "yy",
                _ => "",
            };
            var monthFmt = dateTimeOptions.Month switch
            {
                MonthFormat.Numeric => "M",
                MonthFormat.TwoDigit => "MM",
                MonthFormat.Short => "MMM",
                MonthFormat.Long => "MMMM",
                _ => "",
            };

            var dayFmt = dateTimeOptions.Day switch
            {
                NumericDateFormat.Numeric => "d",
                NumericDateFormat.TwoDigit => "dd",
                _ => "",
            };
            
            var weekDayFmt = dateTimeOptions.Weekday switch
            {
                DateTextFormat.Long => "dddd",
                DateTextFormat.Short => "ddd",
                _ => "",
            };
            
            var finalFmt = Regex.Replace(startingFmt, "y{1,4}", yearFmt);
            finalFmt = Regex.Replace(finalFmt, "M{1,4}", monthFmt);
            finalFmt = Regex.Replace(finalFmt, "d{1,2}", dayFmt);
            finalFmt = Regex.Replace(finalFmt, "d{3,4}", weekDayFmt);

            return finalFmt;
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

        /// <summary>
        /// Implicitly converts a <see cref="DateTimeOffset"/> to a <see cref="FluentDateTime"/> instance.
        /// </summary>
        /// <param name="input">The <see cref="DateTimeOffset"/> value to be converted.</param>
        /// <returns>A new <see cref="FluentDateTime"/> instance initialized with the specified <see cref="DateTimeOffset"/> value.</returns>
        public static implicit operator FluentDateTime(DateTimeOffset input)
        {
            return new FluentDateTime(input, null);
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
    /// Specifies the recognized formats for representing date values.
    /// </summary>
    enum DateFormatRecognized : byte
    {
        /// <summary>
        /// Just year.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Just year.
        /// </summary>
        Year = 1,
        
        /// <summary>
        /// Just month.
        /// </summary>
        Month = 2,
        
        /// <summary>
        /// Just day.
        /// </summary>
        Day = 4,
        
        /// <summary>
        /// Just day.
        /// </summary>
        Weekday = 8,
        
        /// <summary>
        /// Format year and month.
        /// </summary>
        YearMonth = 3,
        
        /// <summary>
        /// Format month and day.
        /// </summary>
        MonthDay = 6,
        
        /// <summary>
        /// Formats year, month and day.
        /// </summary>
        Short = 7,
        
        /// <summary>
        /// Format year, month, day and weekday.
        /// </summary>
        Long = 15,
    }

    /// <summary>
    /// Extensions from converting <see cref="FluentString"/> into <see cref="FluentNumberOptions"/> fields.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Attempts to determine the recognized date format based on the provided <see cref="FluentDateTimeOptions"/>.
        /// </summary>
        /// <param name="fs">The <see cref="FluentDateTimeOptions"/> instance to evaluate.</param>
        /// <param name="formatRecognized">
        /// When this method returns, contains the recognized <see cref="DateFormatRecognized"/> value
        /// if the evaluation is successful; otherwise, contains <see cref="DateFormatRecognized.None"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a valid date format is recognized; otherwise, <c>false</c>.
        /// </returns>
        internal static bool TryGetRecognizedDate(this FluentDateTimeOptions fs, out DateFormatRecognized formatRecognized)
        {
            var year = fs.Year != null ? 1 : 0;
            var month = fs.Month != null ? 2 : 0;
            var day = fs.Day != null ? 4 : 0;
            var weekDay = fs.Weekday != null ? 8 : 0;

            formatRecognized = (year + month + day + weekDay) switch
            {
                1 => DateFormatRecognized.Year,
                2 => DateFormatRecognized.Month,
                4 => DateFormatRecognized.Day,
                8 => DateFormatRecognized.Weekday,
                3 => DateFormatRecognized.YearMonth,
                6 => DateFormatRecognized.MonthDay,
                7 => DateFormatRecognized.Short,
                15 => DateFormatRecognized.Long,
                _ => DateFormatRecognized.None,
            };

            return formatRecognized != DateFormatRecognized.None;
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