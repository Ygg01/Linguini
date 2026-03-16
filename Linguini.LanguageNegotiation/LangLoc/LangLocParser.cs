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
        public static bool TryParse(string langLoc, out List<string> errors,
            [NotNullWhen(true)] out LangLocId? langLocId)
        {
            errors = new List<string>();
            var language = ReadOnlyMemory<char>.Empty;
            ReadOnlyMemory<char>? region = null;
            ReadOnlyMemory<char>? script = null;

            if (string.IsNullOrEmpty(langLoc))
            {
                errors.Add("Language tag cannot be null or empty");
                langLocId = null;
                return false;
            }

            if (langLoc == "root")
            {
                language = "root".AsMemory();
                langLocId = new LangLocId(language, region, script);
                return true;
            }

            var pos = 0;
            // Parse language subtag
            var firstPart = TryReadAlpha(langLoc.AsMemory(), pos);
            if (firstPart.Length == 2 || firstPart.Length == 3 || firstPart.Length == 5 || firstPart.Length == 8)
            {
                language = ToLowerCase(firstPart);
                pos += firstPart.Length;
            }
            else
            {
                errors.Add("Language tag must be 2, 3, 5 or 8 characters long");
                langLocId = null;
                return false;
            }
            
            // Parse script subtag (optional)
            script = GetScript(langLoc.AsMemory(), ref pos);

            region = GetRegion(langLoc.AsMemory(), ref errors, ref pos);


            if (errors.Count > 0)
            {
                langLocId = null;
                return false;
            } 
            
            langLocId = new LangLocId(language, region, script);
            return true;
        }

        private static ReadOnlyMemory<char>? GetScript(ReadOnlyMemory<char> input, ref int oldPos)
        {
            var pos = oldPos;
            
            // Skip  separator
            if (pos < input.Length 
                && (input.Span[pos] == '-' || input.Span[pos] == '_'))
            {
                pos += 1;
            }

            var secondPart = TryReadAlpha(input, pos);
            if (secondPart.Length != 4) return null;
                
            pos += secondPart.Length;
            oldPos = pos;

            return ToLowerCase(secondPart);
        }
        
        private static ReadOnlyMemory<char>? GetRegion(ReadOnlyMemory<char> input, ref List<string> errors, ref int oldPos)
        {
            var pos = oldPos;
            
            // Don't read past the end of the string
            if (pos >= input.Length) return null;

            if (input.Span[pos] == '-' || input.Span[pos] == '_')
            {
                pos += 1;

                if (pos >= input.Length)
                {
                    errors.Add("Unexpected end of tag");
                    return null;
                }

                if (char.IsAscii(input.Span[pos]))
                {
                    var alphaRegionCode = TryReadAlpha(input, pos);

                    if (alphaRegionCode.Length == 2)
                    {
                        ToUpperCase(alphaRegionCode);
                        pos += alphaRegionCode.Length;
                        oldPos = pos;
                        return ToUpperCase(alphaRegionCode);
                    }
                }
                else if (char.IsDigit(input.Span[pos]))
                {
                    var digitRegionCode = TryReadDigit(input, pos);
                    
                    if (digitRegionCode.Length == 3)
                    {
                        pos += digitRegionCode.Length;
                        oldPos = pos;
                        return ToUpperCase(digitRegionCode);
                    }
                }
            }
            errors.Add("Expected region code found something else");
            return null;
        }


        private static ReadOnlyMemory<char> TryReadAlpha(
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

            return readOnlyMemory.Slice(oldPos, ind - oldPos);
        }
        
        private static ReadOnlyMemory<char> TryReadDigit(
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
            return readOnlyMemory.Slice(oldPos, index - oldPos);
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