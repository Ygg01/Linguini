using System;
using System.Collections.Generic;
using System.Text;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Function
{
    /// <summary>
    ///     Provides a set of static functions to be used within the Fluent localization system.
    /// </summary>
    public static class LinguiniFluentFunctions
    {
        /// <summary>
        ///     Converts the first argument to a <see cref="FluentNumber" /> if possible and merges
        ///     any additional named arguments. If the conversion fails, returns a <see cref="FluentErrType" />.
        /// </summary>
        /// <param name="args">
        ///     A list of <see cref="IFluentType" /> arguments. The first argument is expected
        ///     to be convertible to a <see cref="FluentNumber" />.
        /// </param>
        /// <param name="namedArgs">
        ///     A dictionary of named arguments where the key is the argument name and the
        ///     value is the corresponding <see cref="IFluentType" />.
        /// </param>
        /// <returns>
        ///     Returns the converted <see cref="FluentNumber" /> if successful, or a <see cref="FluentErrType" />
        ///     if the conversion fails.
        /// </returns>
        public static IFluentType Number(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            var num = args[0].ToFluentNumber();
            if (num != null)
                // TODO merge named arguments
                return num;

            return new FluentErrType();
        }

        /// <summary>
        ///     Calculates the sum of numeric values from a list of Fluent arguments.
        ///     If any argument cannot be converted to a <see cref="FluentNumber" />, returns a <see cref="FluentErrType" />.
        /// </summary>
        /// <param name="args">
        ///     A list of <see cref="IFluentType" /> arguments to be summed. Each argument is expected
        ///     to be convertible to a <see cref="FluentNumber" />.
        /// </param>
        /// <param name="namedArgs">
        ///     A dictionary of named arguments, where the key is a string representing
        ///     the argument name, and the value is the corresponding <see cref="IFluentType" />. This parameter is not used in
        ///     this method.
        /// </param>
        /// <returns>
        ///     Returns the sum of all arguments as a <see cref="FluentNumber" /> if all conversions succeed,
        ///     or a <see cref="FluentErrType" /> if any conversion fails.
        /// </returns>
        public static IFluentType Sum(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            var sum = 0.0;
            for (var i = 0; i < args.Count; i++)
            {
                var fluentType = args[i].ToFluentNumber();
                if (fluentType == null) return new FluentErrType();

                sum += fluentType.Value;
            }

            return (FluentNumber)sum;
        }

        /// <summary>
        ///     Identity function that returns the first argument unaltered if it exists, or returns a <see cref="FluentErrType" />
        ///     if an exception occurs, such as when the argument is missing.
        /// </summary>
        /// <param name="args">
        ///     A list of <see cref="IFluentType" /> arguments. The first argument is expected to be
        ///     present and will be returned as is.
        /// </param>
        /// <param name="namedArgs">
        ///     A dictionary of named arguments where the key is the argument name and the value is
        ///     the corresponding <see cref="IFluentType" />. These are not utilized in this method.
        /// </param>
        /// <returns>
        ///     Returns the first argument as an <see cref="IFluentType" /> if successful, or a <see cref="FluentErrType" />
        ///     if an exception occurs.
        /// </returns>
        public static IFluentType Identity(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            try
            {
                var id = args[0];
                return id;
            }
            catch (Exception)
            {
                return new FluentString("IDENTITY()");
            }
        }

        /// <summary>
        ///     Concatenates the values from the provided arguments into a single string.
        /// </summary>
        /// <param name="args">
        ///     A list of <see cref="IFluentType" /> arguments. The method concatenates values that
        ///     are either <see cref="FluentString" /> or <see cref="FluentNumber" />.
        /// </param>
        /// <param name="namedArgs">
        ///     A dictionary of named arguments. These are not used in this method.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="FluentString" /> representing the concatenated result
        ///     of the string and number values from the provided arguments.
        /// </returns>
        public static IFluentType Concat(IList<IFluentType> args, IDictionary<string, IFluentType> namedArgs)
        {
            var stringConcat = new StringBuilder();
            for (var i = 0; i < args.Count; i++)
            {
                var str = args[i];
                if (str is FluentString fs)
                    stringConcat.Append(fs);
                else if (str is FluentNumber fn) stringConcat.Append(fn);
            }

            return (FluentString)stringConcat.ToString();
        }
    }
}