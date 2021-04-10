using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Types
{
    public static class TypeHelpers
    {
        public static Attribute? GetAttribute(this AstMessage message, string attrName)
        {
            return message.Attributes
                .Find(attribute => attribute.Id.ToString() == attrName);
        }
    }
}