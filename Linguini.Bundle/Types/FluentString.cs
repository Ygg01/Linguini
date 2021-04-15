using System;
using System.Diagnostics.CodeAnalysis;
using PluralRules.Types;


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
            return (_content != null ? _content.GetHashCode() : 0);
        }

        public bool TryGetPluralCategory([NotNullWhen(true)]out PluralCategory? category)
        {
            switch (_content)
            {
                case "zero":
                    category = PluralCategory.Zero;
                    break;
                case "one":
                    category = PluralCategory.One;
                    break;
                case "two":
                    category = PluralCategory.Two;
                    break;
                case "few":
                    category = PluralCategory.Few;
                    break;
                case "many":
                    category = PluralCategory.Many;
                    break;
                case "other":
                    category = PluralCategory.Other;
                    break;
                default:
                    category = null;
                    return false;
            }
            
            return true;
        }
    }
}