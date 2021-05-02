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
            var func = RuleTable.GetPluralFunc(info.TwoLetterISOLanguageName, ruleType);
            if (PluralOperandsHelpers.TryParse(number.Value, out var op))
            {
                return func(op);
            }

            return PluralCategory.One;
        }
    }
}
