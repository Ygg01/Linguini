using System;

namespace Linguini.Bundle.Types
{
    public class FluentCustom: IFluentType, IEquatable<FluentCustom>
    {
        public IFluentType Value;
            
        public object Clone()
        {
            return Value.Clone();
        }

        public string AsString()
        {
            return Value.AsString();
        }

        public bool Equals(FluentCustom? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FluentCustom) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}