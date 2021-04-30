using System.Collections.Generic;
using System.Text;
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
            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder($@"
using System;
namespace PluralRulesGenerated
{{
    public static class RuleTable
    {{
        public static void SayHello3() 
        {{
            Console.WriteLine(""Hello from generated {ordinalRules.Count} code!!"");
            Console.WriteLine(""Hello from generated {cardinalRules.Count} code!!"");
        }}
    }}
}}
");
            context.AddSource("helloWorldGenerated",  SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

    }
}
