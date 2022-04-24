namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent value representing lack of value
    /// similar to <c>Nullable</c> or <c>Option</c> monad. 
    /// </summary>
    public class FluentNone : IFluentType
    {
        /// <summary>
        /// Static value representing empty value.
        /// </summary>
        public static readonly FluentNone None = new FluentNone();
        
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
    }
}
