using System;
using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;
using PluralRulesGenerated.Test;
using static Linguini.Bundle.Resolver.ResolverHelpers.PluralRules;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [Parallelizable]
    public class TestRules
    {
        public static readonly object?[][] OrdinalTestData = RuleTableTest.OrdinalTestData;
        public static readonly object?[][] CardinalTestData = RuleTableTest.CardinalTestData;

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(CardinalTestData))]
        public void TestCardinal(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, isDecimal, lower, upper, expected);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(OrdinalTestData))]
        public void TestOrdinal(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, isDecimal, lower, upper, expected);
        }

        [Test]
        [Parallelizable]
        [TestCase("root", RuleType.Cardinal, false, "0", "15", PluralCategory.Other)]
        public void TestIndividual(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, isDecimal, lower, upper, expected);
        }


        private static void TestData(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            CultureInfo info;
            try
            {
                info = new CultureInfo(cultureStr);
            }
            catch (Exception)
            {
                info = CultureInfo.InvariantCulture;
            }

            // If upper limit exist, we probe the range a bit
            if (upper != null)
            {
                var start = FluentNumber.FromString(lower);
                var end = FluentNumber.FromString(upper);
                var midDouble = (end.Value - start.Value) / 2 + start;
                FluentNumber mid = isDecimal
                    ? midDouble
                    : Convert.ToInt32(Math.Floor(midDouble));

                var actualStart = GetPluralCategory(info, type, start);
                Assert.AreEqual(expected, actualStart, $"Failed on start of range: {start}");
                var actualEnd = GetPluralCategory(info, type, end);
                Assert.AreEqual(expected, actualEnd, $"Failed on end of range: {end}");
                var actualMid = GetPluralCategory(info, type, mid);
                Assert.AreEqual(expected, actualMid, $"Failed on middle of range: {mid}");
            }
            else
            {
                var value = FluentNumber.FromString(lower);
                var actual = GetPluralCategory(info, type, value);

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
