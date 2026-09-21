using System;
using System.Collections.Generic;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    public class FormattingTest
    {
        private static IEnumerable<TestCaseData> TestNumberFormatCases()
        {
            // Simple Formats
            yield return new TestCaseData(123, new FluentContext(LangLocId.EN, new FluentNumberOptions()
            {
                MinimumFractionDigits = 2
            })).Returns("123.00");
            
            yield return new TestCaseData(123.1, new FluentContext(LangLocId.EN, new FluentNumberOptions()
            {
                MinimumFractionDigits = 4
            })).Returns("123.1000");
            
            yield return new TestCaseData(11.1, new FluentContext(LangLocId.EN, new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Currency,
                MinimumFractionDigits = 3
            })).Returns("¤11.100");
            
            // Decimal complex formats
            yield return new TestCaseData(123.45, new FluentContext(LangLocId.EN, new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Decimal,
                MaximumFractionDigits = 1,
                MinimumFractionDigits = 3,
            })).Returns("123.450");
            yield return new TestCaseData(123.4567, new FluentContext(LangLocId.EN, new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Decimal,
                MaximumFractionDigits = 3,
                MinimumFractionDigits = 1,
            })).Returns("123.457");
            
            // Currency complex formats
            yield return new TestCaseData(123.4567, new FluentContext("en-US", new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Currency,
                MaximumFractionDigits = 3,
                MinimumFractionDigits = 1,
            })).Returns("$123.457");
            yield return new TestCaseData(-456.789, new FluentContext("en-US", new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Currency,
                MaximumFractionDigits = 2,
                MinimumFractionDigits = 1,
            })).Returns("-$456.79");
            yield return new TestCaseData(-456.789, new FluentContext("en-US", new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Currency,
                Currency = "USD",
                MaximumFractionDigits = 2,
                MinimumFractionDigits = 1,
            })).Returns("-USD456.79");
            
            // Percent complex formats
            yield return new TestCaseData(0.41789, new FluentContext("en-US", new FluentNumberOptions()
            {
                Style = FluentNumberStyle.Percent,
                MaximumFractionDigits = 2,
                MinimumFractionDigits = 1,
            })).Returns("41.79%");
            
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(TestNumberFormatCases))]
        public string TestNumberFormat(double input, IFluentContext context)
        {
            var fn = (FluentNumber)input;
            return fn.AsString(context);
        }

        private static IEnumerable<TestCaseData> TestDateFormatCases()
        {
            // Time formatting
            yield return new TestCaseData(
                new DateTimeOffset(2023, 2, 1, 1, 3, 4, TimeSpan.Zero),
                new FluentContext("en-US", dateTimeOptions: new FluentDateTimeOptions()
                {
                    Minute = NumericDateFormat.Numeric,
                    Hour = NumericDateFormat.TwoDigit,
                })).Returns("01:3");
            yield return new TestCaseData(
                new DateTimeOffset(2023, 2, 1, 2, 3, 4, TimeSpan.Zero),
                new FluentContext("en-US", dateTimeOptions: new FluentDateTimeOptions()
                {
                    Second = NumericDateFormat.TwoDigit,
                    Hour = NumericDateFormat.TwoDigit,
                })).Returns("0204");
            yield return new TestCaseData(
                new DateTimeOffset(2023, 2, 1, 12, 3, 4, TimeSpan.Zero),
                new FluentContext("en-US", dateTimeOptions: new FluentDateTimeOptions()
                {
                    Second = NumericDateFormat.TwoDigit,
                    Minute = NumericDateFormat.TwoDigit,
                    Hour = NumericDateFormat.TwoDigit,
                    FractionalSecondsDigit = FractionalSecodsDigit.OneDigit
                })).Returns("12:03:04.0");
            // Date
            yield return new TestCaseData(
                new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new FluentContext("en-US", dateTimeOptions: new FluentDateTimeOptions()
                {
                    Year = NumericDateFormat.Numeric,
                    Month = MonthFormat.TwoDigit,
                })).Returns("01 2021");
            yield return new TestCaseData(
                new DateTimeOffset(2022, 2, 1, 0, 0, 0, TimeSpan.Zero),
                new FluentContext("en-US", dateTimeOptions: new FluentDateTimeOptions()
                {
                    Day = NumericDateFormat.Numeric,
                    Month = MonthFormat.Long,
                })).Returns("February 1");
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(TestDateFormatCases))]
        public string TestDateFormat(DateTimeOffset input, IFluentContext context)
        {
            var fn = (FluentDateTime)input;
            return fn.AsString(context);
        }
    }
}