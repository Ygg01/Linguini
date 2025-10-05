using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        internal readonly Dictionary<string, IFluentType>? _args;
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
        internal readonly List<Pattern> Travelled;

        internal Dictionary<string, IFluentType>? _localNameArgs;
        internal List<IFluentType>? _localPosArgs;


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
            {
                return false;
            }

            var specialCaseTable = ruleType switch
            {
                RuleType.Ordinal => RuleTable.SpecialCaseOrdinal,
                _                => RuleTable.SpecialCaseCardinal
            };
            return specialCaseTable.Contains(info);
        }

        private static string GetPluralRuleLang(CultureInfo info, bool specialCase)
        {
            if (CultureInfo.InvariantCulture.Equals(info))
                // When culture info is uncertain we default to common 
                // language behavior
            {
                return "root";
            }

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

        internal bool Contains(Pattern pattern)
        {
            return Travelled.Contains(pattern);
        }

        internal void PopTraveled()
        {
            if (Travelled.Count > 0)
            {
                Travelled.RemoveAt(Travelled.Count - 1);
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
            if (_args != null && _args.TryGetValue(argument, out var fluentType))
            {
                switch (fluentType)
                {
                    case FluentReference refs:
                        reference = refs;
                        return true;
                    case FluentString fs:
                        reference = new FluentReference(fs);
                        return true;
                }
            }

            reference = null;
            return false;
        }

        /// <summary>
        ///     Sets the local arguments for the scope, updating named and positional arguments.
        /// </summary>
        /// <param name="resNamed">Resolved arguments containing named and positional arguments to set locally.</param>
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
    }

    /// <summary>
    ///     Provides methods to handle pluralization rules for different languages and cultures.
    ///     This includes determining the plural category of a number based on the given culture
    ///     and rule type (e.g., cardinal or ordinal), as well as handling special cases for specific locales.
    /// </summary>
    public static class PluralRules
    {
        /// <summary>Determines the plural category of a given number based on the specified culture and rule type.</summary>
        /// <param name="info">The CultureInfo representing the cultural context to determine the plural category.</param>
        /// <param name="ruleType">The type of pluralization rule (e.g., Cardinal or Ordinal) to apply.</param>
        /// <param name="number">The FluentNumber to evaluate for determining the plural category.</param>
        /// <return>A PluralCategory enumerating the plural classification of the provided number.</return>
        public static PluralCategory GetPluralCategory(CultureInfo info, RuleType ruleType, FluentNumber number)
        {
            var specialCase = IsSpecialCase(info.Name, ruleType);
            var langStr = GetPluralRuleLang(info, specialCase);
            var func = RuleTable.GetPluralFunc(langStr, ruleType);
            if (number.TryPluralOperands(out var op))
            {
                return func(op);
            }

            return PluralCategory.Other;
        }

        /// <summary>
        ///     Special language identifier, that goes over 4 ISO language code.
        /// </summary>
        /// <param name="info">language code</param>
        /// <param name="ruleType">Is it ordinal or cardinal rule type.</param>
        /// <returns><c>true</c> when its not standard language code; <c>false</c> otherwise.</returns>
        public static bool IsSpecialCase(string info, RuleType ruleType)
        {
            if (info.Length < 4)
            {
                return false;
            }

            var specialCaseTable = ruleType switch
            {
                RuleType.Ordinal => RuleTable.SpecialCaseOrdinal,
                _                => RuleTable.SpecialCaseCardinal
            };
            return specialCaseTable.Contains(info);
        }

        private static string GetPluralRuleLang(CultureInfo info, bool specialCase)
        {
            if (CultureInfo.InvariantCulture.Equals(info))
                // When culture info is uncertain we default to common 
                // language behavior
            {
                return "root";
            }

            var langStr = specialCase
                ? info.Name.Replace('-', '_')
                : info.TwoLetterISOLanguageName;
            return langStr;
        }
    }

    /// <summary>
    ///     Represents the resolved arguments used within the Fluent localization system.
    ///     It encapsulates positional and named arguments that are extracted and prepared
    ///     for use in runtime operations involving message resolution and formatting.
    /// </summary>
    public record ResolvedArgs(IList<IFluentType> Positional, IDictionary<string, IFluentType> Named);
}