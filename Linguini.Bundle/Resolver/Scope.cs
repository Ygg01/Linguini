using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using PluralRulesGenerated;

namespace Linguini.Bundle.Resolver
{
    /// <summary>
    ///     Represents the operational context used for resolving Fluent expressions and patterns.
    ///     The class acts as an intermediary between the localization logic (expressed in patterns and expressions)
    ///     and the runtime localization data (like arguments and bundle-specific configurations).
    /// </summary>
    public class Scope : IScope
    {
        private readonly Dictionary<string, IFluentType>? _args;
        private readonly CultureInfo _culture;
        private readonly List<FluentError> _errors;

        /// <summary>
        ///     Represents the primary bundle associated with the current scope.
        ///     The <c>Bundle</c> variable provides access to the underlying implementation of the <see cref="IReadBundle" />
        ///     interface.
        ///     It is utilized in message resolution, formatting, and retrieving required elements such as terms or attributes.
        /// </summary>
        public readonly IReadBundle Bundle;

        internal readonly int MaxPlaceable;
        internal readonly int MaxRecursion;
        internal readonly List<Pattern> Travelled;

        private Dictionary<string, IFluentType>? _localNameArgs;
        private List<IFluentType>? _localPosArgs;


        /// <summary>
        ///     Constructor for <see cref="Scope" />
        /// </summary>
        /// <param name="fluentBundle">input bundle</param>
        /// <param name="args">arguments provided to the scope for resolution.</param>
        public Scope(FluentBundle fluentBundle, IDictionary<string, IFluentType>? args)
        {
            Placeable = 0;
            Bundle = fluentBundle;
            MaxPlaceable = fluentBundle.MaxPlaceable;
            MaxRecursion = fluentBundle.MaxRecursion;
            _culture = fluentBundle.Culture;
            TransformFunc = fluentBundle.TransformFunc;
            FormatterFunc = fluentBundle.FormatterFunc;
            UseIsolating = fluentBundle.UseIsolating;
            FormatterFunc = fluentBundle.FormatterFunc;
            Dirty = false;

            _errors = new List<FluentError>();
            Travelled = new List<Pattern>();
            _args = args != null ? new Dictionary<string, IFluentType>(args) : null;

            _localNameArgs = null;
            _localPosArgs = null;
            _errors = new List<FluentError>();
        }

        /// <summary>
        ///     Constructor for <see cref="Scope" />
        /// </summary>
        /// <param name="frozenBundle">read only bundle</param>
        /// <param name="args">arguments provided to the scope for resolution.</param>
        public Scope(FrozenBundle frozenBundle, IDictionary<string, IFluentType>? args)
        {
            Placeable = 0;
            Bundle = frozenBundle;
            MaxPlaceable = frozenBundle.MaxPlaceable;
            _culture = frozenBundle.Culture;
            TransformFunc = frozenBundle.TransformFunc;
            FormatterFunc = frozenBundle.FormatterFunc;
            UseIsolating = frozenBundle.UseIsolating;
            Dirty = false;

            _errors = new List<FluentError>();
            Travelled = new List<Pattern>();
            _args = args != null ? new Dictionary<string, IFluentType>(args) : null;

            _localNameArgs = null;
            _localPosArgs = null;
            _errors = new List<FluentError>();
        }


        /// <summary>
        ///     Indicates whether the current scope has been modified during the resolution process.
        ///     The <c>Dirty</c> property is used to flag the scope when conditions trigger errors, such as exceeding
        ///     the maximum number of allowable placeables or encountering invalid patterns or expressions. This is done
        ///     to signal that this entry will be formatted differently.
        /// </summary>
        internal bool Dirty { get; set; }

        internal short Placeable { get; set; }

        internal short Recursion { get; set; }

        /// <summary>
        ///     Provides access to the collection of <see cref="FluentError" /> instances encountered during processing.
        ///     The <c>Errors</c> property is used to track and retrieve any errors that occur while resolving patterns or
        ///     expressions within the scope.
        /// </summary>
        public IList<FluentError> Errors => _errors;

