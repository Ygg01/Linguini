using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using PluralRulesGenerated;

namespace PluralRules
{
    public static class Rules
    {
        public static PluralCategory GetPluralCategory(CultureInfo info, RuleType ruleType, FluentNumber number)
        {
            var specialCase = RuleTable.UseFourLetter(info.Name, ruleType);
            var langStr = GetPluralRuleLang(info, specialCase);
            var func = RuleTable.GetPluralFunc(langStr, ruleType);
            if (PluralOperandsHelpers.TryPluralOperands(number, out var op))
            {
                return func(op);
            }

            return PluralCategory.Other;
        }

        private static string GetPluralRuleLang(CultureInfo info, bool specialCase)
        {
            if (CultureInfo.InvariantCulture.Equals(info))
            {
                // When culture info is uncertain we default to common 
                // language behavior
                return "root";
            }
            var langStr = specialCase
                ? info.Name.Replace('-', '_')
                : info.TwoLetterISOLanguageName;
            return langStr;
        }
    }
}
