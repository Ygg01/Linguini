using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Linguini.LanguageNegotiation.LangLoc
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
            return LangLocParser.TryParse(langLoc, out var errors, out var langLocId) 
                ? langLocId.Value 
                : throw new LangParseError(errors);
        }

        public static LangLocId FromCultureInfo(CultureInfo cultureInfo)
        {
            var region = new RegionInfo(cultureInfo.LCID);
            return new LangLocId(
                cultureInfo.TwoLetterISOLanguageName.AsMemory(),
                region.ThreeLetterISORegionName.AsMemory()
            );
        }
    }
}