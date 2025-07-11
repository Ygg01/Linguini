using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Types
{
    /// <summary>
    /// Method for adding helper methods to <see cref="AstMessage"/> and <see cref="IFluentType"/>.
    /// </summary>
    public static class TypeHelpers
    {
        /// <summary>
        /// Retrieves an attribute with the specified name from the message.
        /// </summary>
        /// <param name="message">The <see cref="AstMessage"/> instance containing a collection of attributes.</param>
        /// <param name="attrName">The name of the attribute to retrieve.</param>
        /// <returns>An <see cref="Attribute"/> instance if found, otherwise null.</returns>
        public static Attribute? GetAttribute(this AstMessage message, string attrName)
        {
            return message.Attributes
                .Find(attribute => attribute.Id.ToString() == attrName);
        }

        /// <summary>
        /// Attempts to cast the specified <see cref="IFluentType"/> to a <see cref="FluentNumber"/>.
        /// </summary>
        /// <param name="fluentType">The <see cref="IFluentType"/> instance to cast.</param>
        /// <returns>A <see cref="FluentNumber"/> instance if the cast is successful, otherwise null.</returns>
        public static FluentNumber? ToFluentNumber(this IFluentType fluentType)
        {
            if (fluentType is FluentNumber type)
            {
                return type;
            }

            return null;
        }
    }
}