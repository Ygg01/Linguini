using System;
using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a reference to another Term.
    /// </summary>
    public record FluentReference : IFluentType
    {
        private readonly string _reference = "";
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="reference">id of another Term in Fluent Bundle.</param>
        public FluentReference(string reference)
        {
            _reference = reference;
        }
        
        /// <summary>
        /// Convenience constructor. Shorthand for <see cref="FluentReference(string)"/>
        /// </summary>
        /// <param name="reference">id of another Term in Fluent Bundle.</param>
        public FluentReference(ReadOnlySpan<char> reference)
        {
            _reference = reference.ToString();
        }
        
        /// <inheritdoc/>
        public string AsString()
        {
            return _reference;
        }

        /// <inheritdoc/>
        public bool IsError()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
        }

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return new FluentReference(_reference);
        }
        
        /// <summary>
        /// Implicit conversion between <see cref="FluentReference"/> and string.
        /// </summary>
        /// <param name="fr">A <see cref="FluentReference"/> to convert</param>
        /// <returns>Id of the <see cref="FluentReference"/></returns>
        public static implicit operator string(FluentReference fr) => fr._reference;
        
        /// <summary>
        /// Implicit conversion from <see cref="string"/> into <see cref="FluentReference"/>.
        /// </summary>
        /// <param name="s">string reference of the term</param>
        /// <returns>A <see cref="FluentReference"/></returns>
        public static implicit operator FluentReference(string s) => new(s);
    }
}