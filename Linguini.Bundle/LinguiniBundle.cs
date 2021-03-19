using System.Collections.Generic;
using System.Globalization;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle
{
    public class LinguiniBundle
    {
        public CultureInfo CultureInfo;
        public List<Resource> Resources;
        public Dictionary<string, IEntry> Entries;
    }
}
