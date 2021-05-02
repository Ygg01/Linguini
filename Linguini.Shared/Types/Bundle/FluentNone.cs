using System;

namespace Linguini.Shared.Types.Bundle
{
    public class FluentNone: IFluentType, IEquatable<FluentNone>
    {
        private readonly string _desc;

        public static FluentNone None = new();

        public FluentNone()
        {
            _desc = "{???}";
        }
        
        public FluentNone(string desc)
        {
            _desc = desc;
        }

        public object Clone()
        {
            return new FluentNone(_desc);
        }

        public string AsString()
        {
            return "{???}";
        }

        public override string ToString()
        {
            return AsString();
        }

        public bool Equals(FluentNone? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return true;
        }
    }
}