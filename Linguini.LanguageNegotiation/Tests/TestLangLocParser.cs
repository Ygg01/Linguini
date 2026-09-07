using Linguini.LanguageNegotiation.LangLoc;
using NUnit.Framework;

namespace Linguini.LanguageNegotiation.Tests
{
    [TestFixture]
    public class TestLangLocParser
    {
        [Parallelizable]
        [TestCase("en-US", "en", "US", null, ExpectedResult = true)]
        [TestCase("sr", "sr", null, null, ExpectedResult = true)]
        [TestCase("sr-RS", "sr", "RS", null, ExpectedResult = true)]
        [TestCase("sr-Cyrl-RS", "sr", "RS", "cyrl", ExpectedResult = true)]
        [TestCase("en-", null, null, null, ExpectedResult = false)]
        [TestCase("-en", null, null, null, ExpectedResult = false)]
        public bool Test(string input, string expectedLanguage, string? expectedRegion, string? expectedScript)
        {
            var result = LangLocParser.TryParse(
                input,
                out var _,
                out var langLocId
            );
            
            Assert.That(langLocId?.LanguageStr, Is.EqualTo(expectedLanguage));
            Assert.That(langLocId?.RegionStr, Is.EqualTo(expectedRegion));
            Assert.That(langLocId?.ScriptStr, Is.EqualTo(expectedScript));
            return result;
        }
    }
    
  
}