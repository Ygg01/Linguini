using System;
using System.Globalization;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Shared;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using PluralRulesGenerated;

namespace Linguini.Bundle.Resolver
{
    public static class ResolverHelpers
    {
        public static IFluentType Resolve(this Pattern self, Scope scope)
        {
            var len = self.Elements.Count;

            if (len == 1)
            {
                if (self.Elements[0].TryConvert(out TextLiteral? textLiteral))
                {
                    return GetFluentString(textLiteral.ToString(), scope.Bundle.TransformFunc);
                }
            }

            var stringWriter = new StringWriter();
            self.Write(stringWriter, scope);
            return (FluentString) stringWriter.ToString();
        }

        public static IFluentType Resolve(this IInlineExpression self, Scope scope)
        {
            if (self.TryConvert(out TextLiteral? textLiteral))
            {
                return (FluentString) textLiteral.Value.Span;
            }

            if (self.TryConvert(out NumberLiteral? numberLiteral))
            {
                return FluentNumber.TryNumber(numberLiteral.Value.Span);
            }

            if (self.TryConvert(out VariableReference? varRef))
            {
                var args = scope.LocalArgs ?? scope.Args;
                if (args != null
                    && args.TryGetValue(varRef.Id.ToString(), out var arg))
                {
                    return (IFluentType) arg.Clone();
                }

                if (scope.LocalArgs == null)
                {
                    scope.AddError(ResolverFluentError.UnknownVariable(varRef));
                }

                return new FluentErrType();
            }

            var writer = new StringWriter();
            self.TryWrite(writer, scope);
            return (FluentString) writer.ToString();
        }

        private static FluentString GetFluentString(string str, Func<string, string>? transformFunc)
        {
            return transformFunc == null ? str : transformFunc(str);
        }
        
        public static class PluralRules
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
}