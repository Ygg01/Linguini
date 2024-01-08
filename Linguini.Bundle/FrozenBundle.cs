using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle
{
    public class FrozenBundle : IReadBundle
    {
#if NET8_0_OR_GREATER
        
#else

#endif
        public bool HasMessage(string identifier)
        {
            throw new System.NotImplementedException();
        }

        public bool HasAttrMessage(string idWithAttr)
        {
            throw new System.NotImplementedException();
        }

        public string? GetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args = null)
        {
            throw new System.NotImplementedException();
        }

        public string? GetAttrMessage(string msgWithAttr, params (string, IFluentType)[] args)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetAttrMessage(string msgWithAttr, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetMessage(string id, IDictionary<string, IFluentType>? args, out IList<FluentError> errors,
            [NotNullWhen(true)] out string? message)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetMessage(string id, string? attribute, IDictionary<string, IFluentType>? args,
            [NotNullWhen(false)] out IList<FluentError>? errors, [NotNullWhen(true)] out string? message)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? message)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? function)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetFunction(string funcName, [NotNullWhen(true)] out FluentFunction? function)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetMessageEnumerable()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetFuncEnumerable()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetTermEnumerable()
        {
            throw new System.NotImplementedException();
        }
    }
}