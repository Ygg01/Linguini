using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Linguini.Shared.Types;
using PluralRules.Generator.Types;

namespace PluralRules.Generator
{
    public static class HelperMethods
    {
        public static List<CldrRule> ProcessXmlPath(string? path)
        {
            var rules = new List<CldrRule>();

            if (string.IsNullOrEmpty(path))
            {
                return rules;
            }

            using (var cardinalFileStream = File.OpenRead(path))
            {
                var xmlElems = XDocument.Load(cardinalFileStream)
                    .XPathSelectElements("//supplementalData/plurals/*");
                rules = Convert(xmlElems);
            }

            return rules;
        }

        private static List<CldrRule> Convert(IEnumerable<XElement> plurals)
        {
            var retVal = new List<CldrRule>(40);
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

                    var rule = new CldrParser(element.FirstNode.ToString()).ParseRule();
                    rules.Add(new RuleMap(category.GetValueOrDefault(PluralCategory.Other), rule));
                }

                retVal.Add(new CldrRule(langs, rules));
            }

            return retVal;
        }
    }
}
