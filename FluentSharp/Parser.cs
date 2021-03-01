using System;
using System.Collections.Generic;
using System.IO;
using FluentSharp.IO;

namespace FluentSharp
{
    public class Parser
    {
        public Resource Parse(TextReader input)
        {
            // TODO add buffering
            ZeroCopyReader zeroCopyReader = new ZeroCopyReader(input.ReadToEnd());

            var errors = new List<ParseError>();
            var body = new List<Entry>();
            
            zeroCopyReader.SkipBlankBlock();
            while (zeroCopyReader.IsNotEof())
            {
                GetEntry();
            }
            
            return new Resource(body);
        }

        private void GetEntry()
        {
            throw new NotImplementedException();
        }
    }

    public class ParseError
    {
    }
}
