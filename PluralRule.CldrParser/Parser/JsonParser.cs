using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PluralRules.Types;

namespace PluralRule.CldrParser.Parser
{
    public class JsonParser
    {
        public static List<PluralRuleRaw> GetElements(Stream stream, RuleType category)
        {
            var root = JsonDocument.Parse(stream).RootElement;
            var elementsToParse = new List<PluralRuleRaw>(250);
            string elementName = "";
            switch (category)
            {
                case RuleType.Cardinal:
                    elementName = "plurals-type-cardinal";
                    break;
                case RuleType.Ordinal:
                    elementName = "plurals-type-ordinal";
                    break;
            }

            if (root.TryGetProperty("supplemental", out var supplemental)
            && supplemental.TryGetProperty(elementName, out var rulesElem))
            {
                foreach (var ruleElem in rulesElem.EnumerateObject())
                {
                    var ruleName = ruleElem.Name;
                    var ruleList = new List<(string, string)>();
                    foreach (var pluralRule in ruleElem.Value.EnumerateObject())
                    {
                        ruleList.Add((pluralRule.Name, pluralRule.Value.GetString()));
                    }
                    elementsToParse.Add(new PluralRuleRaw(ruleName, ruleList));
                }
            }

            return elementsToParse;
        }
    }
}