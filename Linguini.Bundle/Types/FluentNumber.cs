using System;

#nullable enable
namespace Linguini.Bundle.Types
{
    public class FluentNumber : IFluentType
    {
        public readonly double Value;
        public FluentNumberOptions Options;

        public FluentNumber(double value, FluentNumberOptions options)
        {
            Value = value;
            Options = options;
        }
        public string AsString()
        {
            // TODO
            throw new NotImplementedException();
        }

        
        public static FluentNumber FromString(ReadOnlySpan<char> input)
        {
            var parsed = Double.Parse(input);
            var options = new FluentNumberOptions();
            options.MaximumFractionDigits = input.Length  - input.IndexOf('.') - 1;
            return new FluentNumber(parsed, options);
        }
        public static FluentNumber FromString(string input)
        {
            var parsed = Double.Parse(input);
            var options = new FluentNumberOptions();
            options.MaximumFractionDigits = input.Length  - input.IndexOf('.') - 1;
            return new FluentNumber(parsed, options);
        }

        public static IFluentType TryNumber(ReadOnlySpan<char> valueSpan)
        {
            try
            {
                return FromString(valueSpan);
            }
            catch (Exception _)
            {
                return new FluentString(valueSpan);
            }
        }

        public object Clone()
        {
            return new FluentNumber(Value, Options);
        }
    }

    public record FluentNumberOptions
    {
        public FluentNumberStyle Style;
        public string? Currency;
        public FluentNumberCurrencyDisplayStyle CurrencyDisplayStyle;
        public bool UseGrouping;
        public int? MinimumIntegerDigits;
        public int? MinimumFractionDigits;
        public int? MaximumFractionDigits;
        public int? MinimumSignificantDigits;
        public int? MaximumSignificantDigits;

        public FluentNumberOptions()
        {
            Style = FluentNumberStyle.Decimal;
            Currency = null;
            CurrencyDisplayStyle = FluentNumberCurrencyDisplayStyle.Symbol;
            UseGrouping = true;
            MinimumIntegerDigits = null;
            MinimumFractionDigits = null;
            MaximumFractionDigits = null;
            MinimumSignificantDigits = null;
            MaximumSignificantDigits = null;
        }
    }

    public enum FluentNumberStyle
    {
        Decimal,
        Currency,
        Percent,
    }
    
    public enum FluentNumberCurrencyDisplayStyle {
        Symbol,
        Code,
        Name,
    }
}
