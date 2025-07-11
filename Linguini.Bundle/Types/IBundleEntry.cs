namespace Linguini.Bundle.Types
{
    /// <summary>
    /// Represents a fluent function that acts as a wrapper for an external function,
    /// allowing seamless integration of external functional behaviors into a Fluent resource.
    /// A FluentFunction is implicitly convertible from an ExternalFunction for ease of use.
    /// </summary>
    public record FluentFunction(ExternalFunction Function)
    {
        /// <summary>
        /// Defines an implicit operator for a conversion between <see cref="ExternalFunction"/> to <see cref="FluentFunction"/>
        /// </summary>
        /// <param name="ef"><see cref="ExternalFunction"/> to be converted to <see cref="FluentFunction"/>.</param>
        /// <returns>The converted FluentFunction.</returns>
        public static implicit operator FluentFunction(ExternalFunction ef) => new(ef);
    }
}
