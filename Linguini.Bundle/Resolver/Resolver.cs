using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using Linguini.Shared.Util;
using Linguini.Syntax.Ast;
using ArgumentException = System.ArgumentException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace Linguini.Bundle.Resolver
{
    public static class Resolver
    {
        public static string FormatPattern(this Pattern pattern, Scope scope)
        {
            return pattern.ResolvePattern(scope).AsString();
        }

        private static IFluentType ResolvePattern(this Pattern pattern, Scope scope)
        {
            return pattern.ResolvePatternInternal(WriterScope.FromScope(scope));
        }

        private static IFluentType ResolvePattern(this Pattern pattern, WriterScope scope)
        {
            return pattern.ResolvePatternInternal(WriterScope.CreateTermScope(scope));
        }

        private static IFluentType ResolvePatternInternal(this Pattern pattern, WriterScope writingScope)
        {
            if (writingScope.Contains(pattern))
            {
                return writingScope.AddCyclicError(pattern);;
            }

            writingScope.AddTravelledPattern(pattern);
            var needsIsolating = writingScope.UseIsolating
                                 && pattern.Elements.Count > 1;

            if (pattern.Elements.Count == 1)
                return pattern.Elements[0] switch
                {
                    TextLiteral textLiteral => writingScope.Write(textLiteral),
                    Placeable placeable => placeable.ResolvePlaceable(writingScope),
                    _ => throw new ArgumentOutOfRangeException()
                };

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
            var selector = selectExpression.Selector.ResolveInlineExpr(WriterScope.Dummy(writerScope), FluentNone.None);
            if (selector is FluentNone && !writerScope.IsTermScoped && selectExpression.Selector is not DynamicReference)
            {
                writerScope.UnknownVariable(selectExpression.Selector);
            }
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

            return retVal?.Value.ResolvePattern(writerScope) ?? new FluentErrType();
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
            scope.PrevExpression = expr;
            return expr switch
            {
                NumberLiteral numberLiteral => numberLiteral.ResolveNumber(),
                TextLiteral textLiteral => textLiteral.ResolveText(),
                FunctionReference functionReference => functionReference.ResolveFuncRef(scope),
                VariableReference variableReference => variableReference.ResolveVarRef(scope, localPosArg),
                TermReference termReference => termReference.ResolveTermRef(scope),
                MessageReference messageReference => messageReference.ResolveMessageRef(scope),
                DynamicReference dynamicReference when scope.EnableExtensions => dynamicReference.ResolveDynamicRef(scope, localPosArg),
                Placeable placeable => placeable.ResolvePlaceable(scope),
                _ => throw new ArgumentException($"Unexpected expression! {expr}")
            };
        }

        private static IFluentType ResolveText(this TextLiteral self)
        {
            var str = new StringWriter();
            UnicodeUtil.WriteUnescapedUnicode(self.Value, str);
            return new FluentString(str.ToString());
        }

        private static IFluentType ResolveNumber(this NumberLiteral numberLiteral)
        {
            return FluentNumber.TryNumber(numberLiteral.Value.Span);
        }

        private static IFluentType ResolveFuncRef(this FunctionReference funcRef, WriterScope scope)
        {
            var (resolvedPosArgs, resolvedNamedArgs) = ResolveArgs(funcRef.Arguments, scope);

            if (scope.TryGetFunction(funcRef.Id, out var func))
                return func.Function(resolvedPosArgs, resolvedNamedArgs);

            return scope.AddReferenceError(funcRef);
        }

        private static IFluentType ResolveVarRef(this VariableReference varRef, WriterScope scope,
            IFluentType? localContextArg = null)
        {
            var args = scope.Scope._localNameArgs ?? scope.Scope._args;
            if (args != null
                && args.TryGetValue(varRef.Id.ToString(), out var arg))
                return arg.Copy();

            if (localContextArg != null)
                return localContextArg;

            return scope.AddReferenceError(varRef);
        }

        private static IFluentType ResolveTermRef(this TermReference termRef, WriterScope scope)
        {
            if (!scope.TryGetAstTerm(termRef.Id.ToString(), out var term))
                return scope.AddReferenceError(termRef);

            var (pos, named) = ResolveArgs(termRef.Arguments, scope);
            scope.Scope._localNameArgs = (Dictionary<string, IFluentType>?)named;
            scope.Scope._localPosArgs = (List<IFluentType>?)pos;

            if (termRef.Attribute == null)
                return term.Value.ResolvePattern(WriterScope.CreateTermScope(scope));

            foreach (var arg in term.Attributes)
                if (termRef.Attribute.Equals(arg.Id))
                    return arg.Value.ResolvePattern(WriterScope.CreateTermScope(scope));

            return scope.AddReferenceError(termRef);
        }

        private static IFluentType ResolveMessageRef(this MessageReference messageRef, WriterScope scope)
        {
            if (!scope.TryGetAstMessage(messageRef.Id.ToString(), out var message))
                return scope.AddReferenceError(messageRef);

            if (messageRef.Attribute == null)
                return message.Value == null
                    ? scope.AddNoValueError(messageRef.Id)
                    : message.Value.ResolvePattern(scope);

            foreach (var arg in message.Attributes)
                if (messageRef.Attribute.Equals(arg.Id))
                    return arg.Value.ResolvePattern(scope);

            return scope.AddReferenceError(messageRef);
        }

        private static IFluentType ResolveDynamicRef(this DynamicReference dynRef, WriterScope scope,
            IFluentType? localContextArg = null)
        {
            if (!scope.EnableExtensions || !scope.TryGetReference(dynRef.Id.ToString(), out var fr) ||
                !scope.TryGetTermOrMessage(fr, out var actualRef))
                return new FluentErrType();

            if (dynRef.Attribute == null && actualRef.pattern != null)
                return actualRef.pattern.ResolvePattern(scope);

            foreach (var arg in actualRef.attributes)
                if (dynRef.Attribute != null && dynRef.Attribute.Equals(arg.Id) && arg.Value.Elements.Count == 1)
                    return arg.Value.ResolvePattern(scope);

            return localContextArg ?? scope.AddReferenceError(dynRef);
        }

        private static ResolvedArgs ResolveArgs(CallArguments? callArguments, WriterScope scope)
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
    }

    public ref struct WriterScope
    {
        private StringBuilder? _writer;
        internal bool IsTermScoped;
        internal IInlineExpression? PrevExpression;

        internal bool Dirty
        {
            get => Scope.Dirty;
            set => Scope.Dirty = value;
        }

        internal bool UseIsolating => Scope.UseIsolating;
        internal IReadOnlyList<IFluentType>? LocalPosArgs => Scope.LocalPosArgs;


        internal Scope Scope { get; private set; }

        internal bool EnableExtensions => Scope.Bundle.EnableExtensions;

        public static WriterScope CreateTermScope(WriterScope scope)
        {
            return new WriterScope
            {
                _writer = new StringBuilder(),
                Scope = scope.Scope,
                PrevExpression = scope.PrevExpression,
                IsTermScoped = true
            };
        }

        internal static WriterScope Dummy(WriterScope writerScope)
        {
            return new WriterScope
            {
                _writer = null,
                Scope = writerScope.Scope,
                PrevExpression = writerScope.PrevExpression,
                IsTermScoped = writerScope.IsTermScoped
            };
        }
        
        
        internal static WriterScope FromScope(Scope scope)
        {
            return new WriterScope
            {
                _writer = new StringBuilder(),
                Scope = scope,
                IsTermScoped = false,
                PrevExpression = null,
            };
        }

        internal IFluentType Write(TextLiteral textLiteral)
        {
            var str = Scope.TransformFunc == null
                ? textLiteral.Value.ToString()
                : Scope.TransformFunc(textLiteral.Value.ToString()); 
            _writer?.Append(str);
            return new FluentString(str);
        }

        internal void Write(char textLiteral)
        {
            _writer?.Append(textLiteral);
        }

        internal FluentString AsString()
        {
            return new FluentString(_writer?.ToString() ?? string.Empty);
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
            _writer?.Append(fluentType.AsString());
        }

        internal IFluentType AddCyclicError(Pattern pattern)
        {
            Scope.AddError(ResolverFluentError.Cyclic(pattern));
            var _lastExpr = PrevExpression == null ? "???" : PrevExpression.ToString();
            return new FluentErrType($"{{{_lastExpr}}}");
        }

        internal void AddTooManyPlaceablesError()
        {
            Scope.AddError(ResolverFluentError.TooManyPlaceables(Scope.Placeable, Scope.MaxPlaceable));
        }

        internal void AddTravelledPattern(Pattern pattern)
        {
            Scope.Travelled.Add(pattern);
        }

        internal FluentErrType AddReferenceError(IInlineExpression reference)
        {
            if (!IsTermScoped) Scope.AddError(ResolverFluentError.Reference(reference));
            return new FluentErrType($"{{{reference}}}");
        }
        
        internal void UnknownVariable(IInlineExpression reference)
        {
            Scope.AddError(ResolverFluentError.Reference(reference));
        }

        internal bool TryGetFunction(Identifier id, [NotNullWhen(true)] out FluentFunction? func)
        {
            return Scope.Bundle.TryGetFunction(id, out func);
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
                retVal = new System.ValueTuple<Pattern?, List<Attribute>>(term.Value, term.Attributes);
                return true;
            }

            if (Scope.Bundle.TryGetAstMessage(indent, out var message))
            {
                retVal = new System.ValueTuple<Pattern?, List<Attribute>>(message.Value, message.Attributes);
                return true;
            }

            retVal = default;
            return false;
        }

        internal bool TryGetReference(string ident, [NotNullWhen(true)] out FluentReference? fr)
        {
            return Scope.TryGetReference(ident, out fr);
        }

        internal IFluentType AddNoValueError(Identifier id)
        {
            Scope.AddError(ResolverFluentError.NoValue(id.Name));
            return new FluentErrType($"{{{id}}}");
        }

    }
}