        /// <summary>
        ///     Provides access to the locally scoped named arguments for the current resolution process.
        ///     The <c>LocalNameArgs</c> property represents a read-only dictionary mapping strings
        ///     to <see cref="IFluentType" /> instances, allowing retrieval of argument values specific to the current scope.
        ///     If no local arguments are set, <c>LocalNameArgs</c> may return <c>null</c>.
        ///     It is utilized primarily for resolving variable references within the scope.
        /// </summary>
        public IReadOnlyDictionary<string, IFluentType>? LocalNameArgs => _localNameArgs;

        /// <summary>
        ///     Represents the list of positional arguments available in the current scope.
        ///     The <c>LocalPosArgs</c> property provides access to an optional, read-only collection
        ///     of <see cref="IFluentType" /> instances that define the positional arguments for the resolution process.
        ///     These arguments can be used during the evaluation of localized messages and functions.
        /// </summary>
        public IReadOnlyList<IFluentType>? LocalPosArgs => _localPosArgs;

        /// <summary>
        ///     Provides access to the arguments passed into the current scope.
        ///     The <c>Args</c> property exposes a read-only dictionary of named arguments, where the key is a string and the value
        ///     implements the <see cref="IFluentType" /> interface.
        ///     It is utilized during message resolution to retrieve or evaluate the values associated with specific variables or
        ///     arguments.
        /// </summary>
        public IReadOnlyDictionary<string, IFluentType>? Args => _args;

        /// <summary>
        ///     Defines a function used to specific instances of <see cref="IFluentType" /> into their string representations.
        /// </summary>
        /// <remarks>
        ///     The <c>FormatterFunc</c> is invoked during runtime resolution to customize the output of domain specific
        ///     <see cref="IFluentType" />
        ///     objects to strings. For example you can define a specific formatter for Dates to ensure they are rendered to
        ///     strings in same way.
        /// </remarks>
        public Func<IFluentType, string>? FormatterFunc { get; }

        /// <summary>
        ///     Represents a transformation function that can modify or process strings within the current scope.
        ///     The <c>TransformFunc</c> property allows custom logic to be applied to all string values, enabling
        ///     functionalities such as text normalization or custom transformations during string resolution and
        ///     formatting.
        /// </summary>
        public Func<string, string>? TransformFunc { get; }


        /// <summary>
        ///     Indicates whether to apply Unicode isolation marks around interpolated content
        ///     during formatting. This property helps prevent unintended interaction between
        ///     bidirectional text or other adjacent content outside the current scope.
        /// </summary>
        public bool UseIsolating { get; init; }

        /// <summary>
        ///     Retrieves the plural category based on the provided rule type and number.
        /// </summary>
        /// <param name="type">The rule type (e.g., Cardinal or Ordinal) for determining the plural category.</param>
        /// <param name="number">The number used to resolve the plural category.</param>
        /// <returns>The plural category corresponding to the given rule type and number.</returns>
        public PluralCategory GetPluralRules(RuleType type, FluentNumber number)
        {
            var specialCase = IsSpecialCase(_culture.Name, type);
            var langStr = GetPluralRuleLang(_culture, specialCase);
            var func = RuleTable.GetPluralFunc(langStr, type);
            return number.TryPluralOperands(out var op) ? func(op) : PluralCategory.Other;
        }

        /// <summary>
        ///     Special language identifier, that goes over 4 ISO language code.
        /// </summary>
        /// <param name="info">language code</param>
        /// <param name="ruleType">Is it ordinal or cardinal rule type.</param>
        /// <returns><c>true</c> when its not standard language code; <c>false</c> otherwise.</returns>
        private static bool IsSpecialCase(string info, RuleType ruleType)
        {
            if (info.Length < 4)
                return false;

            var specialCaseTable = ruleType switch
            {
                RuleType.Ordinal => RuleTable.SpecialCaseOrdinal,
                _ => RuleTable.SpecialCaseCardinal
            };
            return specialCaseTable.Contains(info);
        }

        private static string GetPluralRuleLang(CultureInfo info, bool specialCase)
        {
            if (CultureInfo.InvariantCulture.Equals(info))
                // When culture info is uncertain we default to common 
                // language behavior
                return "root";

            var langStr = specialCase
                ? info.Name.Replace('-', '_')
                : info.TwoLetterISOLanguageName;
            return langStr;
        }

