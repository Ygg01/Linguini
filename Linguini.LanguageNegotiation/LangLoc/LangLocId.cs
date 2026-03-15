using System.Globalization;

namespace Linguini.LanguageNegotiation.LangLoc
{
    public sealed class LangLocId
    {
        string Language { get; }
        string? Region { get; }
        string? Script { get;  }

        private LangLocId(string language, string? region = null, string? script = null)
        {
            Region = region;
            Language = language;
            Script = script;
        }
        
        public static LangLocId Create(string langLoc)
        {
            if (LangLocParser.TryParse(langLoc, out var errors, out var region, out var language, out var script))
            {
                return new LangLocId(region, language, script);
            }
            throw new LangParseError(errors);
        }

        public static LangLocId FromCultureInfo(CultureInfo cultureInfo)
        {
            return Create(cultureInfo.Name);
        }
    }
}