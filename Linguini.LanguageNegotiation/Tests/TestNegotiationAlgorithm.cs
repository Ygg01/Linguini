using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Linguini.LanguageNegotiation.Algorithm;
using Linguini.LanguageNegotiation.LangLoc;
using NUnit.Framework;

namespace Linguini.LanguageNegotiation.Tests
{
    [TestFixture]
    public class TestNegotiationAlgorithm
    {
        private static readonly string[] EmptyStringArr = { };

        public static IEnumerable<TestCaseData> TestMatchingArgs()
        {
            yield return new TestCaseData(new[] { "fr", "en" }, new[] { "en-US", "fr-FR", "en", "fr" },
                    NegotiationStrategy.Matching, null)
                .Returns(new[] { "fr", "en" });
            yield return new TestCaseData(new[] { "und" }, new[] { "fr", "de", "it", "ru", "pl" },
                    NegotiationStrategy.Matching, null)
                .Returns(EmptyStringArr);
        }

        public static IEnumerable<TestCaseData> TestLookupArgs()
        {
            yield return new TestCaseData(new[] { "fr-FR", "en" }, new[] { "en-US", "fr-FR", "en", "fr" },
                    NegotiationStrategy.Lookup, "en-US")
                .Returns(new[] { "fr-FR" });
            yield return new TestCaseData(new[] { "fr", "en" }, new[] { "en-US", "fr-FR", "en" },
                    NegotiationStrategy.Lookup, "en-US")
                .Returns(new[] { "fr-FR" });
            yield return new TestCaseData(new[] { "en", "de" }, new[] { "en-GB", "en-US", "de" },
                    NegotiationStrategy.Lookup, "it")
                .Returns(new[] { "en-US" });
            yield return new TestCaseData(new[] { "und" }, new[] { "en-GB", "en-US", "de" }, NegotiationStrategy.Lookup,
                    "it")
                .Returns(new[] { "it" });
        }


        public static IEnumerable<TestCaseData> TestFilteringUnd()
        {
            yield return new TestCaseData(new[] { "und" }, new[] { "de", "pl-PL", "it", "fr-Latn-CA", "ru" },
                    NegotiationStrategy.Filtering, null)
                .Returns(EmptyStringArr);
            yield return new TestCaseData(new[] { "und" }, new[] { "und", "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[] { "und" });
            yield return new TestCaseData(new[] { "und" }, new[] { "und", "en-US" },
                    NegotiationStrategy.Filtering, "en-US")
                .Returns(new[] { "und", "en-US" });
        }

        [Parallelizable]
        [TestCaseSource(nameof(TestMatchingArgs))]
        [TestCaseSource(nameof(TestLookupArgs))]
        [TestCaseSource(nameof(TestFilteringUnd))]
        public string[] TestAlgo(string[] requested, string[] available, NegotiationStrategy strategy,
            string? defLang = null)
        {
            var defLangId = LangLocParser.Parse(defLang);
            var requestedLangs = requested
                .Select(LangLocParser.Parse)
                .OfType<LangLocId>()
                .ToList();
            var availableLangs = available
                .Select(LangLocParser.Parse)
                .OfType<LangLocId>()
                .ToList();
            var actual = NegotiationAlgorithm.NegotiateLanguages(requestedLangs, availableLangs, strategy, defLangId)
                .Select(s => s.ToString())
                .ToArray();
            return actual;
        }
    }
}