        /// <summary>
        ///     Increments the current count of placeable objects within the scope.
        ///     Ensures the count does not exceed the maximum allowable placeable objects.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the incremented placeable count is within the maximum limit; otherwise, <c>false</c>.
        /// </returns>
        public bool IncrPlaceable()
        {
            return ++Placeable <= MaxPlaceable;
        }

        /// <summary>
        ///     Adds a new error to the <see cref="Scope" /> error collection.
        /// </summary>
        /// <param name="resolverFluentError">The error to be added to the collection.</param>
        public void AddError(ResolverFluentError resolverFluentError)
        {
            _errors.Add(resolverFluentError);
        }

        /// <summary>
        ///     Tracks the evaluation of an expression in a pattern and writes the output to a writer.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> used to write the output.</param>
        /// <param name="pattern">The <see cref="Pattern" /> being evaluated, used to track recursion.</param>
        /// <param name="expr">The <see cref="IExpression" /> to be evaluated and written.</param>
        /// <param name="pos">The position of the current expression in the pattern.</param>
        public void MaybeTrack(TextWriter writer, Pattern pattern, IExpression expr, int pos)
        {
            if (Travelled.Count == 0) Travelled.Add(pattern);

            expr.TryWrite(writer, this, pos);

            if (Dirty)
            {
                writer.Write('{');
                expr.WriteError(writer);
                writer.Write('}');
            }
        }

        internal bool Contains(Pattern pattern)
        {
            return Travelled.Contains(pattern);
        }

        /// <summary>
        ///     Tracks the traversal of a pattern within the scope and writes its resolved output.
        ///     Handles cyclic references by logging errors and writing error output for the expression.
        /// </summary>
        /// <param name="writer">The text writer used to output the resolved pattern or error.</param>
        /// <param name="pattern">The pattern being traversed and resolved.</param>
        /// <param name="exp">The inline expression to write an error message if a cyclic reference is detected.</param>
        public void Track(TextWriter writer, Pattern pattern, IInlineExpression exp)
        {
            if (Travelled.Contains(pattern))
            {
                AddError(ResolverFluentError.Cyclic(pattern));
                writer.Write('{');
                exp.WriteError(writer);
                writer.Write('}');
            }
            else
            {
                Travelled.Add(pattern);
                pattern.Write(writer, this);
                PopTraveled();
            }
        }

        private void PopTraveled()
        {
            if (Travelled.Count > 0) Travelled.RemoveAt(Travelled.Count - 1);
        }

