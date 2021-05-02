using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace PluralRules
{
    public class TestRules
    {
        [Test]
        public void Test()
        {
            var actual = Rules
                .GetPluralCategory(
                    CultureInfo.CurrentCulture,
                    RuleType.Cardinal,
                    FluentNumber.FromString("0")
                );
            Assert.AreEqual(PluralCategory.Other, actual);
        }
    }
}
