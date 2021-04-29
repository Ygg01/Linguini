using System.Collections.Generic;

namespace PluralRules.Parser
{
    public class PluralRulesRaw
    {
        public List<PluralRuleRaw> OrdinalRules = new();
        public List<PluralRuleRaw> CardinalRules = new();
    }
}