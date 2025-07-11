using System.Collections.Generic;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Types
{
    using FluentArgs = IDictionary<string, IFluentType>;

    /// <summary>
    /// Represents a delegate for defining external functions to be used within Fluent localization.
    /// </summary>
    /// <param name="positionalArgs">
    /// A list of positional arguments passed to the external function.
    /// </param>
    /// <param name="namedArgs">
    /// A dictionary containing named arguments, where the key is the name of the argument and the value is of type <see cref="IFluentType"/>.
    /// </param>
    /// <returns>
    /// An instance of <see cref="IFluentType"/> representing the result of the external function execution.
    /// </returns>
    public delegate IFluentType ExternalFunction(
        IList<IFluentType> positionalArgs,
        FluentArgs namedArgs);
}
