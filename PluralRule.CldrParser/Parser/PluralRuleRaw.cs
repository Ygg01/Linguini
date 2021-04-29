using System;
using System.Collections.Generic;
using PluralRule.CldrParser.Ast;
using PluralRules.Types;

namespace PluralRule.CldrParser.Parser
{
    public record PluralRuleRaw
    {
        public List<string> LangIds;
        public List<RuleMap> Rules;

        public PluralRuleRaw(List<string> langIds, List<RuleMap> ruleList)
        {
            LangIds = langIds;
            Rules = ruleList;
        }
    }

    public record RuleMap(PluralCategory Category, Rule Rule);
  
}