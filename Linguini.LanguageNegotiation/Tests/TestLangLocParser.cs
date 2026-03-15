using Linguini.LanguageNegotiation.LangLoc;
using NUnit.Framework;

namespace Linguini.LanguageNegotiation.Tests
{
    [TestFixture]
    public class TestLangLocParser
    {
        [Test]
        [TestCase("en-US", "en", "US", null, ExpectedResult = true)]
        public bool Test(string input, string expectedLanguage, string? expectedRegion, string? expectedScript)
        {
            var result = LangLocParser.TryParse(
                input,
                out var errors,
                out var language,
                out var script,
                out var region
            );
            Assert.That(language, Is.EqualTo(expectedLanguage));
            Assert.That(region, Is.EqualTo(expectedRegion));
            Assert.That(script, Is.EqualTo(expectedScript));
            return result;
        }
    }
}