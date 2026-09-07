using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Linguini.LanguageNegotiation.LangLoc
{


    
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public struct LangLocId : IEquatable<LangLocId>
    {
        public bool Equals(LangLocId other)
        {
            return Language.Equals(other.Language) && Nullable.Equals(Region, other.Region) && Nullable.Equals(Script, other.Script);
        }

        public override bool Equals(object? obj)
        {
            return obj is LangLocId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Language, Region, Script);
        }

        public static bool operator ==(LangLocId left, LangLocId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LangLocId left, LangLocId right)
        {
            return !left.Equals(right);
        }

        public static LangLocId EN = new LangLocId("en");
        public static LangLocId FR = new LangLocId("fr");
        public static LangLocId SR = new LangLocId("sr");
        public static LangLocId SR_RU = new LangLocId("sr", "RU");
        public static LangLocId AZ_IR = new LangLocId("az", "IR");
        public static LangLocId ZH_GB = new LangLocId("zh", "GB");
        
        public ReadOnlyMemory<char> Language { get; }
        public ReadOnlyMemory<char>? Region { get; set; }
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
        
         public LangLocId(string language, string? region = null,
            string? script = null)
        {
            Language = language.AsMemory();
            Region = region?.AsMemory();
            Script = script?.AsMemory();
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