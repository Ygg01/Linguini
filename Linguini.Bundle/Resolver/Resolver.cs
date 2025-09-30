using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public static class Resolver
    {
        public static string FormatPattern(this Pattern pattern, Scope scope)
        {
            var x = pattern.ResolvePattern(scope);
            return x.AsString();
        }

        private static IFluentType ResolvePattern(this Pattern self, Scope scope)
        {
            var scopeWriter = WriterScope.Create(scope);
            if (self.IsTriviallyResolveable()) self.Elements[0].Resolve(scopeWriter);

            return self.ResolvePatternComplex(scopeWriter);
        }

        private static IFluentType ResolvePatternComplex(this Pattern pattern, WriterScope writingScope)
        {
            if (writingScope.Contains(pattern))
            {
                writingScope.AddCyclicError(pattern);
                return FluentNone.None;
            }

            writingScope.AddTravelledError(pattern);
            var needsIsolating = writingScope.UseIsolating
                                 && pattern.Elements.Count > 1;

            foreach (var element in pattern.Elements)
            {
                if (writingScope.Dirty)
                    return new FluentErrType();

                switch (element)
                {
                    case TextLiteral textLiteral:
                        writingScope.Write(textLiteral);
                        break;
                    case Placeable placeable:
                    {
                        if (!writingScope.IncrPlaceable())
                        {
                            writingScope.Dirty = true;
                            writingScope.AddTooManyPlaceablesError();
                            return new FluentErrType();
                        }


                        if (needsIsolating) writingScope.Write('\u2068');

                        var plc = placeable.ResolvePlaceable(writingScope);
                        writingScope.Write(plc);

                        if (needsIsolating) writingScope.Write('\u2069');
                        break;
                    }
                }
            }

            return writingScope.AsString();
        }

        // TODO use Scope instead of WriterScope
        private static IFluentType ResolvePlaceable(this Placeable placeable, WriterScope scope)
        {
            return placeable.Expression switch
            {
                SelectExpression selectExpression => selectExpression.ResolveSelect(scope),
                IInlineExpression inlineExpression => inlineExpression.ResolveInlineExpr(scope),
                _ => FluentNone.None
            };
        }

        private static IFluentType ResolveSelect(this SelectExpression selectExpression, WriterScope writerScope)
        {
            var selector = selectExpression.Selector.ResolveInlineExpr(WriterScope.Dummy(writerScope));
            var retVal = selectExpression.GetDefault(writerScope);

            foreach (var variant in selectExpression.Variants)
            {
                IFluentType key;
                switch (variant.Type)
                {
                    case VariantType.NumberLiteral:
                        key = FluentNumber.TryNumber(variant.Key.Span);
                        break;
                    case VariantType.Identifier:
                    default:
                        key = new FluentString(variant.Key.Span);
                        break;
                }

                if (key.Matches(selector, writerScope.Scope))
                {
                    retVal = variant;
                    break;
                }
            }

            return retVal?.Value.ResolvePattern(writerScope.Scope) ?? new FluentErrType();
        }

        private static Variant? GetDefault(this SelectExpression selectExpression, WriterScope writerScope)
        {
            foreach (var variant in selectExpression.Variants)
                if (variant.IsDefault)
                    return variant;

            writerScope.AddMissingDefaultError();
            return null;
        }

        private static IFluentType ResolveInlineExpr(this IInlineExpression expr, WriterScope scope,
            IFluentType? localPosArg = null)
        {
            return expr switch
            {
                NumberLiteral numberLiteral => numberLiteral.ResolveNumber(scope),
                TextLiteral textLiteral => textLiteral.ResolveText(scope),
                FunctionReference functionReference => functionReference.ResolveFuncRef(scope),
                VariableReference variableReference => variableReference.ResolveVarRef(scope, localPosArg),
                TermReference termReference => termReference.ResolveTermRef(scope),
                DynamicReference dynamicReference => dynamicReference.ResolveDynamicRef(scope),
                _ => FluentNone.None
            };
        }


        private static IFluentType ResolveText(this TextLiteral self, WriterScope scope)
        {
            return new FluentString(self.Value.Span);
        }

        private static IFluentType ResolveNumber(this NumberLiteral numberLiteral, WriterScope scope)
        {
            return FluentNumber.TryNumber(numberLiteral.Value.Span);
        }

        private static IFluentType ResolveFuncRef(this FunctionReference funcRef, WriterScope scope)
        {
            var (resolvedPosArgs, resolvedNamedArgs) = ResolveArgs(funcRef.Arguments, scope);

            if (scope.TryGetFunction(funcRef.Id, out var func))
                return func.Function(resolvedPosArgs, resolvedNamedArgs);

            scope.AddReferenceError(funcRef);
            return new FluentErrType();
        }

        private static IFluentType ResolveVarRef(this VariableReference varRef, WriterScope scope,
            IFluentType? localContextArg = null)
        {
            var args = scope.LocalNameArgs ?? scope.Args;
            if (args != null
                && args.TryGetValue(varRef.Id.ToString(), out var arg))
                return arg.Copy();

            if (localContextArg != null)
                return localContextArg;

            if (scope.LocalNameArgs == null) scope.AddUnknownVariableError(varRef);

            return new FluentErrType();
        }

        private static IFluentType ResolveTermRef(this TermReference termRef, WriterScope scope)
        {
            if (!scope.EnableExtensions || !scope.TryGetAstTerm(termRef.Id.ToString(), out var term))
                return new FluentErrType();

            if (termRef.Attribute == null)
                return term.Value.ResolvePattern(scope.Scope);

            foreach (var arg in term.Attributes)
                if (termRef.Attribute.Equals(arg.Id) && arg.Value.Elements.Count == 1)
                    return arg.Value.ResolvePattern(scope.Scope);

            return new FluentErrType();
        }


        private static IFluentType ResolveDynamicRef(this DynamicReference dynRef, WriterScope scope)
        {
            if (!scope.EnableExtensions || !scope.TryGetReference(dynRef.Id.ToString(), out var fr) ||
                !scope.TryGetTermOrMessage(fr, out var actualRef))
                return new FluentErrType();

            if (dynRef.Attribute == null && actualRef.pattern != null)
                return actualRef.pattern.ResolvePattern(scope.Scope);

            foreach (var arg in actualRef.attributes)
                if (dynRef.Attribute != null && dynRef.Attribute.Equals(arg.Id) && arg.Value.Elements.Count == 1)
                    return arg.Value.ResolvePattern(scope.Scope);

            return new FluentErrType();
        }

        private static ResolvedArgs ResolveArgs(CallArguments? callArguments, WriterScope scope, bool isTerm = false)
        {
            var positionalArgs = new List<IFluentType>();
            var namedArgs = new Dictionary<string, IFluentType>();
            if (callArguments != null)
            {
                var listPositional = callArguments.Value.PositionalArgs;
                for (var i = 0; i < listPositional.Count; i++)
                {
                    var locaPosArg = scope.LocalPosArgs?.GetAt(i);
                    var expr = listPositional[i].ResolveInlineExpr(scope, locaPosArg);
                    positionalArgs.Add(expr);
                }

                var listNamed = callArguments.Value.NamedArgs;
                for (var i = 0; i < listNamed.Count; i++)
                {
                    var arg = listNamed[i];
                    namedArgs.Add(arg.Name.ToString(), arg.Value.ResolveInlineExpr(scope));
                }
            }

            return new ResolvedArgs(positionalArgs,
                namedArgs);
        }

        private static IFluentType? GetAt(this IReadOnlyList<IFluentType> list, int index)
        {
            if (index < 0 || index >= list.Count)
                return null;
            return list[index];
        }

        private static IFluentType Resolve(this IPatternElement self, WriterScope scope)
        {
            return self switch
            {
                TextLiteral text => text.ResolveText(scope),
                Placeable placeable => placeable.ResolvePlaceable(scope),
                _ => new FluentErrType()
            };
        }
    }

    public ref struct WriterScope
    {
        private StringBuilder? writer;
        private int placeableCount;
        private int recursionCount;

        public bool Dirty
        {
            get => Scope.Dirty;
            set => Scope.Dirty = value;
        }

        internal bool UseIsolating => Scope.UseIsolating;
        internal IReadOnlyList<IFluentType>? LocalPosArgs => Scope.LocalPosArgs;
        internal IReadOnlyDictionary<string, IFluentType>? LocalNameArgs => Scope.LocalNameArgs;
        internal IReadOnlyDictionary<string, IFluentType>? Args => Scope.Args;

        internal Scope Scope { get; private set; }

        internal bool EnableExtensions => Scope.Bundle.EnableExtensions;

        public static WriterScope Create(Scope scope)
        {
            return new WriterScope
            {
                writer = new StringBuilder(),
                Scope = scope
            };
        }

        internal static WriterScope Dummy(WriterScope writerScope)
        {
            return new WriterScope
            {
                writer = null,
                Scope = writerScope.Scope
            };
        }

        internal void Write(TextLiteral textLiteral)
        {
            writer?.Append(Scope.TransformFunc == null
                ? textLiteral.Value
                : Scope.TransformFunc(textLiteral.Value.ToString()));
            ;
        }

        internal void Write(char textLiteral)
        {
            writer?.Append(textLiteral);
        }

        internal FluentString AsString()
        {
            return new FluentString(writer?.ToString() ?? string.Empty);
        }

        internal bool Contains(Pattern pattern)
        {
            return Scope.Contains(pattern);
        }

        internal bool IncrPlaceable()
        {
            return Scope.IncrPlaceable();
        }

        internal void Write(IFluentType fluentType)
        {
            writer?.Append(fluentType.AsString());
        }

        internal void AddCyclicError(Pattern pattern)
        {
            Scope.AddError(ResolverFluentError.Cyclic(pattern));
        }

        internal void AddTooManyPlaceablesError()
        {
            Scope.AddError(ResolverFluentError.TooManyPlaceables(Scope.Placeable, Scope.MaxPlaceable));
        }

        internal void AddTravelledError(Pattern pattern)
        {
            Scope.Travelled.Add(pattern);
        }

        internal void AddReferenceError(FunctionReference functionReference)
        {
            Scope.AddError(ResolverFluentError.Reference(functionReference));
        }

        internal bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? func)
        {
            return Scope.Bundle.TryGetFunction(id, out func);
        }

        internal void AddUnknownVariableError(VariableReference varRef)
        {
            Scope.AddError(ResolverFluentError.UnknownVariable(varRef));
        }

        internal void AddMissingDefaultError()
        {
            Scope.AddError(ResolverFluentError.MissingDefault());
        }

        internal bool TryGetAstTerm(string ident, [NotNullWhen(true)] out AstTerm? term)
        {
            return Scope.Bundle.TryGetAstTerm(ident, out term);
        }

        internal bool TryGetAstMessage(string ident, [NotNullWhen(true)] out AstMessage? term)
        {
            return Scope.Bundle.TryGetAstMessage(ident, out term);
        }

        internal bool TryGetTermOrMessage(string indent, out (Pattern? pattern, List<Attribute> attributes) retVal)
        {
            if (Scope.Bundle.TryGetAstTerm(indent, out var term))
            {
                retVal = new(term.Value, term.Attributes);
                return true;
            }

            if (Scope.Bundle.TryGetAstMessage(indent, out var message))
            {
                retVal = new(message.Value, message.Attributes);
                return true;
            }

            retVal = default;
            return false;
        }

        public bool TryGetReference(string ident, [NotNullWhen(true)] out FluentReference? fr)
        {
            return Scope.TryGetReference(ident, out fr);
        }
    }
}