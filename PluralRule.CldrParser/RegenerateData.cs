using System;
using System.Collections.Generic;
using System.IO;
using PluralRule.CldrParser.Parser;
using PluralRules.Types;

namespace PluralRule.CldrParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var cardinalPath = Path.Combine(CurrDir("cldr_pluralrules_cardinals.json").ToArray());
            var ordinalPath = Path.Combine(CurrDir("cldr_pluralrules_ordinals.json").ToArray());
            var rulesRaw = new PluralRulesRaw();
            using (File.OpenRead(ordinalPath))
            using (File.OpenRead(cardinalPath))
            {
                rulesRaw.CardinalRules = JsonParser.GetElements(
                    File.OpenRead(cardinalPath),
                    RuleType.Cardinal
                );
                rulesRaw.OrdinalRules = JsonParser.GetElements(
                    File.OpenRead(ordinalPath),
                    RuleType.Ordinal
                );
            }

            Console.WriteLine(rulesRaw.OrdinalRules.Count);
            Console.WriteLine(rulesRaw.CardinalRules.Count);
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