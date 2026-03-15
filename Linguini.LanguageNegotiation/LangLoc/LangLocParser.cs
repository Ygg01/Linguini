using System;
using System.Collections.Generic;

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
    /// 
    /// unicode_language_subtag = alpha{2,3} | alpha{5,8}
    /// unicode_script_subtag   = alpha{4}
    /// unicode_region_subtag   = (alpha{2} | digit{3})
    /// unicode_variant_subtag  = (alphanum{5,8} | digit alphanum{3})
    /// </remarks>
    public class LangLocParser
    {
        public static bool TryParse(string langLoc, out List<string> errors, out string language, out string? script, out string? region)
        {
            errors = new List<string>();
            language = string.Empty;
            script = string.Empty;
            region = string.Empty;

            if (string.IsNullOrEmpty(langLoc))
            {
                errors.Add("Language tag cannot be null or empty");
                return false;
            }

            if (langLoc == "root")
            {
                language = "root";
                return true;
            }

            var parts = langLoc.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                errors.Add("Invalid language tag format");
                return false;
            }

            int index = 0;

            // Parse language subtag
            if (!TryParseLanguage(parts[index], out language))
            {
                errors.Add($"Invalid language subtag: {parts[index]}");
                return false;
            }
            index++;

            // Parse script subtag (optional)
            if (index < parts.Length && TryParseScript(parts[index], out script))
            {
                index++;
            }

            // Parse region subtag (optional)
            if (index < parts.Length && TryParseRegion(parts[index], out region))
            {
                index++;
            }

            return errors.Count == 0;
        }

        private static bool TryParseLanguage(string value, out string language)
        {
            language = string.Empty;
            if (string.IsNullOrEmpty(value))
                return false;

            if (!IsAlpha(value))
                return false;

            int len = value.Length;
            if ((len >= 2 && len <= 3) || (len >= 5 && len <= 8))
            {
                language = value.ToLowerInvariant();
                return true;
            }

            return false;
        }

        private static bool TryParseScript(string value, out string script)
        {
            script = string.Empty;
            if (string.IsNullOrEmpty(value))
                return false;

            if (!IsAlpha(value))
                return false;

            if (value.Length == 4)
            {
                script = char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
                return true;
            }

            return false;
        }

        private static bool TryParseRegion(string value, out string region)
        {
            region = string.Empty;
            if (string.IsNullOrEmpty(value))
                return false;

            if (value.Length == 2 && IsAlpha(value))
            {
                region = value.ToUpperInvariant();
                return true;
            }

            if (value.Length == 3 && IsDigit(value))
            {
                region = value;
                return true;
            }

            return false;
        }

        private static bool IsAlpha(string value)
        {
            foreach (char c in value)
            {
                if (!char.IsLetter(c))
                    return false;
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