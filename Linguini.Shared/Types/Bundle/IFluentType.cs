﻿namespace Linguini.Shared.Types.Bundle
{
    /// <summary>
    /// Common interface for fluent types
    /// </summary>
    public interface IFluentType
    {
        /// <summary>
        /// String representation of the value
        /// </summary>
        /// <returns>String value of the Fluent type.</returns>
        string AsString();

        /// <summary>
        /// Determines if type is an error. Defaults to <c>false</c>.
        /// </summary>
        /// <returns>If the value returned is an error.</returns>
        bool IsError()
        {
            return false;
        }

        /// <summary>
        /// Method for matching Fluent types.
        ///
        /// Should be overriden if the type has custom value matching. E.g. for datetime
        /// one could compare by the long value of their timestamp or by their representations.
        /// </summary>
        /// <param name="other">The other FluentType to compare this value with</param>
        /// <param name="scope">Current scope of the fluent bundle</param>
        /// <returns><c>true</c> if the values match and <c>false</c> otherwise</returns>
        bool Matches(IFluentType other, IScope scope)
        {
            return (this, other) switch
            {
                (FluentString fs1, FluentString fs2) => fs1.Equals(fs2),
                (FluentNumber fn1, FluentNumber fn2) => fn1.Equals(fn2),
                (FluentString fs1, FluentNumber fn2) => scope.MatchByPluralCategory(fs1, fn2),
                _ => false,
            };
        }

        /// <summary>
        /// Copies the Fluent value
        /// </summary>
        /// <returns>The copy of this fluent value</returns>
        IFluentType Copy();
    }

    /// <summary>
    /// Fluent type used to denote errors, it's only basic type that returns true for <c>IsError</c>
    /// </summary>
    public class FluentErrType : IFluentType
    {
        /// <inheritdoc/>
        public IFluentType Copy()
        {
            return this;
        }

        /// <summary>
        /// Fluent representation of error
        /// </summary>
        /// <returns>A constant string value.</returns>
        public string AsString()
        {
            return "FluentErrType";
        }

        /// <inheritdoc/>
        public bool IsError()
        {
            return true;
        }
    }
}