        /// <summary>
        ///     Writes a reference error to the specified <see cref="TextWriter" />.
        ///     Tracks the error and attempts to write an error representation for the
        ///     provided inline expression.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> used to write the error.</param>
        /// <param name="exp">The <see cref="IInlineExpression" /> causing the reference error.</param>
        /// <returns>
        ///     Returns <c>true</c> if the error was successfully written; otherwise, returns <c>false</c>
        ///     if an exception occurred while writing.
        /// </returns>
        public bool WriteRefError(TextWriter writer, IInlineExpression exp)
        {
            AddError(ResolverFluentError.Reference(exp));
            try
            {
                writer.Write('{');
                exp.WriteError(writer);
                writer.Write('}');
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        ///     Attempts to retrieve a reference of type <see cref="FluentReference" /> associated with the specified argument.
        /// </summary>
        /// <param name="argument">The name of the argument to look up.</param>
        /// <param name="reference">
        ///     When this method returns, contains the <see cref="FluentReference" /> associated with the specified argument if the
        ///     argument exists
        ///     and is of the correct type; otherwise, contains <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a reference associated with the specified argument exists and is of type
        ///     <see cref="FluentReference" />;
        ///     otherwise, <c>false</c>.
        /// </returns>
        // [Obsolete("Will be removed in 1.0 release. Use TryResolveReference instead.")]
        public bool TryGetReference(string argument, [NotNullWhen(true)] out FluentReference? reference)
        {
            if (_args != null && _args.TryGetValue(argument, out var fluentType) && fluentType is FluentReference refs)
            {
                reference = refs;
                return true;
            }

            reference = null;
            return false;
        }

        /// <summary>
        ///     Attempts to resolve a reference of type <see cref="FluentReference" /> associated with the specified argument.
        /// </summary>
        /// <param name="argument">The name of the argument to look up.</param>
        /// <param name="reference">
        ///     When this method returns, contains the <see cref="FluentReference" /> associated with the specified argument if the
        ///     argument exists
        ///     and is of the correct type; otherwise, contains <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a reference associated with the specified argument exists and is of type
        ///     <see cref="FluentReference" />;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public bool TryResolveReference(string argument, [NotNullWhen(true)] out IFluentType? value)
        {
            if (_args != null && _args.TryGetValue(argument, out var fluentType)
                              && fluentType is FluentReference refs && _args.TryGetValue(refs.AsString(), out var val))
            {
                value = val;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        ///     Resolves positional and named arguments from the provided call arguments.
        /// </summary>
        /// <param name="callArguments">The call arguments containing positional and named arguments.</param>
        /// <returns>A <see cref="ResolvedArgs" /> object containing the resolved positional and named arguments.</returns>
        public ResolvedArgs GetArguments(CallArguments? callArguments)
        {
            var positionalArgs = new List<IFluentType>();
            var namedArgs = new Dictionary<string, IFluentType>();
            if (callArguments != null)
            {
                var listPositional = callArguments.Value.PositionalArgs;
                for (var i = 0; i < listPositional.Count; i++)
                {
                    var expr = listPositional[i].Resolve(this);
                    positionalArgs.Add(expr);
                }

                var listNamed = callArguments.Value.NamedArgs;
                for (var i = 0; i < listNamed.Count; i++)
                {
                    var arg = listNamed[i];
                    namedArgs.Add(arg.Name.ToString(), arg.Value.Resolve(this));
                }
            }

            return new ResolvedArgs(positionalArgs, namedArgs);
        }

        /// <summary>
        ///     Sets the local named arguments within the <see cref="Scope" />.
        /// </summary>
        /// <param name="resNamed">The dictionary of named arguments to set; if null, clears the local named arguments.</param>
        /// <seealso cref="SetLocalArgs(ResolvedArgs)" />
        public void SetLocalArgs(IDictionary<string, IFluentType>? resNamed)
        {
            _localNameArgs = resNamed != null
                ? new Dictionary<string, IFluentType>(resNamed)
                : null;
        }

        /// <summary>
        ///     Sets the local arguments for the scope, updating named and positional arguments.
        /// </summary>
        /// <param name="resNamed">Resolved arguments containing named and positional arguments to set locally.</param>
        /// <seealso cref="SetLocalArgs(System.Collections.Generic.IDictionary{string,Linguini.Shared.Types.Bundle.IFluentType}?)" />
        public void SetLocalArgs(ResolvedArgs resNamed)
        {
            _localNameArgs = (Dictionary<string, IFluentType>?)resNamed.Named;
            _localPosArgs = (List<IFluentType>?)resNamed.Positional;
        }

        /// <summary>
        ///     Clears local arguments from scope.
        /// </summary>
        public void ClearLocalArgs()
        {
            _localNameArgs = null;
            _localPosArgs = null;
        }

        /// <summary>
        ///     Resolves a reference to a string value based on the provided identifier.
        /// </summary>
        /// <param name="refId">The identifier to resolve into a string reference.</param>
        /// <returns>
        ///     A string representation of the resolved reference, either from local or dynamic context.
        /// </returns>
        public string ResolveReference(Identifier refId)
        {
            var refArg = refId.ToString();
            while (true)
            {
                if (IncrPlaceable() && _args != null && _args.TryGetValue(refArg, out var fluentType))
                {
                    if (fluentType is FluentReference dynamicReference)
                    {
                        refArg = dynamicReference.AsString();
                        continue;
                    }

                    return fluentType.AsString();
                }

                return refArg;
            }
        }
    }

    /// <summary>
    ///     Represents the resolved arguments used within the Fluent localization system.
    ///     It encapsulates positional and named arguments that are extracted and prepared
    ///     for use in runtime operations involving message resolution and formatting.
    /// </summary>
    public record ResolvedArgs(IList<IFluentType> Positional, IDictionary<string, IFluentType> Named);
}