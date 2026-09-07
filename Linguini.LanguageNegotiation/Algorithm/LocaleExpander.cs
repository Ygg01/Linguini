using System;
using System.Collections.Generic;
using Linguini.LanguageNegotiation.LangLoc;

namespace Linguini.LanguageNegotiation.Algorithm
{
    public class LocaleExpander
    {
        private static readonly Dictionary<string, string> RegionMatchingKeys = new Dictionary<string, string>{
            {"az","AZ"},
            {"bg","BG"},
            {"cs","CS"},
            {"de","DE"},
            {"es","ES"},
            {"fi","FI"},
            {"fr","FR"},
            {"it","IT"},
            {"lt","LT"},
            {"lv","LV"},
            {"nl","NL"},
            {"nu","NU"},
            {"pl","PL"},
            {"ro","RO"},
            {"ru","RU"},
        };
        
        
        private List<Func<LangLocId, LangLocId?>> _expandLocale = new List<Func<LangLocId, LangLocId?>>
        {
            id =>
            {
                if (id == new LangLocId("en"))
                {
                    return new LangLocId("en", "US", "Latn");
                }
                if (id == new LangLocId("fr"))
                {
                    return new LangLocId("fr", "FR", "Latn");
                }
                if (id == new LangLocId("sr"))
                {
                    return new LangLocId("sr", "SR", "Cyrl");
                }
                if (id == new LangLocId("sr", "SR"))
                {
                    return new LangLocId("sr", "SR", "Latn");
                }
                if (id == new LangLocId("az", "IR"))
                {
                    return new LangLocId("az", "IR", "Arab");
                }
                if (id == new LangLocId("zh", "GB"))
                {
                    return new LangLocId("zh", "GB", "Hant");
                }
                if (id == new LangLocId("zh", "US"))
                {
                    return new LangLocId("zh", "US", "Hant");
                }

                if (RegionMatchingKeys.TryGetValue(id.LanguageStr, out var region))
                {
                    return new LangLocId(id.LanguageStr, region);
                };
                
                return null;
            },
        };

        public LocaleExpander() {}
        public LocaleExpander(Func<LangLocId, LangLocId?>[] expandLocale) { _expandLocale.AddRange(expandLocale); }
        
        public bool Maximize(ref LangLocId languageIdentifier)
        {
            foreach (var func in _expandLocale)
            {
                var expand = func(languageIdentifier);
                if (expand == null) continue;
                
                languageIdentifier = expand.Value;
                return true;
            }

            return false;
        }
    }
}