using System;
using System.Diagnostics.CodeAnalysis;
using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a string value. A thin wrapper around the value.
    /// </summary>
    public record FluentString : IFluentType
    {
        private readonly string _content;

        /// <summary>
        /// Constructs a value from string.
        /// </summary>
        /// <param name="content">string being wrapped</param>
        private FluentString(string content)
        {
            _content = content;
        }

        /// <summary>
        /// Constructs a value from <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="content">string being wrapped</param>
        public FluentString(ReadOnlySpan<char> content)
        {
            _content = content.ToString();
        }

        /// <inheritdoc/>
        string IFluentType.AsString()
        {
            return _content;
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

        /// <summary>
        /// Converts a <see cref="FluentString"/> to a <see cref="string"/>
        /// </summary>
        /// <param name="fs">value </param>
        /// <returns>the string being wrapped</returns>
        public static implicit operator string(FluentString fs) => fs._content;
        
        /// <summary>
        /// Converts a <see cref="string"/> to a <see cref="FluentString"/>
        /// </summary>
        /// <param name="s">value to be wrapped in <see cref="FluentString"/></param>
        /// <returns>the wrapper around FluentString</returns>
        public static implicit operator FluentString(string s) => new(s);
        
        /// <summary>
        /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="FluentString"/>
        /// </summary>
        /// <param name="s">value to be wrapped in <see cref="FluentString"/></param>
        /// <returns>the wrapper around FluentString</returns>
        public static implicit operator FluentString(ReadOnlySpan<char> s) => new(s.ToString());

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return new FluentString(_content);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _content.GetHashCode();
        }

        /// <summary>
        /// Helper methods to extract PluralCategory. <seealso cref="PluralCategoryHelper.TryPluralCategory"/>
        /// </summary>
        /// <param name="category">Case-insensitive name of the Plural category</param>
        /// <returns><c>true</c> if it matches the <c>pluralCategory</c> value</returns>
        public bool TryGetPluralCategory([NotNullWhen(true)] out PluralCategory? category)
        {
            return _content.TryPluralCategory(out category);
        }
    }
}
