using System;
using System.Collections.Generic;
using Linguini.Shared.Types;

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

        public Func<LangLocId, LangLocId?>[] Expanders;

        public LocaleExpander()
        {
            Expanders = new Func<LangLocId, LangLocId?>[]
            {
                Maximize,
            };
        }

        public LocaleExpander(List<Func<LangLocId, LangLocId?>> expansions)
        {
            Expanders = expansions.ToArray();
        }

        public LangLocId? Expand(LangLocId id)
        {
            foreach (var expander in Expanders)
            {
                if (expander(id) != null)
                {
                    return expander(id);
                }
            }
            return null;
        }
        


        private static LangLocId? Maximize(LangLocId id)
        {
            if (id == LangLocId.EN)
            {
                return new LangLocId("en", "US", "Latn");
            }
            if (id == LangLocId.FR)
            {
                return new LangLocId("fr", "FR", "Latn");
            }
            if (id == LangLocId.SR)
            {
                return new LangLocId("sr", "SR", "Cyrl");
            }
            if (id == LangLocId.SR_RU)
            {
                return new LangLocId("sr", "SR", "Latn");
            }
            if (id == LangLocId.AZ_IR)
            {
                return new LangLocId("az", "IR", "Arab");
            }
            if (id == LangLocId.ZH_GB)
            {
                return new LangLocId("zh", "GB", "Hant");
            }
            if (id ==  LangLocId.ZH_US)
            {
                return new LangLocId("zh", "US", "Hant");
            }

            if (RegionMatchingKeys.TryGetValue(id.LanguageStr, out var region))
            {
                return new LangLocId(id.LanguageStr, region, id.ScriptStr);
            };
                
            return null;
        }
    }
}