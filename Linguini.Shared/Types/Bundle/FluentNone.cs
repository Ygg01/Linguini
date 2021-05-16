using System;

namespace Linguini.Shared.Types.Bundle
{
    public class FluentNone : IFluentType, IEquatable<FluentNone>
    {
        public static readonly FluentNone None = new();

        private FluentNone()
        {
        }

        public object Clone()
        {
            return None;
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
