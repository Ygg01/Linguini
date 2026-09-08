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
            return LanguageStr.Equals(other.LanguageStr, StringComparison.OrdinalIgnoreCase)
                   && String.Equals(RegionStr, other.RegionStr, StringComparison.OrdinalIgnoreCase)
                   && String.Equals(ScriptStr, other.ScriptStr, StringComparison.OrdinalIgnoreCase);
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
            return _original;
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

        private string _original;
        private ReadOnlyMemory<char> _language;
        private ReadOnlyMemory<char>? _region;
        private ReadOnlyMemory<char>? _script;

        public readonly ReadOnlyMemory<char> Language => _language;

        public readonly ReadOnlyMemory<char>? Region => _region;

        public readonly ReadOnlyMemory<char>? Script => _script;

        public string LanguageStr => Language.ToString();
        public string? RegionStr => Region?.ToString();
        public string? ScriptStr => Script?.ToString();
        

        public static LangLocId CreateFromOffsets(string full, Range language, Range? region = null, Range? script = null)
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
            
            if (scriptRange != null )
            {
                scriptMemory = _original.AsMemory(scriptRange.Value);
            }
            if (regionRange != null )
            {
                regionMemory = _original.AsMemory(regionRange.Value);
            }
            
            _script = scriptMemory;
            _region = regionMemory;
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
                cultureInfo.TwoLetterISOLanguageName,
                region.TwoLetterISORegionName
            );
        }

        public LangLocId ClearRegion()
        {
            return new LangLocId(LanguageStr, null, ScriptStr);
        }
    }
}