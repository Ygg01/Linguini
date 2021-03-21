using System;

namespace Linguini.Bundle.Types
{
    public class FluentString : IFluentType
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
    }
}
