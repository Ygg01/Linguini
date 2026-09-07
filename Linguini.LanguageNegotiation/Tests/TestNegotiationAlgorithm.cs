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
        [TestCase("en-US;de-AT", "en;de;pl", NegotiationStrategy.Matching, ExpectedResult = "en;de")]
        public string Test(string requested, string available, NegotiationStrategy strategy)
        {
            var requestedLangs = requested
                .Split(';')
                .Select(s =>
                {
                    if (LangLocParser.TryParse(s, out _, out var langLocId))
                    {
                        return langLocId;
                    }
                    return null;
                })
                .OfType<LangLocId>()
                .ToList();
            var availableLangs = available
                .Split(';')
                .Select(s =>
                {
                    if (LangLocParser.TryParse(s, out _, out var langLocId))
                    {
                        return langLocId;
                    }
                    return null;
                })
                .OfType<LangLocId>()
                .ToList();
            var actual = NegotiationAlgorithm.NegotiateLanguages(requestedLangs, availableLangs, strategy);
            return String.Join(";", actual);
        }
    }
}