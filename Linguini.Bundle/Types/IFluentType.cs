using System;

namespace Linguini.Bundle.Types
{
    public interface IFluentType : ICloneable
    {
        string AsString();
    }

    public class FluentErrType : IFluentType
    {
        public object Clone()
        {
            return this;
        }

        public string AsString()
        {
            return "FluentErrType";
        }
    }
}
