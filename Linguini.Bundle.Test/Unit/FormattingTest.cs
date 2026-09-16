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
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(TestNumberFormatCases))]
        public string TestNumberFormat(double input, IFluentContext context)
        {
            var fn = (FluentNumber)input;
            return fn.AsString(context);
        }
    }
}