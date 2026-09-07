using System;
using System.Globalization;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Linguini.LanguageNegotiation.LangLoc
{
    
    public struct LangLocId : IEquatable<LangLocId>
    {
        public bool Equals(LangLocId other)
        {
            return LanguageStr.Equals(other.LanguageStr)
                   && Nullable.Equals(RegionStr, other.RegionStr) 
                   && Nullable.Equals(ScriptStr, other.ScriptStr);
        }

        public override bool Equals(object? obj)
        {
            return obj is LangLocId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Language, Region, Script);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(10);
            
            sb.Append(LanguageStr);

            if (Region != null)
            {
                sb.Append('-');
                sb.Append(RegionStr);
            }
            
            if (Script != null)
            {
                sb.Append('-');
                sb.Append(ScriptStr);
            }
            
            return sb.ToString();
        }

        public static bool operator ==(LangLocId left, LangLocId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LangLocId left, LangLocId right)
        {
            return !left.Equals(right);
        }


        public static readonly LangLocId EN = new LangLocId("en");
        public static readonly LangLocId FR = new LangLocId("fr");
        public static readonly LangLocId SR = new LangLocId("sr");
        public static readonly LangLocId SR_SR = new LangLocId("sr", "SR");
        public static readonly LangLocId AZ_IR = new LangLocId("az", "IR");
        public static readonly LangLocId ZH_GB = new LangLocId("zh", "GB");
        public static readonly LangLocId ZH_US = new LangLocId("zh", "US");

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
                region.TwoLetterISORegionName.AsMemory()
            );
        }
    }
}