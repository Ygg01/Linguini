using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    public class FluentNumberTests
    {
        [TestCase("0.0", 0.0, 0u, 1, 0, 0, 0, 0)]
        [TestCase("-1.0", 1.0, 1u, 1, 0, 0, 0, 0)]
        [TestCase("-20.1", 20.1, 20u, 1, 1, 1, 1, 0)]
        [TestCase("-20.10", 20.1, 20u, 2, 1, 10, 1, 0)]
        [TestCase("-2e2", 200.0, 200u, 0, 0, 0, 0, 2)]
        [TestCase("-2c2", 200.0, 200u, 0, 0, 0, 0, 2)]
        [TestCase("-2c-2", 0.02, 0u, 0, 0, 0, 0, -2)]
        [TestCase("1", 1, 1u, 0, 0, 0, 0, 0)]
        [TestCase("1.0", 1, 1u, 1, 0, 0, 0, 0)]
        [TestCase("1.00", 1, 1u, 2, 0, 0, 0, 0)]
        [TestCase("1.3", 1.3, 1u, 1, 1, 3, 3, 0)]
        [TestCase("1.30", 1.3, 1u, 2, 1, 30, 3, 0)]
        [TestCase("1.03", 1.03, 1u, 2, 2, 3, 3, 0)]
        [TestCase("1.230", 1.23, 1u, 3, 2, 230, 23, 0)]
        [TestCase("1200000", 1200000, 1200000u, 0, 0, 0, 0, 0)]
        [TestCase("1.2e6", 1200000, 1200000u, 0, 0, 0, 0, 6)]
        [TestCase("123c6", 123000000, 123000000u, 0, 0, 0, 0, 6)]
        [TestCase("123c5", 12300000, 12300000u, 0, 0, 0, 0, 5)]
        [TestCase("1200.50", 1200.5, 1200u, 2, 1, 50, 5, 0)]
        [TestCase("1.20050e3", 1200.5, 1200u, 2, 1, 50, 5, 3)]
        public void TestCase(string numberStr, double abs, ulong intDigit, int visibleFracDigit, int withoutZFracDigit, long fractionWithZ, long trailingZRemoved, long exp)
        {
            var number = FluentNumber.FromString(numberStr);

            number.TryPluralOperands(out var actualPluralOperands);
            var expectedPluralOperand = new PluralOperands(abs, intDigit, visibleFracDigit, withoutZFracDigit, fractionWithZ, trailingZRemoved, exp);
            
            
            Assert.That(actualPluralOperands, Is.EqualTo(expectedPluralOperand));
        }
    }
}