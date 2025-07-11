using System;
using System.Collections.Generic;
using System.Text;

namespace Linguini.Bundle.Errors
{
    /// <summary>
    /// Represents an exception that occurs within the Linguini system when processing localized messages or when term
    /// resolution fails.
    /// </summary>
    public class LinguiniException : Exception
    {
        /// <summary>
        /// Constructs an exception from a list of <see cref="FluentError"/>
        /// </summary>
        /// <param name="errors">List of errors that occured during parsing or resolution.</param>
        public LinguiniException(IList<FluentError> errors) 
            : this(FluentErrorsToString(errors))
        {
        }

        private static string FluentErrorsToString(IList<FluentError> errors)
        {
            StringBuilder sb = new();
            sb.Append("Following errors weren't handled:\n");
            foreach (var error in errors)
            {
                sb.Append(error).Append('\n');
            }
            return sb.ToString();
        }

        private LinguiniException(string name) : base(name) {}
    }
}
