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
            if (!TryParseLanguage(langLoc.AsMemory(), ref pos, ref errors, out language))
            {
                langLocId = null;
                return false;
            }
            
            // skip `-`
            pos += 1;
            
            if (!TryParseScript(langLoc.AsMemory(), ref pos, ref errors, out script))
            {
                langLocId = null;
                return false;
            }
            
            //
            // index++;
            //
            // // Parse script subtag (optional)
            // if (index < parts.Length && TryParseScript(parts[index], out script))
            // {
            //     index++;
            // }
            //
            // // Parse region subtag (optional)
            // if (index < parts.Length && TryParseRegion(parts[index], out region))
            // {
            //     index++;
            // }

            langLocId = new LangLocId(language, region, script);
            return errors.Count == 0;
        }


        private static bool TryParseLanguage(
            ReadOnlyMemory<char> value,
            ref int index,
            ref List<string> errors,
            out ReadOnlyMemory<char> language)
        {
            language = string.Empty.AsMemory();

            if (value.IsEmpty)
            {
                errors.Add("Language tag cannot be empty");
                return false;
            }

            for (; index < value.Length; index++)
            {
                if (value.Span[index] != '-' && value.Span[index] != '_') continue;
                break;
            }
            language = value.Slice(0, index);

            if (!IsAlpha(language))
            {
                errors.Add("Language tag must contain only letters");
                return false;
            }

            if ((index < 2 || index > 3) && (index < 5 || index > 8))
            {
                errors.Add("Language tag can only be 2-3 or 5-8 characters long");
                return false;
            }

            language = ToLowerCase(language);
            return true;
        }
        
        private static bool TryParseScript(
            ReadOnlyMemory<char> asMemory, 
            ref int pos, 
            ref List<string> errors, 
            out ReadOnlyMemory<char>? script)
        {
            // Not enough space for language script
            if (pos + 4 >= asMemory.Length)
            {
                script = null;
                return true;
            }
            var scriptStr = asMemory.Slice(pos, 4);
            if (IsAlpha(scriptStr))
            {
                script = scriptStr;
                pos += 4;
            }
        }


        private static ReadOnlyMemory<char> ToLowerCase(ReadOnlyMemory<char> language)
        {
            if (IsAllLowercase(language))
            {
                return language;
            }

            var memory = new char[language.Length];
            foreach (char c in language.Span)
            {
                memory[0] = char.ToLowerInvariant(c);
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

        

        private static bool IsAlpha(ReadOnlyMemory<char> value)
        {
            foreach (char c in value.Span)
            {
                if (!char.IsLetter(c))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsDigit(string value)
        {
            foreach (char c in value)
            {
                if (!char.IsDigit(c))
                    return false;
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