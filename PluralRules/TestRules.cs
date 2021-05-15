using System;
using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;
using PluralRulesGenerated.Test;

namespace PluralRules
{
    [TestFixture]
    [Parallelizable]
    public class TestRules
    {
        public static readonly object?[][] _ordinalTestData = RuleTableTest.OrdinalTestData;
        public static readonly object?[][] _cardinalTestData = RuleTableTest.CardinalTestData;

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(_cardinalTestData))]
        public void TestCardinal(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, lower, expected);
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(_ordinalTestData))]
        public void TestOrdinal(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, lower, expected);
        }

        [Test]
        [Parallelizable]
        [TestCase("root", RuleType.Cardinal, false, "0", "15", PluralCategory.Other)]
        public void TestIndividual(string cultureStr, RuleType type, bool isDecimal, string lower, string? upper,
            PluralCategory expected)
        {
            TestData(cultureStr, type, lower, expected);
        }


        private static void TestData(string cultureStr, RuleType type, string lower, PluralCategory expected)
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

            var value = FluentNumber.FromString(lower);
            var actual = Rules.GetPluralCategory(info, type, value);

            Assert.AreEqual(expected, actual);
        }
    }
}
