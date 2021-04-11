using System;

namespace Linguini.Bundle.Types
{
    public class FluentNone: IFluentType, IEquatable<FluentNone>
    {
        private string Desc;


        public FluentNone()
        {
            Desc = "{???}";
        }
        
        public FluentNone(string desc)
        {
            Desc = desc;
        }

        public object Clone()
        {
            return new FluentNone(Desc);
        }

        public string AsString()
        {
            return "{???}";
        }

        public bool Equals(FluentNone? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return true;
        }
    }
}