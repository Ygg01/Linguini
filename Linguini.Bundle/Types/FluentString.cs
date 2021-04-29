using System;
using System.Diagnostics.CodeAnalysis;
using Linguini.Shared.Types;

namespace Linguini.Bundle.Types
{
    public class FluentString : IFluentType, IEquatable<FluentString>
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

        public object Clone()
        {
            return new FluentString(_content);
        }

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
            return Equals((FluentString) obj);
        }

        public override int GetHashCode()
        {
            return _content.GetHashCode();
        }

        public bool TryGetPluralCategory([NotNullWhen(true)] out PluralCategory? category)
        {
            return PluralCategoryHelper.TryFromString(_content, out category);
        }
    }
}
