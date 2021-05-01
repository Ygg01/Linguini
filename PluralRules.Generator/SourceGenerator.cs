using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PluralRules.Generator.Types;

namespace PluralRules.Generator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private List<CldrRule> ordinalRules = new();
        private List<CldrRule> cardinalRules = new();
        public void Initialize(GeneratorInitializationContext context)
        {
            //
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var excp = "";
            try
            {
                cardinalRules = ProcessXmlPath(
                    "C:\\BACKUP\\projects\\Linguini\\PluralRules.Generator\\cldr_data\\plurals.xml");
            }
            catch (Exception e)
            {
                excp = e.Message;
            }

            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder($@"
using System;
using Linguini.Shared.Types;

namespace PluralRulesGenerated
{{
    public static class RuleTable
    {{
        public static void SayHello3() 
        {{
            Console.WriteLine(""Hello from generated {cardinalRules.Count} code!!"");
            Console.WriteLine(""{excp}"");
        }}
    }}
}}
");
            context.AddSource("helloWorldGenerated",  SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

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

                    var rule = new CldrParser(element.FirstNode.ToString()).ParseRule();
                    rules.Add(new RuleMap( countTag!, rule));
                }

                retVal.Add(new CldrRule(langs, rules));
            }

            return retVal;
        }
    }
}
