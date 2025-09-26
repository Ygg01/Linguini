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

            // A single value can be a number or a string, but in a complex pattern it will be stringified.
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
            switch (self)
            {
                case TextLiteral textLiteral:
                {
                    StringWriter stringWriter = new();
                    UnicodeUtil.WriteUnescapedUnicode(textLiteral.Value, stringWriter);
                    return (FluentString)stringWriter.ToString();
                }
                case NumberLiteral numberLiteral:
                    return FluentNumber.TryNumber(numberLiteral.Value.Span);
                case VariableReference varRef:
                    return varRef.ResolveRef(scope, pos);
                case FunctionReference funcRef:
                    return funcRef.ResolveRef(scope);
                case TermReference termRef:
                    return termRef.ResolveRef(scope, pos);
                default:
                {
                    var writer = new StringWriter();
                    self.TryWrite(writer, scope);
                    return (FluentString)writer.ToString();
                }
            }
        }

        private static IFluentType ResolveRef(this IPatternElement self, Scope scope, int? pos = null)
        {
            switch (self)
            {
                case Placeable placeable:
                    return placeable.ResolvePlace(scope, pos);
                case TextLiteral textLiteral:
                    return textLiteral.ResolveRef(scope);
                default:
                    throw new ArgumentException("Unexpected expression!");
            }
        }

        private static IFluentType ResolvePlace(this Placeable self, Scope scope, int? pos = null)
        {
            switch (self.Expression)
            {
                case SelectExpression selectExpression:
                    return selectExpression.ResolveRef(scope, pos);
                case IInlineExpression inlineExpression:
                    return inlineExpression.Resolve(scope, pos);
                default:
                    throw new NotImplementedException();
            }
        }

        private static IFluentType ResolveRef(this FunctionReference funcRef, Scope scope)
        {
            var (resolvedPosArgs, resolvedNamedArgs) = scope.GetArguments(funcRef.Arguments);

            if (scope.Bundle.TryGetFunction(funcRef.Id, out var func))
                return func.Function(resolvedPosArgs, resolvedNamedArgs);

            scope.AddError(ResolverFluentError.Reference(funcRef));
            return new FluentErrType();
        }

        
        private static IFluentType ResolveRef(this SelectExpression selectExpression, Scope scope, int? pos)
        {
            var selector = selectExpression.Selector.Resolve(scope, pos);
            var variant = GetVariant(selectExpression, scope, selector);
            return variant != null 
                ? variant.Value.Resolve(scope) 
                : new FluentErrType();
        }

        public static Variant? GetVariant(SelectExpression selectExpression, Scope scope, IFluentType selector)
        {
            Variant? retVal = null;
            if (selector is FluentString or FluentNumber)
            {
                foreach (var variant in selectExpression.Variants)
                {
                    // If we have a default and no match set default.
                    if (retVal == null && variant.IsDefault)
                    {
                        retVal = variant;
                    }
                    IFluentType key;
                    switch (variant.Type)
                    {
                        case VariantType.NumberLiteral:
                            key = FluentNumber.TryNumber(variant.Key.Span);
                            break;
                        default:
                            key = new FluentString(variant.Key.Span);
                            break;
                    }

                    if (key.Matches(selector, scope))
                    {
                        return variant;
                    }
                }
            }

            return retVal;
        }

        private static IFluentType ResolveRef(this TermReference termRef, Scope scope, int? pos = null)
        {
            var (posArgs, resolveArgs) = scope.GetArguments(termRef.Arguments);

            if (!scope.Bundle.EnableExtensions || !scope.Bundle.TryGetAstTerm(termRef.Id.ToString(), out var term))
                return new FluentErrType();

            if (termRef.Attribute == null) 
                return term.Value.Resolve(scope);
            
            foreach (var arg in term.Attributes)
                if (termRef.Attribute.Equals(arg.Id) && arg.Value.Elements.Count == 1)
                    return arg.Value.Elements[0].ResolveRef(scope, pos);

            return new FluentErrType();

        }

        private static IFluentType ResolveRef(this VariableReference varRef, Scope scope, int? pos = null)
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


        private static FluentString GetFluentString(string str, Func<string, string>? transformFunc)
        {
            return transformFunc == null ? str : transformFunc(str);
        }

        /// <summary>
        ///     Provides methods to handle pluralization rules for different languages and cultures.
        ///     This includes determining the plural category of a number based on the given culture
        ///     and rule type (e.g., cardinal or ordinal), as well as handling special cases for specific locales.
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
            ///     Special language identifier, that goes over 4 ISO language code.
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