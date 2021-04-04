using System;

namespace Linguini.Bundle.Types
{
    public class FluentCustom: IFluentType, IEquatable<FluentCustom>
    {
        public IFluentType Type;
            
        public object Clone()
        {
            return Type.Clone();
        }

        public string AsString()
        {
            return Type.AsString();
        }

        public bool Equals(FluentCustom other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FluentCustom) obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }
    }
}