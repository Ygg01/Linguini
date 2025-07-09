using Linguini.Shared.Util;

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

        /// <summary>
        /// Method that for a given scope and 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
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

        /// <summary>
        /// Determines if type is an error. Fluent None isn't an error.
        /// </summary>
        /// <returns>false</returns>
        public bool IsError()
        {
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return AsString();
        }
    }
}
