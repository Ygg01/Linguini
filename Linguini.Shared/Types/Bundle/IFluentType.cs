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
            return (this, other) switch
            {
                (FluentString fs1, FluentString fs2) => fs1.Equals(fs2),
                (FluentNumber fn1, FluentNumber fn2) => fn1.Equals(fn2),
                (FluentString fs1, FluentNumber fn2) => MatchByPluralCategory(scope, fs1, fn2),
                _ => false,
            };
        }

        bool MatchByPluralCategory(IScope scope, FluentString fs1, FluentNumber fn2)
        {
            if (!fs1.TryGetPluralCategory(out var strCategory)) return false;
            var numCategory = scope.GetPluralRules(RuleType.Cardinal, fn2);

            return numCategory == strCategory;
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
