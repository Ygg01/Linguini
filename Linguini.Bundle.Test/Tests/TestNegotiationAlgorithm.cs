
using System.Collections.Generic;
using System.Linq;
using Linguini.Shared.Algorithm;
using Linguini.Shared.Types;
using NUnit.Framework;

namespace Linguini.LanguageNegotiation.Tests
{
    [TestFixture]
    public class TestNegotiationAlgorithm
    {
        private static readonly string[] EmptyStringArr = { };

        static IEnumerable<TestCaseData> TestMatchingArgs()
        {
            yield return new TestCaseData(new[] { "fr", "en" }, new[] { "en-US", "fr-FR", "en", "fr" },
                    NegotiationStrategy.Matching, null)
                .Returns(new[] { "fr", "en" });
            yield return new TestCaseData(new[] { "und" }, new[] { "fr", "de", "it", "ru", "pl" },
                    NegotiationStrategy.Matching, null)
                .Returns(EmptyStringArr);
        }

        static IEnumerable<TestCaseData> TestLookupArgs()
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


        static IEnumerable<TestCaseData> TestFilteringUnd()
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
        
        static IEnumerable<TestCaseData> TestFilteringAvailableAsRange()
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
        
        static IEnumerable<TestCaseData> TestFilteringCrossRegion()
        {
            yield return new TestCaseData(new[] { "en" }, new[] { "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "en-US"});
            yield return new TestCaseData(new[] { "en-US" }, new[] { "en-GB" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "en-GB"});
            yield return new TestCaseData(new[] { "en-Latn-US" }, new[] { "en-Latn-GB" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new []{ "en-Latn-GB"});
        }
        
        static IEnumerable<TestCaseData> TestFilteringCases()
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

        static IEnumerable<TestCaseData> TestFilteringDefaultLocale()
        {
            yield return new TestCaseData(new[] { "fr" }, new[] { "de", "it" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new string[]{});
            yield return new TestCaseData(new[] { "fr" }, new[] { "de", "it" },
                    NegotiationStrategy.Filtering, "en-US")
                .Returns(new []{ "en-US"});
            yield return new TestCaseData(new[] { "fr" }, new[] { "de", "en-US" },
                    NegotiationStrategy.Filtering,  "en-US")
                .Returns(new []{  "en-US"});
            yield return new TestCaseData(new[] { "fr", "de-DE" }, new[] { "de-DE", "fr-CA" },
                    NegotiationStrategy.Filtering,  "en-US")
                .Returns(new []{ "fr-CA", "de-DE", "en-US"});
        }
        
        static IEnumerable<TestCaseData> TestFilteringExactMatch()
        {
            yield return new TestCaseData(new[] { "en" }, new[] { "en" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en"});
            yield return new TestCaseData(new[] { "en-US" }, new[] { "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-US"});
            yield return new TestCaseData(new[] { "en-Latn-US" }, new[] { "en-Latn-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-Latn-US"});
            // Disabled until variants are implemented
            // yield return new TestCaseData(new[] { "en-Latn-US-windows" }, new[] { "en-Latn-US-windows" },
            //         NegotiationStrategy.Filtering, null)
            //     .Returns(new[]{"en-Latn-US-windows"});
            yield return new TestCaseData(new[] { "fr-FR" }, new[] { "de", "it", "fr-FR" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"fr-FR"});
            yield return new TestCaseData(new[] { "fr", "pl", "de-DE" }, new[] { "pl", "en-US", "de-DE" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"pl", "de-DE"});
        }
        
        static IEnumerable<TestCaseData> TestFilteringLikelySubtag()
        {
            yield return new TestCaseData(new[] { "en" }, new[] { "en-GB", "de", "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-US", "en-GB"});
            yield return new TestCaseData(new[] { "en" }, new[] {"en-Latn-GB", "de", "en-Latn-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-Latn-US", "en-Latn-GB"});
            yield return new TestCaseData(new[] { "fr" }, new[] {"fr-CA", "fr-FR" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"fr-FR", "fr-CA"});
            yield return new TestCaseData(new[] { "az-IR" }, new[] {"az-Latn", "az-Arab" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"az-Arab"});
            yield return new TestCaseData(new[] { "sr-RU" }, new[] {"sr-Cyrl", "sr-Latn" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"sr-Latn"});
            yield return new TestCaseData(new[] { "zh-GB" }, new[] {"zh-Hans", "zh-Hant" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"zh-Hant"});
            yield return new TestCaseData(new[] { "sr", "ru" }, new[] {"sr-Latn", "ru" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"ru"});
            yield return new TestCaseData(new[] { "sr-RU" }, new[] {"sr-Latn-RO", "sr-Cyrl" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"sr-Latn-RO"});
            yield return new TestCaseData(new[] { "en-CA" }, new[] {"en-ZA", "en-GB", "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-US", "en-ZA", "en-GB"});
        }
        
        static IEnumerable<TestCaseData> TestFilteringPriority()
        {
            yield return new TestCaseData(new[] { "en-Latn-US" }, new[] {"en-GB", "en-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-US", "en-GB"});
            yield return new TestCaseData(new[] { "en-US" }, new[] {"en-GB", "en" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en", "en-GB"});
            yield return new TestCaseData(new[] { "en" }, new[] {"en-Cyrl-US", "en-Latn-US" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new[]{"en-Latn-US"});
        }
        
        static IEnumerable<TestCaseData> TestFilteringRequestedUnd()
        {
            yield return new TestCaseData(new[] { "und" }, new[] {"de", "pl-PL", "it", "fr-Latn-CA", "ru" },
                    NegotiationStrategy.Filtering, null)
                .Returns(new string[]{});
            yield return new TestCaseData(new[] { "und" }, new[] {"und", "en-US" },
                    NegotiationStrategy.Filtering, "en-US")
                .Returns(new[]{"und", "en-US"});
        }

        [Parallelizable]
        [TestCaseSource(nameof(TestMatchingArgs))]
        [TestCaseSource(nameof(TestLookupArgs))]
        [TestCaseSource(nameof(TestFilteringUnd))]
        [TestCaseSource(nameof(TestFilteringAvailableAsRange))]
        [TestCaseSource(nameof(TestFilteringCases))]
        [TestCaseSource(nameof(TestFilteringCrossRegion))]
        [TestCaseSource(nameof(TestFilteringDefaultLocale))]
        [TestCaseSource(nameof(TestFilteringExactMatch))]
        [TestCaseSource(nameof(TestFilteringPriority))]
        [TestCaseSource(nameof(TestFilteringRequestedUnd))]
        [TestCaseSource(nameof(TestFilteringLikelySubtag))]
        public string[] TestAlgo(string[] requested, string[] available, NegotiationStrategy strategy,
            string? defLang = null)
        {
            
            LangLocParser.TryParse(defLang, out _, out var defLangId);
            var requestedLangs = requested
                .Select(LangLocParser.Parse)
                .ToList();
            var availableLangs = available
                .Select(LangLocParser.Parse)
                .ToList();
            var actual = NegotiationAlgorithm.NegotiateLanguages(requestedLangs, availableLangs, strategy, defLangId)
                .Select(s => s.ToString())
                .ToArray();
            return actual;
        }
    }
}