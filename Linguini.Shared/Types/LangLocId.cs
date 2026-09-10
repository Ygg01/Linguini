using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Linguini.Shared.Types
{
    /// <summary>
    /// Presents a BCP-47 <i>like</i> language tag. Essentially a triplet of language, script, and region.
    /// </summary>
    public struct LangLocId : IEquatable<LangLocId>
    {
        /// <inheritdoc/>
        public bool Equals(LangLocId other)
        {
            return LanguageStr.Equals(other.LanguageStr, StringComparison.OrdinalIgnoreCase)
                   && String.Equals(RegionStr, other.RegionStr, StringComparison.OrdinalIgnoreCase)
                   && String.Equals(ScriptStr, other.ScriptStr, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is LangLocId other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Language, Region, Script);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _original;
        }

        /// <summary>
        /// Defines an equality operator for comparing two instances of <see cref="LangLocId"/>
        /// to determine if they are equal. The comparison is case-insensitive.
        /// </summary>
        /// <param name="left">The first <see cref="LangLocId"/> to compare.</param>
        /// <param name="right">The second <see cref="LangLocId"/> to compare.</param>
        /// <returns>True if the two instances are equal; otherwise, false.</returns>
        public static bool operator ==(LangLocId left, LangLocId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="LangLocId"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="LangLocId"/> to compare.</param>
        /// <param name="right">The second <see cref="LangLocId"/> to compare.</param>
        /// <returns>True if the two instances are inequal; otherwise, false.</returns>
        public static bool operator !=(LangLocId left, LangLocId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Language identifier for English language of with an unspecified region.
        /// </summary>
        public static readonly LangLocId EN = new LangLocId("en");
        
        /// <summary>
        /// Language identifier for French language of with an unspecified region.
        /// </summary>
        public static readonly LangLocId FR = new LangLocId("fr");
        
        /// <summary>
        /// Language identifier for Serbian language of with an unspecified region.
        /// </summary>
        public static readonly LangLocId SR = new LangLocId("sr");

        /// <summary>
        /// Language identifier for the Serbian language as used in the Cyrillic script region of Russia.
        /// </summary>
        public static readonly LangLocId SR_RU = new LangLocId("sr", "RU");

        /// <summary>
        /// Language identifier for the Azerbaijani language as used in Iran.
        /// </summary>
        public static readonly LangLocId AZ_IR = new LangLocId("az", "IR");
        
        /// <summary>
        /// Language identifier for the Chinese language as used in the United Kingdom.
        /// </summary>
        public static readonly LangLocId ZH_GB = new LangLocId("zh", "GB");
        
        /// <summary>
        /// Language identifier for the Chinese language as used in the United States.
        /// </summary>
        public static readonly LangLocId ZH_US = new LangLocId("zh", "US");

        private string _original;
        private ReadOnlyMemory<char> _language;
        private ReadOnlyMemory<char>? _region;
        private ReadOnlyMemory<char>? _script;

        /// <summary>
        /// Represents the language component of a <see cref="LangLocId"/>, identifying the linguistic
        /// aspect of a locale. For instance, <c>en</c> in <c>en-US</c> specifies English as the language.
        /// </summary>
        public readonly ReadOnlyMemory<char> Language => _language;

        /// <summary>
        /// Represents the region part of <see cref="LangLocId"/>, indicating the geographical or political region
        /// associated with a particular language. For example, <c>US</c> in <c>en-US</c> specifies the United States
        /// as the region for English.
        /// </summary>
        public readonly ReadOnlyMemory<char>? Region => _region;

        /// <summary>
        /// Represents the script component of a <see cref="LangLocId"/>, identifying the writing system
        /// or alphabet used to write the language. For example, <c>Latn</c> in <c>sr-Latn-RS</c> specifies
        /// the Latin script for Serbian.
        /// </summary>
        public readonly ReadOnlyMemory<char>? Script => _script;

        /// <summary>
        /// Convenience for converting the <see cref="Language"/> to a string.
        /// </summary>
        public readonly string LanguageStr => Language.ToString();

        /// <summary>
        /// Convenience for converting the <see cref="Region"/> to a string.
        /// </summary>
        public readonly string? RegionStr => Region?.ToString();

        /// <summary>
        /// Convenience for converting the <see cref="Script"/> to a string.
        /// </summary>
        public readonly string? ScriptStr => Script?.ToString();


#pragma warning disable 1591
        /// Method used internally by the LangLocParser DO NOT USE unless you know how parsing works.
        public static LangLocId CreateFromOffsets(string full, Range language, Range? region = null,
            Range? script = null)
        {
            ReadOnlyMemory<char>? scriptMem = null;
            ReadOnlyMemory<char>? regionMem = null;
            if (script != null)
            {
                scriptMem = full.AsMemory(script.Value);
            }

            if (region != null)
            {
                regionMem = full.AsMemory(region.Value);
            }

            return new LangLocId()
            {
                _original = full,
                _language = full.AsMemory(language),
                _region = regionMem,
                _script = scriptMem
            };
        }
#pragma warning restore 1591

        /// <summary>
        /// Presents a BCP-47 <i>like</i> language tag. Essentially a triplet of language, script, and region.
        /// </summary>
        /// <param name="language">Language spooken. E.g. <c>en</c> for English</param>
        /// <param name="region">Which region the language is spoken in. E.g. <c>GB</c> in <c>en-GB</c> for region of Britain.</param>
        /// <param name="script">Which script is used for writing the language. E.g. <c>Latn</c> for Latin alphabet used in <c>sr-Latn-RS</c>.</param>
        public LangLocId(string language, string? region = null,
            string? script = null)
        {
            var sb = new StringBuilder(language);
            var langRange = ..sb.Length;
            ReadOnlyMemory<char>? scriptMemory = null;
            ReadOnlyMemory<char>? regionMemory = null;
            Range? scriptRange = null;
            Range? regionRange = null;

            if (script != null)
            {
                sb.Append('-');
                var scriptStart = sb.Length;
                sb.Append(script);
                var scriptEnd = sb.Length;
                scriptRange = new Range(scriptStart, scriptEnd);
            }

            if (region != null)
            {
                sb.Append('-');
                var regionStart = sb.Length;
                sb.Append(region);
                var regionEnd = sb.Length;
                regionRange = new Range(regionStart, regionEnd);
            }

            _original = sb.ToString();
            _language = _original.AsMemory(langRange);

            if (scriptRange != null)
            {
                scriptMemory = _original.AsMemory(scriptRange.Value);
            }

            if (regionRange != null)
            {
                regionMemory = _original.AsMemory(regionRange.Value);
            }

            _script = scriptMemory;
            _region = regionMemory;
        }


        /// <summary>
        /// Constructs a <see cref="LangLocId"/> from a <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="cultureInfo">Culture info used to calculate the language.</param>
        /// <returns></returns>
        public static LangLocId FromCultureInfo(CultureInfo cultureInfo)
        {
            var region = new RegionInfo(cultureInfo.LCID);
            return new LangLocId(
                cultureInfo.TwoLetterISOLanguageName,
                region.TwoLetterISORegionName
            );
        }

        /// <summary>
        /// Creates a new instance of <see cref="LangLocId"/> with the same language and script values
        /// as the current instance but with the region value removed.
        /// </summary>
        /// <returns>A new LangLocId instance with the region cleared.</returns>
        public LangLocId ClearRegion()
        {
            return new LangLocId(LanguageStr, null, ScriptStr);
        }

        /// <summary>
        /// Operator for converting string to <see cref="LangLocId"/>. Convenience for calling <see cref="LangLocParser.Parse"/>.
        /// </summary>
        /// <param name="unparsedLocale">string representing a locale.</param>
        /// <returns>LangLocId</returns>
        public static implicit operator LangLocId(string unparsedLocale)
        {
            return LangLocParser.Parse(unparsedLocale);
        }
    }

    /// <summary>
    /// Represents the LangLocParser class, which is responsible for the parsing
    /// and processing of language tags and locale negotiation. This class is
    /// used to handle operations related to language and region identifiers
    /// for internationalization and localization purposes.
    /// </summary>
    /// <remarks>
    /// This class should conform to https://www.unicode.org/reports/tr35/#unicode_language_id
    /// unicode_language_id = "root"
    ///     | (unicode_language_subtag (sep unicode_script_subtag)? (sep unicode_region_subtag)? (sep unicode_variant_subtag)*)
    /// <br/>
    /// unicode_language_subtag = alpha{2,3} | alpha{5,8}
    /// <br/>
    /// unicode_script_subtag   = alpha{4}
    /// <br/>
    /// unicode_region_subtag   = (alpha{2} | digit{3})
    /// <br/>
    /// unicode_variant_subtag  = (alphanum{5,8} | digit alphanum{3})
    /// <br/>
    /// sep                     = "-" | "_"
    /// <br/>
    /// alphanum                = alpha | digit
    /// <br/>
    /// alpha                   = "a".."z" | "A".."Z"
    /// <br/>
    /// digit                   = "0".."9"
    /// </remarks>
    public class LangLocParser
    {


        /// <summary>
        /// Attempts to parse a language-locale identifier string into a <see cref="LangLocId"/> object.
        /// </summary>
        /// <param name="langLoc">
        /// The language-locale identifier string to be parsed.
        /// </param>
        /// <param name="errors">
        /// A list of errors encountered during parsing, if any.
        /// </param>
        /// <param name="langLocId">
        /// When this method returns, contains the parsed <see cref="LangLocId"/> object if successful,
        /// or <c>null</c> if parsing fails.
        /// </param>
        /// <returns>
        /// <c>true</c> if the parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse(string? langLoc, out List<string> errors,
            [NotNullWhen(true)] out LangLocId? langLocId)
        {
            errors = new List<string>();

            if (string.IsNullOrEmpty(langLoc))
            {
                errors.Add("Language tag cannot be null or empty");
                langLocId = null;
                return false;
            }

            if (langLoc == "root")
            {
                langLocId = new LangLocId(langLoc);
                return true;
            }

            var pos = 0;
            var languageEnd = 0;
            // Parse language subtag
            var length = TryReadAlpha(langLoc.AsMemory(), pos);
            if (length == 2 || length == 3 || length == 5 || length == 8)
            {
                pos += length;
                languageEnd = pos;
            }
            else
            {
                errors.Add("Language tag must be 2, 3, 5 or 8 characters long");
                langLocId = null;
                return false;
            }

            // Parse script subtag (optional)
            var scriptRange = GetScript(langLoc.AsMemory(), ref pos);

            var regionRange = GetRegion(langLoc.AsMemory(), ref errors, ref pos);


            if (errors.Count > 0)
            {
                langLocId = null;
                return false;
            }

            langLocId = LangLocId.CreateFromOffsets(langLoc, 0..languageEnd, regionRange, scriptRange);
            return true;
        }

        private static Range? GetScript(ReadOnlyMemory<char> input, ref int oldPos)
        {
            var pos = oldPos;
            var rangeStart = pos;
            int rangeEnd;

            // Skip `-` | `_` separator
            if (pos < input.Length
                && (input.Span[pos] == '-' || input.Span[pos] == '_'))
            {
                pos += 1;
                rangeStart = pos;
            }

            var length = TryReadAlpha(input, pos);
            if (length != 4) return null;

            pos += length;
            oldPos = pos;
            rangeEnd = pos;

            return new Range(rangeStart, rangeEnd);
        }

        private static Range? GetRegion(ReadOnlyMemory<char> input, ref List<string> errors, ref int oldPos)
        {
            var pos = oldPos;

            // Don't read past the end of the string
            if (pos >= input.Length) return null;

            if (input.Span[pos] == '-' || input.Span[pos] == '_')
            {
                pos += 1;
                var regionStart = pos;

                if (pos >= input.Length)
                {
                    errors.Add("Unexpected end of tag");
                    return null;
                }

                int regionEnd;
                if (IsAscii(input.Span[pos]))
                {
                    var length = TryReadAlpha(input, pos);

                    if (length == 2)
                    {
                        pos += length;
                        regionEnd = pos;
                        oldPos = pos;
                        return new Range(regionStart, regionEnd);
                    }
                }
                else if (char.IsDigit(input.Span[pos]))
                {
                    var digitRegionLength = TryReadDigit(input, pos);

                    if (digitRegionLength == 3)
                    {
                        pos += digitRegionLength;
                        regionEnd = pos;
                        oldPos = pos;
                        return new Range(regionStart, regionEnd);
                    }
                }
            }

            errors.Add("Expected region code found something else");
            return null;
        }


        private static int TryReadAlpha(
            ReadOnlyMemory<char> readOnlyMemory,
            int oldPos)
        {
            var ind = oldPos;
            foreach (var chr in readOnlyMemory.Span[oldPos..])
            {
                if (!IsAsciiLetter(chr))
                {
                    break;
                }

                ind += 1;
            }

            return ind - oldPos;
        }

        private static int TryReadDigit(
            ReadOnlyMemory<char> readOnlyMemory,
            int oldPos)
        {
            var index = oldPos;
            foreach (var chr in readOnlyMemory.Span[oldPos..])
            {
                if (!IsAsciiDigit(chr))
                {
                    break;
                }

                index += 1;
            }

            return index - oldPos;
        }


        /// <summary>
        /// Tries to parse a string into <see cref="LangLocId"/>. 
        /// </summary>
        /// <param name="unparsedLangLoc">Unparsed language location string</param>
        /// <returns><see cref="LangLocId"/> if successful</returns>
        /// <exception cref="LangParseError">Throws error upon encountering errors</exception>
        public static LangLocId Parse(string? unparsedLangLoc)
        {
            if (unparsedLangLoc == null)
            {
                throw new LangParseError("Unparsed language location string cannot be null");
            }

            return LangLocParser.TryParse(unparsedLangLoc, out var errors, out var langLocId)
                ? langLocId.Value
                : throw new LangParseError(errors);
        }


        private static bool IsAscii(char c) => c <= '\u007F';
        private static bool IsAsciiLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        private static bool IsAsciiDigit(char c) => c >= '0' && c <= '9';
    }


    /// <summary>
    /// Represents an error that occurs during the parsing of a language location string.
    /// </summary>
    public class LangParseError : Exception
    {
        /// <summary>
        /// A collection of error messages associated with the occurrence of one or more parsing failures.
        /// </summary>
        public List<string> Errors;

        /// <summary>
        /// Represents an error that occurs during the parsing of a language location string.
        /// </summary>
        /// <param name="error">One or more error messages</param>
        public LangParseError(params string[] error)
        {
            Errors = new List<string>(error);
        }

        /// <summary>
        /// Represents an error that occurs during the parsing of a language location string.
        /// </summary>
        /// <param name="error">One or more error messages</param>
        public LangParseError(List<string> error)
        {
            Errors = new(error);
        }
    }
}