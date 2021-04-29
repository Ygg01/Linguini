using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PluralRule.CldrParser.Ast;
using PluralRules.Types;

namespace PluralRule.CldrParser.Parser
{
    public class XmlConverter
    {
        public static List<PluralRuleRaw> convert(IEnumerable<XElement> plurals)
        {
            var retVal = new List<PluralRuleRaw>(40);
            foreach (var pluralRule in plurals)
            {
                var langs = pluralRule.Attribute("locales")!
                    .Value.Split(" ")
                    .ToList();
                var rules = new List<RuleMap>();
                foreach (var element in pluralRule.Elements())
                {
                    var countTag = element.Attribute("count")?.Value;
                    if (!PluralCategoryHelper.TryFromString(countTag, out var category))
                    {
                        continue;
                    }

                    var rule = new CldrParser.ParserPlural(element.FirstNode.ToString()).ParseRule();
                    rules.Add(new RuleMap(category.GetValueOrDefault(PluralCategory.Other), rule));
                }

                retVal.Add(new PluralRuleRaw(langs, rules));
            }

            return retVal;
        }
    }
}
