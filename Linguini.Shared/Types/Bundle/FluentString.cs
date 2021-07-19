using System;
using System.Diagnostics.CodeAnalysis;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a string value
    /// </summary>
    public record FluentString : IFluentType
    {
        private readonly string _content;

        private FluentString(string content)
        {
            _content = content;
        }

        public FluentString(ReadOnlySpan<char> content)
        {
            _content = content.ToString();
        }


        string IFluentType.AsString()
        {
            return _content;
        }

        public static implicit operator string(FluentString fs) => fs._content;
        public static implicit operator FluentString(string s) => new(s);
        public static implicit operator FluentString(ReadOnlySpan<char> s) => new(new string(s));

        public IFluentType Copy()
        {
            return new FluentString(_content);
        }

        public override int GetHashCode()
        {
            return _content.GetHashCode();
        }

        public bool TryGetPluralCategory([NotNullWhen(true)] out PluralCategory? category)
        {
            return _content.TryPluralCategory(out category);
        }
    }
}
