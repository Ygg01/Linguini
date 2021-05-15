using System;
using System.Globalization;

#nullable enable
namespace Linguini.Shared.Types.Bundle
{
    public class FluentNumber : IFluentType, IEquatable<FluentNumber>
    {
        public readonly double Value;
        public readonly FluentNumberOptions Options;

        public FluentNumber(double value, FluentNumberOptions options)
        {
            Value = value;
            Options = options;
        }

        public string AsString()
        {
            var stringVal = Value.ToString(CultureInfo.InvariantCulture);
            if (Options.MinimumFractionDigits != null)
            {
                var minfd = Options.MinimumFractionDigits.Value;
                var pos = stringVal.IndexOf('.');
                if (pos != -1)
                {
                    var fracNum = stringVal.Length - pos - 1;
                    var missing = fracNum > minfd
                        ? 0
                        : minfd - fracNum;
                    var pattern = new String('0', missing);
                    stringVal = $"{stringVal}{pattern}";
                }
                else
                {
                    stringVal = $"{stringVal}.{new String('0', minfd)}";
                }
            }

            return stringVal;
        }

        public static FluentNumber FromString(ReadOnlySpan<char> input)
        {
            var parsed = Double.Parse(input);
            var options = new FluentNumberOptions();
            if (input.IndexOf('.') != -1)
            {
                options.MinimumFractionDigits = input.Length - input.IndexOf('.') - 1;
            }
            return new FluentNumber(parsed, options);
        }

        public static FluentNumber FromString(string input)
        {
            return FromString(input.AsSpan());
        }

        public static IFluentType TryNumber(ReadOnlySpan<char> valueSpan)
        {
            try
            {
                return FromString(valueSpan);
            }
            catch (Exception)
            {
                return new FluentString(valueSpan);
            }
        }

        public static implicit operator double(FluentNumber fs) => fs.Value;
        public static implicit operator FluentNumber(double db) => new(db, new FluentNumberOptions());
        public static implicit operator FluentNumber(float fl) => new(fl, new FluentNumberOptions());

        public object Clone()
        {
            return new FluentNumber(Value, Options);
        }

        public bool Equals(FluentNumber? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FluentNumber) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int AsInt()
        {
            return Convert.ToInt32(Value);
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

    public enum FluentNumberCurrencyDisplayStyle
    {
        Symbol,
        Code,
        Name,
    }
}
