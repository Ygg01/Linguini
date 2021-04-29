using System;
using System.Collections.Generic;
using System.IO;
using System.CodeDom.Compiler;
using System.Xml.Linq;
using System.Xml.XPath;
using PluralRule.CldrParser.Parser;
using PluralRules;
using PluralRules.Types;
using PluralRules = PluralRule.CldrParser.generator.PluralRules;

namespace PluralRule.CldrParser
{
    class Program
    {
        static void Main(string[] args)
        {
            // var cardinalPath = Path.Combine(CurrDir("plurals.xml").ToArray());
            // var ordinalPath = Path.Combine(CurrDir("ordinals.xml").ToArray());
            // var rulesRaw = new PluralRulesRaw();
            // using (var ordinalFileStream = File.OpenRead(ordinalPath))
            // using (var cardinalFileStream = File.OpenRead(cardinalPath))
            // {
            //     var ordinals = XDocument.Load(ordinalFileStream)
            //         .XPathSelectElements("//supplementalData/plurals/*");
            //     var cardinals = XDocument.Load(cardinalFileStream)
            //         .XPathSelectElements("//supplementalData/plurals/*");
            //     rulesRaw.OrdinalRules = XmlConverter.convert(ordinals);
            //    rulesRaw.CardinalRules = XmlConverter.convert(cardinals);
            // }
            //
            // var generator = new generator.PluralRules();
            // File.WriteAllText("test.cs", generator.TransformText());
            //
            // Console.WriteLine(rulesRaw.OrdinalRules.Count);
            // Console.WriteLine(rulesRaw.CardinalRules.Count);
            var generator = new generator.PluralRules();
            
        }


        private static List<string> CurrDir(string filename)
        {
            var currDir = new List<string>(
                Directory.GetCurrentDirectory()
                    .Split(Path.DirectorySeparatorChar)[new Range(0, Index.FromEnd(3))]
            );
            currDir.Add("cldr_data");
            currDir.Add(filename);
            return currDir;
        }
    }
}