using System;
using System.Collections.Generic;
using Linguini.Shared.Types;

namespace Linguini.Shared.Algorithm
{
    /// <summary>
    /// Provides functionality for expanding locale identifiers with the use of custom or predefined expansion functions.
    /// </summary>
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

        /// <summary>
        /// All expander applied to get the maximized locale identifier.
        /// </summary>
        public Func<LangLocId, LangLocId?>[] Expanders;

        /// <summary>
        /// Provides functionality for expanding locale identifiers using specified expansion rules.
        /// </summary>
        public LocaleExpander()
        {
            Expanders = new Func<LangLocId, LangLocId?>[]
            {
                Maximize,
            };
        }

        /// <summary>
        /// Provides functionality for expanding locale identifiers using specified or custom expansion mechanisms.
        /// </summary>
        /// <param name="expansions">The list of expansion functions to apply.</param>
        public LocaleExpander(List<Func<LangLocId, LangLocId?>> expansions)
        {
            Expanders = expansions.ToArray();
        }

        /// <summary>
        /// Executes the sequence of locale expansion functions on the provided locale identifier
        /// and returns the first expanded result that is not null.
        /// </summary>
        /// <param name="id">The locale identifier to be expanded using the registered expansion functions.</param>
        /// <returns>
        /// The expanded locale identifier if a suitable expansion is found; otherwise, null if none
        /// of the expansion functions produce a result.
        /// </returns>
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