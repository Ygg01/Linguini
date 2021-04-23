using System.Collections.Generic;


namespace PluralRule.CldrParser.Parser
{
    public class PluralRulesRaw
    {
        public List<PluralRuleRaw> OrdinalRules = new();
        public List<PluralRuleRaw> CardinalRules = new();
    }
}