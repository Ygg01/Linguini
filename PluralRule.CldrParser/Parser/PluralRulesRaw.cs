using System.Collections.Generic;


namespace PluralRule.CldrParser.Parser
{
    public class PluralRulesRaw
    {
        public List<PluralRuleRaw> OrdinalRules { get; set; }
        public List<PluralRuleRaw> CardinalRules { get; set; }
    }
}