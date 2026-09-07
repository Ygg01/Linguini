using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Linguini.LanguageNegotiation.LangLoc;

namespace Linguini.LanguageNegotiation.Algorithm
{
    public enum NegotiationStrategy
    {
        Filtering,
        Matching,
        Lookup,
    }
    

    public class NegotiationAlgorithm
    {
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

                    if (strategy == NegotiationStrategy.Filtering)
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

                    if (strategy == NegotiationStrategy.Filtering)
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

                if (localeExpander.Maximize(ref refReq) && TestStrategy(refReq, true, false))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Filtering)
                    {
                        continue;
                    }
                }

                // 4) Try to match against a variant as a range
                // TODO refReq.variants.clear();
                if (TestStrategy(refReq, true, true))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Filtering)
                    {
                        continue;
                    }
                }


                // 5) Try to match against the likely subtag without region
                refReq.Region = null;
                if (localeExpander.Maximize(ref refReq) && TestStrategy(refReq, true, false))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Filtering)
                    {
                        continue;
                    }
                }

                // 6) Try to match against a region as a range
                refReq.Region = null;
                if (localeExpander.Maximize(ref refReq) && TestStrategy(refReq, true, true))
                {
                    if (strategy == NegotiationStrategy.Lookup)
                    {
                        break;
                    }

                    if (strategy == NegotiationStrategy.Filtering)
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


        public static List<LangLocId> NegotiateLanguages(List<LangLocId> requested, List<LangLocId> available,
            NegotiationStrategy strategy, LangLocId? defaultLanguage = null, LocaleExpander? localeExpander = null)
        {
            var supported = FilterMatches(requested, available, strategy, localeExpander);

            if (defaultLanguage != null && (strategy == NegotiationStrategy.Lookup && supported.Count == 0 ||
                                            !supported.Contains(defaultLanguage.Value)))
            {
                supported.Add(defaultLanguage.Value);
            }

            return supported;
        }


        public static bool Matches(LangLocId lid1, LangLocId lid2, bool isRange1, bool isRange2)
        {
            return (isRange1 && lid1.Language.IsEmpty)
                   || (isRange2 && lid2.Language.IsEmpty)
                   || lid1.LanguageStr.Equals(lid2.LanguageStr)
                   && ((isRange1 && lid1.Script == null) || (isRange2 && lid2.Script == null) ||
                       Nullable.Equals(lid1.ScriptStr, lid2.ScriptStr))
                   && ((isRange1 && lid1.Region == null) || (isRange2 && lid2.Region == null) ||
                       Nullable.Equals(lid1.RegionStr, lid2.RegionStr));
        }
    }
}