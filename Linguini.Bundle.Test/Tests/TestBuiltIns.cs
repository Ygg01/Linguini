using System.Collections.Generic;
using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Tests
{
    [TestFixture]
    public class TestBuiltIns
    {
        public static IEnumerable<TestCaseData> TestNumberEnumerable()
        {
            yield return new TestCaseData(
                0.0,
                "en-US",
                new FluentNumberOptions(){},
                "0.0");
        }

        [Parallelizable]
        [Test]
        [TestCaseSource(nameof(TestNumberEnumerable))]
        public void TestNumbers(double number, string langLoc, FluentNumberOptions fluentNumberOptions, string expected)
        {
            var fluentNumber = (FluentNumber)number;
            var testContext = new FluentContext(langLoc, fluentNumberOptions);
            var actual = fluentNumber.AsString(testContext);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
    
}