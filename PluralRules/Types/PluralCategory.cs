using System;
using System.Diagnostics.CodeAnalysis;

namespace PluralRules.Types
{
    public enum PluralCategory : byte
    {
        Zero,
        One,
        Two,
        Few,
        Many,
        Other,
    }

    public static class PluralCategoryHelper
    {
        public static bool TryFromString(string? input, [NotNullWhen(true)]  out PluralCategory? pluralCategory)
        {
            if (input == null)
            {
                pluralCategory = null;
                return false;
            }

            switch (input.ToLower())
            {
                case "zero":
                    pluralCategory = PluralCategory.Zero;
                    return true;
                case "one":
                    pluralCategory = PluralCategory.One;
                    return true;
                case "two":
                    pluralCategory = PluralCategory.Two;
                    return true;
                case "few":
                    pluralCategory = PluralCategory.Few;
                    return true;
                case "many":
                    pluralCategory = PluralCategory.Many;
                    return true;
                case "other":
                    pluralCategory = PluralCategory.Other;
                    return true;
                default:
                    throw new AggregateException($"Unexpected PluralCategory `{input}`");
            }
        }
    }
}
