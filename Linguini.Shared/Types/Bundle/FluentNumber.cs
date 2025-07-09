using System;
using System.Globalization;
using Linguini.Shared.Util;

#nullable enable
namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Fluent representation of a number in double precision
    /// </summary>
    public record FluentNumber : IFluentType
    {
        /// <summary>
        /// Numerical value of fluent number, depicted using IEEE 754 64-bit floating number.
        /// </summary>
        public readonly double Value;
        private readonly FluentNumberOptions _options;

        private FluentNumber(double value, FluentNumberOptions options)
        {
            Value = value;
            _options = options;
        }

        /// <inheritdoc/>
        public string AsString()
        {
            var stringVal = Value.ToString(CultureInfo.InvariantCulture);
            if (_options.MinimumFractionDigits != null)
            {
                var minfd = _options.MinimumFractionDigits.Value;
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

        /// <inheritdoc/>
        public bool IsError()
        {
            return false;
        }
        
        /// <inheritdoc/>
        public bool Matches(IFluentType other, IScope scope)
        {
            return SharedUtil.Matches(this, other, scope);
        }
        

        /// <summary>
        /// Method that converts characters span into a <see cref="FluentNumber"/>
        /// </summary>
        /// <param name="input">Span of <see cref="char"/> that represents a number.</param>
        /// <exception cref="FormatException"><c>input</c> doesn't represent a number in a valid format.</exception>
        /// <returns>extracted <see cref="FluentNumber"/></returns>
        public static FluentNumber FromString(ReadOnlySpan<char> input)
        {
            var parsed = Double.Parse(input.ToString(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            var options = new FluentNumberOptions();
            if (input.IndexOf('.') != -1)
            {
                options.MinimumFractionDigits = input.Length - input.IndexOf('.') - 1;
            }
            return new FluentNumber(parsed, options);
        }

        /// <summary>
        /// Converts the string into a <see cref="FluentNumber"/>
        /// </summary>
        /// <param name="input">string being converted to number</param>
        /// <returns>valid <see cref="FluentNumber"/> or throws an exception</returns>
        public static FluentNumber FromString(string input)
        {
            return FromString(input.AsSpan());
        }

        /// <summary>
        /// Attempts to parse a number from the provided character span.
        /// If parsing fails, returns a new instance of <see cref="FluentString"/> containing the original input.
        /// </summary>
        /// <param name="valueSpan">The read-only span of characters representing the number to parse.</param>
        /// <returns>An instance of <see cref="FluentNumber"/> if parsing is successful; otherwise, a <see cref="FluentString"/> containing the unparsed input.</returns>
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

        /// <summary>
        /// Overloads an operator to convert a <see cref="FluentNumber"/> to <see cref="double"/>.
        /// </summary>
        public static implicit operator double(FluentNumber fs) => fs.Value;

        /// <summary>
        /// Overloads an operator to convert a <see cref="double"/> to <see cref="FluentNumber"/>.
        /// </summary>
        public static implicit operator FluentNumber(double db) => new(db, new FluentNumberOptions());
        /// <summary>
        /// Overloads an operator to convert a <see cref="float"/> to <see cref="FluentNumber"/>.
        /// </summary>
        public static implicit operator FluentNumber(float fl) => new(fl, new FluentNumberOptions());

        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return new FluentNumber(Value, _options);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// Number options as defined in <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/NumberFormat/NumberFormat">MDN Intl.NumberFormat()</see>
    /// </summary>
    public record FluentNumberOptions
    {
        /// <summary>
        /// Number style used. <seealso cref="FluentNumberStyle"/>
        /// </summary>
        public FluentNumberStyle Style;
        
        /// <summary>
        /// Currency string
        /// </summary>
        public string? Currency;
        
        /// <summary>
        /// Display style for currency. <seealso cref="FluentNumberCurrencyDisplayStyle"/>
        /// </summary>
        public FluentNumberCurrencyDisplayStyle CurrencyDisplayStyle;
        
        /// <summary>
        /// Whether to use digits grouping in number display.
        /// </summary>
        public bool UseGrouping;
        
        /// <summary>
        /// The minimum number of integer digits to use. A value with a smaller number of integer digits than this
        /// number will be left-padded with zeros (to the specified length) when formatted. Possible values are
        /// from <c>1</c> to <c>21</c>; the default is <c>1</c>.
        /// </summary>
        public int? MinimumIntegerDigits;
        /// <summary>
        /// The minimum number of fraction digits to use. Possible values are from <c>0</c> to <c>100</c>;
        /// the default for plain number and percent formatting is <c>0</c>; the default for currency formatting is the
        /// number of minor unit digits provided by the <see href="https://www.six-group.com/dam/download/financial-information/data-center/iso-currrency/lists/list-one.xml">ISO 4217 currency code list (XML file)</see>
        /// or <c>2</c> if list doesn't provide information.
        /// </summary>
        public int? MinimumFractionDigits;
        
        /// <summary>
        /// The maximum number of fraction digits to use. Possible values are from <c>0</c> to <c>100</c>; the default
        /// for plain number formatting is the larger of <c>minimumFractionDigits</c> and <c>3</c>; the default
        /// for currency formatting is the larger of <c>minimumFractionDigits</c> and the number of minor unit digits
        /// provided by the  <see href="https://www.six-group.com/dam/download/financial-information/data-center/iso-currrency/lists/list-one.xml">ISO 4217 currency code list (XML file)</see>
        /// or <c>2</c> if list doesn't provide information. The default for percent formatting is the larger of <c>minimumFractionDigits</c> and <c>0</c>.
        /// </summary>
        public int? MaximumFractionDigits;
        /// <summary>
        /// The minimum number of significant digits to use. Possible values are from <c>1</c> to <c>21</c>;
        /// the default is <c>1</c>.
        /// </summary>
        public int? MinimumSignificantDigits;
        
        /// <summary>
        /// The maximum  number of significant digits to use. Possible values are from <c>1</c> to <c>21</c>;
        /// the default is <c>21</c>.
        /// </summary>
        public int? MaximumSignificantDigits;

        /// <summary>
        /// Default constructor
        /// </summary>
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

    /// <summary>
    /// Represents which formatting style of a fluent number, specifying how the number is formatted.
    /// </summary>
    public enum FluentNumberStyle
    {
        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a number e.g. <c>1 000</c>.
        /// </summary>
        Decimal,
        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a currency, with provided currency e.g. <c>$100</c>.
        /// </summary>
        Currency,
        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a number, with percent symbol e.g. <c>19%</c>
        /// </summary>
        Percent,
        
        /// <summary>
        /// Formats <see cref="FluentNumber"/> as a number with provided measurement unit e.g. <c>100 gallons</c>
        /// </summary>
        Unit,
    }

    /// <summary>
    /// Represents the style of how currency is displayed
    /// </summary>
    public enum FluentNumberCurrencyDisplayStyle
    {
        /// <summary>
        /// Symbolic depiciton e.g. <c>$</c>.
        /// </summary>
        Symbol,
        /// <summary>
        /// Use ISO currency code e.g. <c>USD</c>.
        /// </summary>
        Code,
        /// <summary>
        /// Name of currency e.g. <c>dollar</c>
        /// </summary>
        Name,
    }
}
