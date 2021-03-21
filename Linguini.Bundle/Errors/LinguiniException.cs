using System;
using System.Collections.Generic;
using System.Text;
using Linguini.Bundle.Errors;

namespace Linguini.Bundle
{
    public class LinguiniException : Exception
    {
        public LinguiniException(List<Error> errors)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Following errors weren't handled:\n");
            foreach (var error in errors)
            {
                sb.Append(error).Append('\n');
            }
        }
    }
}
