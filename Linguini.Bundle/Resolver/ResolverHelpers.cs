using System;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

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
    }
}