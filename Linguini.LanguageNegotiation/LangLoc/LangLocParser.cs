using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Linguini.LanguageNegotiation.LangLoc
{
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
        public static LangLocId? Parse(string? langLoc)
        {
            if (string.IsNullOrEmpty(langLoc))
            {
                return null;
            }

            return TryParse(langLoc, out _, out var langLocId) ? langLocId : null;
        }
        
        public static bool TryParse(string langLoc, out List<string> errors,
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
            if (length== 2 || length == 3 || length== 5 || length == 8)
            {
                // language = ToLowerCase(firstPart);
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
            var rangeEnd = pos;
            
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
            var regionStart = pos;
            var regionEnd = pos;
            
            // Don't read past the end of the string
            if (pos >= input.Length) return null;

            if (input.Span[pos] == '-' || input.Span[pos] == '_')
            {
                pos += 1;
                regionStart = pos;

                if (pos >= input.Length)
                {
                    errors.Add("Unexpected end of tag");
                    return null;
                }

                if (char.IsAscii(input.Span[pos]))
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
                    
                    if (digitRegionLength== 3)
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
                if (!char.IsAsciiLetter(chr))
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
                if (!char.IsAsciiDigit(chr))
                {
                    break;
                }

                index += 1;
            }
            return index - oldPos;
        }

        private static ReadOnlyMemory<char> ToLowerCase(ReadOnlyMemory<char> language)
        {
            if (IsAllLowercase(language))
            {
                return language;
            }

            var memory = new char[language.Length];
            var i = 0;
            foreach (var c in language.Span)
            {
                memory[i] = char.ToLowerInvariant(c);
                i += 1;
            }

            return memory.AsMemory();
        }
        
        private static ReadOnlyMemory<char> ToUpperCase(ReadOnlyMemory<char> language)
        {
            if (IsAllUppercase(language))
            {
                return language;
            }

            var memory = new char[language.Length];
            var i = 0;
            foreach (char c in language.Span)
            {
                memory[i] = char.ToUpperInvariant(c);
                i += 1;
            }

            return memory.AsMemory();
        }

        private static bool IsAllLowercase(ReadOnlyMemory<char> value)
        {
            foreach (char c in value.Span)
            {
                if (!char.IsAsciiLetterLower(c))
                {
                    return false;
                }
            }

            return true;
        }
        
        private static bool IsAllUppercase(ReadOnlyMemory<char> value)
        {
            foreach (char c in value.Span)
            {
                if (!char.IsAsciiLetterUpper(c))
                {
                    return false;
                }
            }

            return true;
        }
    }


    public class LangParseError : Exception
    {
        public List<string> Errors;

        public LangParseError(string error)
        {
            Errors = new List<string> { error };
        }

        public LangParseError(List<string> errors)
        {
            Errors = errors;
        }
    }
}