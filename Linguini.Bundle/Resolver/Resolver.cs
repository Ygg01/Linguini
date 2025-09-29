using System;
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
            var writerScope = WriterScope.Create(scope);
            pattern.ResolvePattern(writerScope);
            return writerScope.AsString();
        }


        private static IFluentType ResolvePattern(this Pattern self, WriterScope scope)
        {
            if (self.IsTriviallyResolveable()) self.Elements[0].Resolve(scope);

            return self.ResolvePatternComplex(scope);
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

                        var resolvedPlaceable = placeable.ResolvePlaceable(writingScope);
                        writingScope.Write(resolvedPlaceable);

                        if (needsIsolating) writingScope.Write('\u2069');
                        break;
                    }
                }
            }

            return writingScope.AsString();
        }

        private static IFluentType ResolvePlaceable(this Placeable placeable, WriterScope scope)
        {
            return placeable.Expression switch
            {
                SelectExpression selectExpression => selectExpression.ResolveSelect(scope),
                IInlineExpression inlineExpression => inlineExpression.ResolveInlineExpr(scope),
                _ => FluentNone.None
            };
        }


        private static IFluentType ResolveInlineExpr(this IInlineExpression expr, WriterScope scope)
        {
            return expr switch
            {
                NumberLiteral numberLiteral => numberLiteral.ResolveNumber(scope),
                TextLiteral textLiteral => textLiteral.ResolveText(scope),
                FunctionReference functionReference => functionReference.ResolveFuncRef(scope),
                _ => FluentNone.None
            };
        }

        private static IFluentType ResolveSelect(this SelectExpression selectExpression, WriterScope scope)
        {
            return FluentNone.None;
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

        private static IFluentType Resolve(this VariableReference selectExpression, WriterScope scope)
        {
            return FluentNone.None;
        }

        private static ResolvedArgs ResolveArgs(
            CallArguments? callArguments, WriterScope scope, bool isTerm = false)
        {
            var positionalArgs = new List<IFluentType>();
            var namedArgs = new Dictionary<string, IFluentType>();
            if (callArguments != null)
            {
                var listPositional = callArguments.Value.PositionalArgs;
                for (var i = 0; i < listPositional.Count; i++)
                {
                    var expr = listPositional[i].ResolveInlineExpr(scope);
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

        private static IFluentType Resolve(this IPatternElement self, WriterScope scope)
        {
            return self switch
            {
                TextLiteral text => text.ResolveText(scope),
                Placeable placeable => placeable.ResolvePlaceable(scope),
                _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
            };
        }
    }

    public ref struct WriterScope
    {
        private StringBuilder? writer;
        private Scope scope;
        private int placeableCount;
        private int recursionCount;

        public bool Dirty
        {
            get => scope.Dirty;
            set => scope.Dirty = value;
        }

        public bool UseIsolating => scope.UseIsolating;
        public IReadOnlyList<IFluentType>? LocalPosArgs => scope.LocalPosArgs;

        public static WriterScope Create(Scope scope)
        {
            return new WriterScope
            {
                writer = new StringBuilder(),
                scope = scope
            };
        }

        internal static WriterScope Dummy(Scope scope)
        {
            return new WriterScope
            {
                writer = null,
                scope = scope
            };
        }

        internal void Write(TextLiteral textLiteral)
        {
            writer?.Append(scope.TransformFunc == null
                ? textLiteral.Value
                : scope.TransformFunc(textLiteral.Value.ToString()));
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
            return scope.Contains(pattern);
        }

        internal bool IncrPlaceable()
        {
            return scope.IncrPlaceable();
        }

        internal void Write(IFluentType fluentType)
        {
            writer?.Append(fluentType.AsString());
        }

        internal void AddCyclicError(Pattern pattern)
        {
            scope.AddError(ResolverFluentError.Cyclic(pattern));
        }

        internal void AddTooManyPlaceablesError()
        {
            scope.AddError(ResolverFluentError.TooManyPlaceables(scope.Placeable, scope.MaxPlaceable));
        }

        internal void AddTravelledError(Pattern pattern)
        {
            scope.Travelled.Add(pattern);
        }

        internal void AddReferenceError(FunctionReference functionReference)
        {
            scope.AddError(ResolverFluentError.Reference(functionReference));
        }

        internal bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? func)
        {
            return scope.Bundle.TryGetFunction(id, out func);
        }
    }
}