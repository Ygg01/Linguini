using System;
using System.Collections.Generic;

namespace PluralRule.CldrParser.Parser
{
    public record PluralRuleRaw
    {
        public String LangId;
        public List<(string, string)> Rules;

        public PluralRuleRaw(string ruleName, List<(string, string)> ruleList)
        {
            LangId = ruleName;
            Rules = ruleList;
        }
    }
}