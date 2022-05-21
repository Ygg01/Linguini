namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent value representing lack of value
    /// similar to <c>Nullable</c> or <c>Option</c> monad. 
    /// </summary>
    public record FluentNone : IFluentType
    {
        /// <summary>
        /// Static value representing empty value.
        /// </summary>
        public static readonly FluentNone None = new();
        
        private FluentNone()
        {
        }

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return None;
        }

        /// <inheritdoc/>
        public string AsString()
        {
            return "{???}";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return AsString();
        }

        /// <inheritdoc/>
        public bool IsError()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool Matches(IFluentType other, IScope scope)
        {
            return false;
        }
    }
}
