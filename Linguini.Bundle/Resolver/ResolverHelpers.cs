using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Shared.Util;
using Linguini.Syntax.Ast;
using PluralRulesGenerated;

namespace Linguini.Bundle.Resolver
{
    /// <summary>
    ///     Provides helper methods to resolve fluent language elements into corresponding values
    ///     or textual representations based on the defined patterns, scopes, or expressions.
    /// </summary>
    public static class ResolverHelpers
    {
        /// Resolves the provided Pattern instance into an IFluentType representation using the given Scope.
        /// <param name="self">The Pattern instance to be resolved.</param>
        /// <param name="scope">The Scope containing context information for resolving the Pattern.</param>
        /// <return>An IFluentType instance representing the resolved Pattern.</return>
        public static IFluentType Resolve(this Pattern self, Scope scope)
        {
            var len = self.Elements.Count;

            if (len == 1)
                if (self.Elements[0] is TextLiteral textLiteral)
                    return GetFluentString(textLiteral.ToString(), scope.TransformFunc);

            var stringWriter = new StringWriter();
            self.Write(stringWriter, scope);
            return (FluentString)stringWriter.ToString();
        }

        /// Resolves the current IInlineExpression instance into an
        /// <see cref="IFluentType" />
        /// representation using the given Scope and optional position.
        /// <param name="self">The IInlineExpression instance to resolve.</param>
        /// <param name="scope">The Scope containing context and arguments for the resolution process.</param>
        /// <param name="pos">An optional positional argument index to consider during the resolution.</param>
        /// <return>An IFluentType instance representing the resolved IInlineExpression, or an error type if resolution fails.</return>
        public static IFluentType Resolve(this IInlineExpression self, Scope scope, int? pos = null)
        {
            if (self is TextLiteral textLiteral)
            {
                StringWriter stringWriter = new();
                UnicodeUtil.WriteUnescapedUnicode(textLiteral.Value, stringWriter);
                return (FluentString)stringWriter.ToString();
            }

            if (self is NumberLiteral numberLiteral) return FluentNumber.TryNumber(numberLiteral.Value.Span);

            if (self is VariableReference varRef)
            {
                var args = scope.LocalNameArgs ?? scope.Args;
                if (args != null
                    && args.TryGetValue(varRef.Id.ToString(), out var arg))
                    return arg.Copy();

                if (scope.LocalPosArgs != null && pos != null
                                               && pos < scope.LocalPosArgs.Count)
                    return scope.LocalPosArgs[pos.Value];

                if (scope.LocalNameArgs == null) scope.AddError(ResolverFluentError.UnknownVariable(varRef));

                return new FluentErrType();
            }

            if (self is FunctionReference funcRef)
            {
                var (resolvedPosArgs, resolvedNamedArgs) = scope.GetArguments(funcRef.Arguments);

                if (scope.Bundle.TryGetFunction(funcRef.Id, out var func))
                {
                    return func.Function(resolvedPosArgs, resolvedNamedArgs);
                }

                scope.AddError(ResolverFluentError.Reference(funcRef));
                return new FluentErrType();
            }

            var writer = new StringWriter();
            self.TryWrite(writer, scope);
            return (FluentString)writer.ToString();
        }

        private static FluentString GetFluentString(string str, Func<string, string>? transformFunc)
        {
            return transformFunc == null ? str : transformFunc(str);
        }

        /// <summary>
        /// Provides methods to handle pluralization rules for different languages and cultures.
        /// This includes determining the plural category of a number based on the given culture
        /// and rule type (e.g., cardinal or ordinal), as well as handling special cases for specific locales.
        /// </summary>
        public static class PluralRules
        {
            /// <summary>Determines the plural category of a given number based on the specified culture and rule type.</summary>
            /// <param name="info">The CultureInfo representing the cultural context to determine the plural category.</param>
            /// <param name="ruleType">The type of pluralization rule (e.g., Cardinal or Ordinal) to apply.</param>
            /// <param name="number">The FluentNumber to evaluate for determining the plural category.</param>
            /// <return>A PluralCategory enumerating the plural classification of the provided number.</return>
            public static PluralCategory GetPluralCategory(CultureInfo info, RuleType ruleType, FluentNumber number)
            {
                var specialCase = IsSpecialCase(info.Name, ruleType);
                var langStr = GetPluralRuleLang(info, specialCase);
                var func = RuleTable.GetPluralFunc(langStr, ruleType);
                if (number.TryPluralOperands(out var op)) return func(op);

                return PluralCategory.Other;
            }

            /// <summary>
            /// Special language identifier, that goes over 4 ISO language code.
            /// </summary>
            /// <param name="info">language code</param>
            /// <param name="ruleType">Is it ordinal or cardinal rule type.</param>
            /// <returns><c>true</c> when its not standard language code; <c>false</c> otherwise.</returns>
            public static bool IsSpecialCase(string info, RuleType ruleType)
            {
                if (info.Length < 4)
                    return false;

                var specialCaseTable = ruleType switch
                {
                    RuleType.Ordinal => RuleTable.SpecialCaseOrdinal,
                    _ => RuleTable.SpecialCaseCardinal
                };
                return specialCaseTable.Contains(info);
            }

            private static string GetPluralRuleLang(CultureInfo info, bool specialCase)
            {
                if (CultureInfo.InvariantCulture.Equals(info))
                    // When culture info is uncertain we default to common 
                    // language behavior
                    return "root";

                var langStr = specialCase
                    ? info.Name.Replace('-', '_')
                    : info.TwoLetterISOLanguageName;
                return langStr;
            }
        }
    }
}