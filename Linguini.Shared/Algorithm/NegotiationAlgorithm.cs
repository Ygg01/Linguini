using System;
using System.Collections.Generic;
using System.Linq;
using Linguini.Shared.Types;

namespace Linguini.Shared.Algorithm
{
    /// <summary>
    /// Defines the strategies used for language negotiation.
    /// </summary>
    public enum NegotiationStrategy
    {
        /// <summary>
        /// Filtering
        /// </summary>
        Filtering,

        /// <summary>
        /// Tries to exact match the requested locale against the available locales.
        /// </summary>
        Matching,
        
        /// <summary>
        /// Lookup as defined in <a href="https://datatracker.ietf.org/doc/html/rfc4647#section-3.4">RFC 4647</a>.
        /// </summary>
        Lookup,
    }


    /// <summary>
    /// Represents a language negotiation algorithm designed to match requested languages
    /// against available languages based on various negotiation strategies.
    /// </summary>
    public class NegotiationAlgorithm
    {
        /// <summary>
        /// Filters the requested language tags against the available language tags based
        /// on the specified negotiation strategy, optionally utilizing a locale expander.
        /// </summary>
        /// <param name="requested">The list of requested language tags.</param>
        /// <param name="available">The list of available language tags.</param>
        /// <param name="strategy">The negotiation strategy to apply during filtering.</param>
        /// <param name="localeExpander">
        /// An optional locale expander object for expanding language ranges.
        /// If not provided, a default instance is created.
        /// </param>
        /// <returns>
        /// A list of language tags that match the filtering conditions based on the specified strategy.
        /// </returns>
        public static List<LangLocId> FilterMatches(List<LangLocId> requested, List<LangLocId> available,
            NegotiationStrategy strategy, LocaleExpander? localeExpander = null)
        {
            localeExpander ??= new LocaleExpander();
            var supportedLocale = new List<LangLocId>();
            var availableLocale = new List<LangLocId>(available);

            foreach (var req in requested.AsReadOnly())
            {
                var refReq = req;
                // 1) Try to find a simple (case-insensitive) string match for the request.
                if (TestStrategy(refReq, false, false))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Matching)
                    {
                        continue;
                    }
                }

                // 2) Try to match against the available locales treated as ranges.
                if (TestStrategy(refReq, true, false))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Matching)
                    {
                        continue;
                    }
                }

                // Per Unicode TR35, 4.4 Locale Matching, we don't add likely subtags to
                // requested locales, so we'll skip it from the rest of the steps.
                if (refReq.Language.IsEmpty)
                {
                    continue;
                }

                // 3) Try to match against a maximized version of the requested locale);
                var expandedRefReq = localeExpander.Expand(refReq);
                if (expandedRefReq != null)
                {
                    refReq = expandedRefReq.Value;
                    if (TestStrategy(refReq, true, false))
                    {
                        if (strategy == NegotiationStrategy.Lookup)
                        {
                            break;
                        }

                        if (strategy == NegotiationStrategy.Matching)
                        {
                            continue;
                        }
                    }
                }


                // 4) Try to match against a variant as a range
                // TODO refReq.variants.clear();
                // if (TestStrategy(refReq, true, true))
                // {
                //     if (strategy == NegotiationStrategy.Lookup)
                //     {
                //         break;
                //     }
                //
                //     if (strategy == NegotiationStrategy.Matching)
                //     {
                //         continue;
                //     }
                // }


                // 5) Try to match against the likely subtag without region
                refReq = refReq.ClearRegion();
                var expandedRefReq5 = localeExpander.Expand(refReq);
                if (expandedRefReq5 != null)
                {
                    refReq = expandedRefReq5.Value;
                    if (TestStrategy(refReq, true, false))
                    {
                        if (strategy == NegotiationStrategy.Lookup)
                        {
                            break;
                        }

                        if (strategy == NegotiationStrategy.Matching)
                        {
                            continue;
                        }
                    }
                }


                // 6) Try to match against a region as a range
                refReq = refReq.ClearRegion();
                if (TestStrategy(refReq, true, true))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Matching)
                    {
                        continue;
                    }
                }
            }

            return supportedLocale;

            bool TestStrategy(LangLocId req, bool isRange1, bool isRange2)
            {
                var matchFound = false;
                availableLocale = availableLocale.Where(locale =>
                {
                    if (strategy != NegotiationStrategy.Filtering && matchFound)
                    {
                        return true;
                    }

                    if (Matches(locale, req, isRange1, isRange2))
                    {
                        matchFound = true;
                        supportedLocale.Add(locale);
                        return false;
                    }

                    return true;
                }).ToList();
                return matchFound;
            }
        }


        /// <summary>
        /// Negotiates a list of languages from the requested and available languages,
        /// using a specified negotiation strategy. Optionally includes a default language
        /// and uses a locale expander if provided.
        /// </summary>
        /// <param name="requested">The list of requested language tags.</param>
        /// <param name="available">The list of available language tags for negotiation.</param>
        /// <param name="strategy">The strategy to use for negotiating language matches.</param>
        /// <param name="defaultLanguage">
        /// An optional default language to include in the result if no matches are found
        /// or if not already present.
        /// </param>
        /// <param name="localeExpander">
        /// An optional locale expander to expand the list of requested or available languages.
        /// If not provided, expansion is skipped.
        /// </param>
        /// <returns>
        /// A list of negotiated language tags based on the specified negotiation strategy,
        /// optionally including the default language if applicable.
        /// </returns>
        public static List<LangLocId> NegotiateLanguages(List<LangLocId> requested, List<LangLocId> available,
            NegotiationStrategy strategy, LangLocId? defaultLanguage = null, LocaleExpander? localeExpander = null)
        {
            var supported = FilterMatches(requested, available, strategy, localeExpander);

            if (defaultLanguage == null) return supported;

            if (strategy == NegotiationStrategy.Lookup)
            {
                if (supported.Count == 0)
                {
                    supported.Add(defaultLanguage.Value);
                }
            }
            else if (!supported.Contains(defaultLanguage.Value))
            {
                supported.Add(defaultLanguage.Value);
            }

            return supported;
        }


        static bool Matches(LangLocId lid1, LangLocId lid2, bool isRange1, bool isRange2)
        {
            return (isRange1 && lid1.Language.IsEmpty)
                   || (isRange2 && lid2.Language.IsEmpty)
                   || String.Equals(lid1.LanguageStr, lid2.LanguageStr, StringComparison.OrdinalIgnoreCase)
                   && SubMatch(lid1.ScriptStr, lid2.ScriptStr, isRange1, isRange2)
                   && SubMatch(lid1.RegionStr, lid2.RegionStr, isRange1, isRange2);
        }
        
        static bool SubMatch(string? lid1, string? lid2, bool isRange1, bool isRange2)
        {
            return (isRange1 && lid1 == null)
                   || (isRange2 && lid2 == null)
                   || String.Equals(lid1, lid2, StringComparison.OrdinalIgnoreCase);
        }
    }
}