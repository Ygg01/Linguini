using System.Globalization;
using Linguini.Bundle.Types;
using Linguini.Shared.Types;

namespace PluralRules
{
    public static class Rules
    {
        public static PluralCategory GetPluralCategory(CultureInfo info, RuleType ruleType, FluentNumber number)
        {
            PluralRulesGenerated.RuleTable.SayHello3();
            return PluralCategory.Other;
        }
    }
}
