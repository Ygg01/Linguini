using System;
using System.Diagnostics.CodeAnalysis;

namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a string value
    /// </summary>
    public class FluentString : IFluentType, IEquatable<FluentString>
    {
        public bool Equals(FluentString? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _content == other._content;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FluentString)obj);
        }

        public static bool operator ==(FluentString? left, FluentString? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FluentString? left, FluentString? right)
        {
            return !Equals(left, right);
        }

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
        public static implicit operator FluentString(string s) => new FluentString(s);
        public static implicit operator FluentString(ReadOnlySpan<char> s) => new FluentString(new string(s));

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
