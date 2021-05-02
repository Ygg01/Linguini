using System;

namespace Linguini.Shared.Types.Bundle
{
    public interface IFluentType : ICloneable
    {
        string AsString();

        bool IsError()
        {
            return false;
        }


        bool Matches(IFluentType other, IScope scope)
        {
            if (this.TryConvert(out FluentString? s1)
                && other.TryConvert(out FluentString? s2))
            {
                return s1.Equals(s2);
            }

            if (this.TryConvert(out FluentNumber? n1)
                && other.TryConvert(out FluentNumber? n2))
            {
                return n1.Equals(n2);
            }

            if (this.TryConvert(out FluentString? fs1)
                && other.TryConvert(out FluentNumber? fn2))
            {
                if (fs1.TryGetPluralCategory(out var strCategory))
                {
                    var numCategory = scope
                        .GetPluralRules(RuleType.Cardinal, fn2);

                    return numCategory == strCategory;
                }

                return false;
            }

            return false;
        }
    }

    public interface IScope
    {
        PluralCategory GetPluralRules(RuleType type, FluentNumber number);
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

        public bool IsError()
        {
            return true;
        }
    }
}
