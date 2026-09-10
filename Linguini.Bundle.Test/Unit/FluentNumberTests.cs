using System.Globalization;

using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    public class FluentNumberTests
    {
        [TestCase("0.0", 0.0, 0u, 1, 0, 0, 0)]
        public void TestCase(string numberStr, double n, ulong i, int v, int w, long f, long t)
        {
            var number = FluentNumber.FromString(numberStr);

            number.TryPluralOperands(out var actualPluralOperands);
            var expectedPluralOperand = new PluralOperands(n, i, v, w, f, t);
            
            
            Assert.That(expectedPluralOperand, Is.EqualTo(actualPluralOperands));
        }
    }
}