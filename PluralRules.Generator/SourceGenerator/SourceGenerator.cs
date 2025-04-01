using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PluralRules.Generator.Cldr;

namespace PluralRules.Generator.SourceGenerator;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var xmlFiles = initContext.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".xml"));

        var namesAndContent = xmlFiles
            .Select((text, token) => (path: Path.GetFileName(text.Path),
                content: text.GetText(token)!.ToString()))
            .Collect();

        initContext.RegisterSourceOutput(namesAndContent, (context, source) =>
        {
            List<CldrRule> cardinalRules = [];
            List<CldrRule> ordinalRules = [];
            var debug = "";
            foreach (var (path, content) in source)
            {
                if (path.EndsWith("plurals.xml"))
                {
                    cardinalRules = ProcessXmlFile(content);
                }
                else if (path.EndsWith("ordinals.xml"))
                {
                    ordinalRules = ProcessXmlFile(content);
                }
            }

            var (sourceBuilder, testBuilder) = GenerateCode(ordinalRules, cardinalRules, debug);
                
            context.AddSource("PluralRule.cs", SourceText.From(sourceBuilder, Encoding.UTF8));
            context.AddSource("PluralRuleTestTable.cs", SourceText.From(testBuilder, Encoding.UTF8));
        });
    }

    private (string, string) GenerateCode(List<CldrRule> ordinalRules, List<CldrRule> cardinalRules,
        string debug)
    {
          
        // begin creating the source we'll inject into the users compilation
        var sourceBuilder = new StringBuilder($@"
using System;
using Linguini.Shared.Util;
using Linguini.Shared.Types;
// ordinal {debug}
namespace PluralRulesGenerated
{{
    public static partial class RuleTable
    {{
        private static Func<PluralOperands, PluralCategory>[] cardinalMap = 
        {{");
        WriteRules(sourceBuilder, cardinalRules);
        sourceBuilder.Append($@"
        }};

        private static Func<PluralOperands, PluralCategory>[] ordinalMap = 
        {{");
        WriteRules(sourceBuilder, ordinalRules);
        sourceBuilder.Append($@"
        }};
        
        private static int GetCardinalIndex(string culture)
        {{");
        WriteRulesIndex(sourceBuilder, cardinalRules);
        sourceBuilder.Append($@"
        }}

        private static int GetOrdinalIndex(string culture)
        {{");
        WriteRulesIndex(sourceBuilder, ordinalRules);
        sourceBuilder.Append($@"
        }}

        public static Func<PluralOperands, PluralCategory> GetPluralFunc(string culture, RuleType type)
        {{
            switch (type)
            {{
                case RuleType.Cardinal:
                    return cardinalMap[GetCardinalIndex(culture)];
                case RuleType.Ordinal:
                    return ordinalMap[GetOrdinalIndex(culture)];
            }}

            return _ => PluralCategory.Other;
        }}

        public static string[] SpecialCaseCardinal = {{");
        WriteSpecialCase(sourceBuilder, cardinalRules);
        sourceBuilder.Append($@"
        }};

        public static string[] SpecialCaseOrdinal = {{");
        WriteSpecialCase(sourceBuilder, ordinalRules);
        sourceBuilder.Append(@"
        };
        
    }
}
");
        var testBuilder = new StringBuilder($@"
using System;
using Linguini.Shared.Types;

namespace PluralRulesGenerated.Test
{{
    public static partial class RuleTableTest
    {{
        public static readonly object?[][] CardinalTestData = 
        {{
");
        foreach (var cardinalRule in cardinalRules)
        {
            WriteRuleTest(testBuilder, cardinalRule, true);
        }

        testBuilder.Append($@"
        }};

        public static readonly object?[][] OrdinalTestData = 
        {{
");
        foreach (var ordinalRule in ordinalRules)
        {
            WriteRuleTest(testBuilder, ordinalRule, false);
        }

        testBuilder.Append($@"
        }};
    }}
}}");
        return (sourceBuilder.ToString(), testBuilder.ToString());
    }

    private void WriteSpecialCase(StringBuilder sourceBuilder, List<CldrRule> cldrRules)
    {
        List<string> specialCases = new();
        foreach (var cldr in cldrRules)
        {
            foreach (var langId in cldr.LangIds)
            {
                if (langId.Length > 4)
                {
                    var specCase = langId.Replace('_', '-');
                    specialCases.Add(langId);
                    specialCases.Add(specCase);
                }
            }
        }

        if (specialCases.Count > 0)
        {
            foreach (var specialCase in specialCases)
            {
                sourceBuilder.Append($"\n            \"{specialCase}\",");
            }
        }
    }

    private void WriteRuleTest(StringBuilder testBuilder, CldrRule rule, bool isCardinal)
    {
        string ruleType = isCardinal ? "RuleType.Cardinal" : "RuleType.Ordinal";
        foreach (var ruleLangId in rule.LangIds)
        {
            foreach (var ruleMap in rule.Rules)
            {
                if (ruleMap.Rule.Samples != null)
                {
                    string category = $"PluralCategory.{ruleMap.Category.FirstCharToUpper()}";
                    foreach (var integerSample in ruleMap.Rule.Samples.Value.IntegerSamples)
                    {
                        var upper = integerSample.Upper == null
                            ? "null"
                            : $"\"{integerSample.Upper.Value}\"";
                        testBuilder.Append($@"
            new object?[] {{""{ruleLangId}"", {ruleType}, false, ""{integerSample.Lower.Value}"", {upper}, {category}}},");
                    }

                    foreach (var decimalSample in ruleMap.Rule.Samples.Value.DecimalSamples)
                    {
                        var upper = decimalSample.Upper == null
                            ? "null"
                            : $"\"{decimalSample.Upper.Value}\"";
                        testBuilder.Append($@"
            new object?[] {{""{ruleLangId}"", {ruleType}, true, ""{decimalSample.Lower.Value}"", {upper}, {category}}},");
                    }
                }
            }
        }
    }

    private void WriteRules(StringBuilder stringBuilder, List<CldrRule> rules)
    {
        foreach (var rule in rules)
        {
            if (rule.Rules.Count == 1)
            {
                var category = rule.Rules[0].Category.FirstCharToUpper();
                stringBuilder.Append(@$"
            _ => PluralCategory.{category},");
            }
            else
            {
                WriteRuleMaps(stringBuilder, rule.Rules);
            }
        }
    }

    private void WriteRuleMaps(StringBuilder stringBuilder, List<RuleMap> ruleMaps)
    {
        stringBuilder.Append(@"
            po =>
            {");
        foreach (var ruleMap in ruleMaps)
        {
            WriteRuleMap(stringBuilder, ruleMap);
        }

        stringBuilder.Append(@"
            },");
    }

    private void WriteRuleMap(StringBuilder stringBuilder, RuleMap ruleMap)
    {
        stringBuilder.Append(@"
                ");
        if (ruleMap.Rule.Condition.IsAny())
        {
            stringBuilder.Append($"return PluralCategory.{ruleMap.Category.FirstCharToUpper()};");
        }
        else
        {
            stringBuilder.Append("if (");
            WriteCondition(stringBuilder, ruleMap.Rule.Condition);
            stringBuilder.Append(")");
            stringBuilder.Append($@"
                {{
                    return PluralCategory.{ruleMap.Category.FirstCharToUpper()};
                }}");
        }
    }

    private void WriteCondition(StringBuilder stringBuilder, Condition ruleCondition)
    {
        if (ruleCondition.Conditions.Count < 1)
        {
            stringBuilder.Append("true");
            return;
        }

        for (var i = 0; i < ruleCondition.Conditions.Count; i++)
        {
            if (i != 0)
            {
                stringBuilder.Append(" || ");
            }

            var andCondition = ruleCondition.Conditions[i]!;
            if (andCondition.Relations.Count == 1)
            {
                WriteRelation(stringBuilder, andCondition.Relations[0]);
            }
            else
            {
                for (var j = 0; j < andCondition.Relations.Count; j++)
                {
                    if (j != 0)
                    {
                        stringBuilder.Append(" && ");
                    }

                    WriteRelation(stringBuilder, andCondition.Relations[j]);
                }
            }
        }
    }

    private void WriteRelation(StringBuilder stringBuilder, Relation relation)
    {
        var operand = relation.Expr.Operand.ToUpperChar();
        if (operand == "E")
        {
            operand = "Exp()";
        }

        var brackets = relation.RangeListItems.Count > 1;
        if (relation.Expr.Modulus != null)
        {
            var sb = new StringBuilder();

            sb.Append("po.").Append(operand).Append(" % ")
                .Append(relation.Expr.Modulus.Value);
            if (relation.Op.IsNegated())
            {
                stringBuilder.Append('!');
                if (relation.RangeListItems[0] is not RangeElem)
                {
                    // Have to bracket negation of plain mod
                    // e.g. instead of !po.N % 10 == 0
                    //                 !(po.N % 10 == 0)
                    brackets = true;
                }
            }

            if (brackets)
            {
                stringBuilder.Append("(");
            }

            for (var i = 0; i < relation.RangeListItems.Count; i++)
            {
                if (i != 0)
                {
                    stringBuilder.AppendLine().Append("                    || ");
                }

                var rel = relation.RangeListItems[i]!;
                if (rel is DecimalValue value)
                {
                    stringBuilder.Append(sb).Append(" == ").Append(value);
                }
                else if (rel is RangeElem rangeElem)
                {
                    stringBuilder.Append($"({sb}).InRange({rangeElem.LowerVal}, {rangeElem.UpperVal})");
                }
            }

            if (brackets)
            {
                stringBuilder.Append(") ");
            }
        }
        else
        {
            var sb = new StringBuilder();
            sb.Append("po.").Append(operand);

            string op = relation.Op.IsNegated() ? " != " : " == ";

            if (brackets)
            {
                stringBuilder.Append("( ");
            }

            for (var i = 0; i < relation.RangeListItems.Count; i++)
            {
                if (i != 0)
                {
                    stringBuilder.Append(" || ").AppendLine().Append("                    ");
                }

                var rel = relation.RangeListItems[i]!;
                if (rel is DecimalValue value)
                {
                    stringBuilder.Append(sb).Append(op).Append(value);
                }
                else if (rel is RangeElem rangeElem)
                {
                    var negate = relation.Op.IsNegated() ? "!" : "";
                    stringBuilder.Append($"{negate}{sb}.InRange({rangeElem.LowerVal}, {rangeElem.UpperVal}) ");
                }
            }

            if (brackets)
            {
                stringBuilder.Append(" )");
            }
        }
    }

    private void WriteRulesIndex(StringBuilder stringBuilder, List<CldrRule> rules)
    {
        stringBuilder.Append(@"
            switch (culture)
            {");
        for (int i = 0; i < rules.Count; i++)
        {
            WriteRuleIndex(stringBuilder, rules[i], i);
        }

        stringBuilder.Append(@"
            }
            return -1;");
    }

    private void WriteRuleIndex(StringBuilder stringBuilder, CldrRule rule, int i)
    {
        foreach (var langId in rule.LangIds)
        {
            stringBuilder.Append($@"
                case ""{langId}"":");
        }

        stringBuilder.Append($@"
                    return {i};");
    }

    private static List<CldrRule> ProcessXmlFile(string text)
    {
        var xmlElems = XDocument.Load(new StringReader(text))
            .XPathSelectElements("//supplementalData/plurals/*");
        var rules = Convert(xmlElems);

        return rules;
    }

    private static List<CldrRule> Convert(IEnumerable<XElement> plurals)
    {
        var retVal = new List<CldrRule>(40);
        foreach (var pluralRule in plurals)
        {
            var langIds = pluralRule.Attribute("locales")!
                .Value.Split(' ')
                .ToList();
            var rules = new List<RuleMap>();
            foreach (var element in pluralRule.Elements())
            {
                var countTag = element.Attribute("count")?.Value;

                var rule = new CldrParser(element.FirstNode.ToString()).ParseRule();
                rules.Add(new RuleMap(countTag!, rule));
            }

            retVal.Add(new CldrRule(langIds, rules));
        }

        return retVal;
    }
}