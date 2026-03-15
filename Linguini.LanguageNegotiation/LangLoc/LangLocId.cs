using System;
using System.Globalization;

namespace Linguini.LanguageNegotiation.LangLoc
{
    public struct LangLocId
    {
        public ReadOnlyMemory<char> Language { get; }
        public ReadOnlyMemory<char>? Region { get; }
        public ReadOnlyMemory<char>? Script { get; }
        
        public string LanguageStr => Language.ToString();
        public string? RegionStr => Region?.ToString();
        public string? ScriptStr => Script?.ToString();
        
        public LangLocId(ReadOnlyMemory<char> language, ReadOnlyMemory<char>? region = null,
            ReadOnlyMemory<char>? script = null)
        {
            Language = language;
            Region = region;
            Script = script;
        }
        
        public static LangLocId Create(string langLoc)
        {
            if (LangLocParser.TryParse(langLoc, out var errors, out var langLocId))
            {
                return langLocId.Value;
            }
            throw new LangParseError(errors);
        }

        public static LangLocId FromCultureInfo(CultureInfo cultureInfo)
        {
            return Create(cultureInfo.Name);
        }
    }
}