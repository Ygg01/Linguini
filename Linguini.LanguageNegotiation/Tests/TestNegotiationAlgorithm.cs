
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
            // yield return new TestCaseData(new[] { "fr-FR", "en" }, new[] { "en-US", "fr-FR", "en", "fr" },
            //         NegotiationStrategy.Lookup, "en-US")
            //     .Returns(new[] { "fr-FR" });
            // yield return new TestCaseData(new[] { "fr", "en" }, new[] { "en-US", "fr-FR", "en" },
            //         NegotiationStrategy.Lookup, "en-US")
            //     .Returns(new[] { "fr-FR" });
            yield return new TestCaseData(new[] { "en", "de" }, new[] { "en-GB", "en-US", "de" },
                    NegotiationStrategy.Lookup, "it")
                .Returns(new[] { "en-US" });
            // yield return new TestCaseData(new[] { "und" }, new[] { "en-GB", "en-US", "de" }, NegotiationStrategy.Lookup,
            //         "it")
            //     .Returns(new[] { "it" });
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
        
        public static IEnumerable<TestCaseData> TestFilteringAvailableAsRange()
        {
            yield return new TestCaseData(new[] { "en-US" }, new[] { "en" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "en"});
            yield return new TestCaseData(new[] { "en-Latn-US" }, new[] { "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "en-US"});
            // Only works with variants and extensions
            // yield return new TestCaseData(new[] { "en-US-windows" }, new[] { "en-US" },
            //         NegotiationStrategy.Filtering, null)
            //     .Returns(new []{ "en-US"});
            // yield return new TestCaseData(new[] { "ja-JP-windows" }, new[] { "ja" },
            //         NegotiationStrategy.Filtering, null)
            //     .Returns(new []{ "ja"});
            yield return new TestCaseData(new[] { "fr-CA", "de-DE" }, new[] { "fr", "it", "de" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "fr", "de"});
            yield return new TestCaseData(new[] { "en-Latn-GB", "en-Latn-IN" }, new[] { "en-IN", "en-GB" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "en-GB", "en-IN"});
        }
        
        public static IEnumerable<TestCaseData> TestFilteringCases()
        {
            yield return new TestCaseData(new[] { "fr_FR" }, new[] { "fr-FR" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "fr-FR"});
            yield return new TestCaseData(new[] { "fr_fr" }, new[] { "fr-fr" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "fr-fr"});
            yield return new TestCaseData(new[] { "fr_Fr" }, new[] { "fr-fR" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "fr-fR"});
            yield return new TestCaseData(new[] { "fr_lAtN_fr" }, new[] { "fr-Latn-FR" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "fr-Latn-FR"});
            yield return new TestCaseData(new[] { "fr_FR" }, new[] { "fr_FR" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "fr_FR"});
            // Only works with variants and extensions
            // yield return new TestCaseData(new[] { "fr_Cyrl_FR_macos" }, new[] { "fr_Cyrl_fr" },
            //         NegotiationStrategy.Filtering, null)
            //     .Returns(new []{ "fr_Cyrl_fr"});
            // yield return new TestCaseData(new[] { "fr_Cyrl_FR_mAcOs" }, new[] { "fr_Cyrl_fr-MaCoS" },
            //         NegotiationStrategy.Filtering, null)
            //     .Returns(new []{ "fr_Cyrl_fr-MaCoS"});
        }

        [Parallelizable]
        [TestCaseSource(nameof(TestMatchingArgs))]
        [TestCaseSource(nameof(TestLookupArgs))]
        [TestCaseSource(nameof(TestFilteringUnd))]
        [TestCaseSource(nameof(TestFilteringAvailableAsRange))]
        [TestCaseSource(nameof(TestFilteringCases))]
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