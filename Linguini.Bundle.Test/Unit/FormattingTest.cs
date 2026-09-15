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
            yield return new TestCaseData(123, new FluentContext(LangLocId.EN, new FluentNumberOptions()
            {
                MinimumFractionDigits = 2
            })).Returns("123.00");
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