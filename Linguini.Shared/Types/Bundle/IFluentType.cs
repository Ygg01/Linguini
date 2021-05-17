using Linguini.Shared.Util;

namespace Linguini.Shared.Types.Bundle
{
    public interface IFluentType
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

        IFluentType Copy();
    }

    public interface IScope
    {
        PluralCategory GetPluralRules(RuleType type, FluentNumber number);
    }

    public record FluentErrType : IFluentType
    {
        public IFluentType Copy()
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
