using System;
using System.Text;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public static class Resolver
    {
        public static IFluentType ResolvePattern(this Pattern self, Scope scope)
        {
            if (self.IsTriviallyResolveable()) self.Elements[0].Resolve(WriterScope.Dummy(scope));

            return self.ResolvePatternComplex(WriterScope.Create(scope));
        }


        public static IFluentType ResolvePatternComplex(this Pattern pattern, WriterScope writingScope)
        {
            if (writingScope.Contains(pattern))
            {
                writingScope.AddCyclicError(pattern);
                return FluentNone.None;
            }

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

                        var needsIsolating = writingScope.UseIsolating
                                             && pattern.Elements.Count > 1;
                        if (needsIsolating)
                        {
                            writingScope.Write('\u2068');
                            ;
                        }

                        placeable.Resolve(writingScope);
                        if (needsIsolating) writingScope.Write('\u2069');
                        break;
                    }
                }
            }

            return writingScope.AsString();
        }

        public static IFluentType Resolve(this Placeable placeable, WriterScope scope)
        {
            return new FluentErrType();
        }

        private static IFluentType Resolve(this IPatternElement self, WriterScope scope)
        {
            return self switch
            {
                TextLiteral text => text.Resolve(scope),
                Placeable placeable => placeable.Resolve(scope),
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

        public static WriterScope Create(Scope scope)
        {
            return new WriterScope
            {
                writer = new StringBuilder(),
                scope = scope
            };
        }

        public static WriterScope Dummy(Scope scope)
        {
            return new WriterScope
            {
                writer = null,
                scope = scope
            };
        }

        public void Write(TextLiteral textLiteral)
        {
            writer?.Append(scope.TransformFunc == null
                ? textLiteral.Value
                : scope.TransformFunc(textLiteral.Value.ToString()));
            ;
        }

        public void Write(char textLiteral)
        {
            writer?.Append(textLiteral);
        }

        public FluentString AsString()
        {
            return new FluentString(writer?.ToString() ?? string.Empty);
        }

        public bool Contains(Pattern pattern)
        {
            return scope.Contains(pattern);
        }

        public void AddCyclicError(Pattern pattern)
        {
            scope.AddError(ResolverFluentError.Cyclic(pattern));
        }

        public void AddTooManyPlaceablesError()
        {
            scope.AddError(ResolverFluentError.TooManyPlaceables(scope.Placeable, scope.MaxPlaceable));
        }

        public bool IncrPlaceable()
        {
            return scope.IncrPlaceable();
        }
    }
}