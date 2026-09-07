using System;
using System.Linq;
using Linguini.LanguageNegotiation.Algorithm;
using Linguini.LanguageNegotiation.LangLoc;
using NUnit.Framework;

namespace Linguini.LanguageNegotiation.Tests
{
    [TestFixture]
    public class TestNegotiationAlgorithm
    {
        [Parallelizable]
        [TestCase(new[] { "en-US", "de-AT" }, new[] { "en", "de", "pl" }, NegotiationStrategy.Matching, null,
            ExpectedResult = "en;de")]
        [TestCase(new[] { "und" }, new[] { "de", "pl-PL", "it", "fr-Latn-CA" }, NegotiationStrategy.Matching, null,
            ExpectedResult = "")]
        [TestCase(new[] { "und" }, new[] { "und", "en-US" }, NegotiationStrategy.Matching, "en-US",
            ExpectedResult = "und")]
        [TestCase(new[] { "und" }, new[] { "fr", "de", "it", "ru", "pl" }, NegotiationStrategy.Matching, null,
            ExpectedResult = "")]
        [TestCase(new[] {"fr", "en" }, new[] { "en-US", "fr-FR", "en", "fr" }, NegotiationStrategy.Matching, null,
            ExpectedResult = "fr;en")]
        public string Test(string[] requested, string[] available, NegotiationStrategy strategy,
            string? defLang = null)
        {
            var requestedLangs = requested
                .Select(s => LangLocParser.TryParse(s, out _, out var langLocId) ? langLocId : null)
                .OfType<LangLocId>()
                .ToList();
            var availableLangs = available
                .Select(s => LangLocParser.TryParse(s, out _, out var langLocId) ? langLocId : null)
                .OfType<LangLocId>()
                .ToList();
            var actual = NegotiationAlgorithm.NegotiateLanguages(requestedLangs, availableLangs, strategy);
            return String.Join(";", actual);
        }
    }
}