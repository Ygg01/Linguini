namespace Linguini.Bundle.Types
{
    public class FluentString : IFluentType
    {
        private string _content;

        public FluentString(string content)
        {
            _content = content;
        }
        
        string IFluentType.AsString()
        {
            return _content;
        }

        public static implicit operator string(FluentString fs) => fs._content;
        public static implicit operator FluentString(string s) => new(s);
    }
}
