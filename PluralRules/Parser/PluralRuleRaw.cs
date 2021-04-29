using System.Collections.Generic;
using Linguini.Shared.Types;
using PluralRule.Ast;

namespace PluralRules.Parser
{
    public class PluralRuleRaw
    {
        public List<string> LangIds;
        public List<RuleMap> Rules;

        public PluralRuleRaw(List<string> langIds, List<RuleMap> ruleList)
        {
            LangIds = langIds;
            Rules = ruleList;
        }
    }

    public class RuleMap
    {
        private PluralCategory Category;
        private Rule Rule;

        public RuleMap(PluralCategory category, Rule rule)
        {
            Category = category;
            Rule = rule;
        }
    }
  
}