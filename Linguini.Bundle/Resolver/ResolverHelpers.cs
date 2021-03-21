using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public static class ResolverHelpers
    {
        private static readonly Func<string, string> identityFunc = s => s;

        public static void Write(this Pattern pattern, TextWriter writer, Scope scope,
            out IList<FluentError> errors)
        {
            errors = new List<FluentError>();
            throw new NotImplementedException();
            
            
        }

        public static FluentString Resolve(this Pattern pattern, Scope scope)
        {
            var len = pattern.Elements.Count;

            if (len == 1)
            {
                if (pattern.Elements[0].TryConvert(out TextLiteral textLiteral))
                {
                    var transformFunc = scope.Bundle.TransformFunc ?? identityFunc;
                    return transformFunc(textLiteral.ToString());
                }
            }

            StringWriter stringWriter = new StringWriter();
            Write(pattern, stringWriter,scope, out _);
            return stringWriter.ToString();
        }
    }
}
