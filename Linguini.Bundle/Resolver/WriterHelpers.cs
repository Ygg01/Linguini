#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;

namespace Linguini.Bundle.Resolver
{
    public static class WriterHelpers
    {
        public static void Write(this Pattern pattern, TextWriter writer, Scope scope,
            out IList<FluentError> errors)
        {
            var len = pattern.Elements.Count;
            var transformFunc = scope.Bundle.TransformFunc;

            for (var i = 0; i < len; i++)
            {
                if (scope.Dirty)
                {
                    errors = null;
                    return;
                }

                var elem = pattern.Elements[i];

                if (elem.TryConvert(out TextLiteral textLiteral))
                {
                    if (transformFunc != null)
                    {
                        writer.Write(transformFunc(textLiteral.ToString()));
                    }
                    else
                    {
                        writer.Write(textLiteral.Value.Span);
                    }
                }
                else if (elem.TryConvert(out Placeable placeable))
                {
                    var expr = placeable.Expression;
                    if (scope.IncrPlaceable() > scope.Bundle.MaxPlaceable)
                    {
                        scope.Dirty = true;
                        scope.AddError(ResolverFluentError.TooManyPlaceables());
                        errors = scope.Errors;
                        return;
                    }

                    var needsIsolating = scope.Bundle.UseIsolating
                                         && len > 1
                                         && !(expr.TryConvert(out IInlineExpression _)
                                              || expr.TryConvert(out TermReference _)
                                              || expr.TryConvert(out TextLiteral _));

                    if (needsIsolating)
                    {
                        writer.Write('\u2068');
                    }

                    scope.MaybeTrack(writer, pattern, expr, out errors);
                    if (needsIsolating)
                    {
                        writer.Write('\u2069');
                    }
                }
            }

            errors = new List<FluentError>();
        }

        public static bool TryWrite(this IExpression expression, TextWriter writer, Scope scope,
            out IList<FluentError> errors)
        {
            errors = new List<FluentError>();
            if (expression.TryConvert(out IInlineExpression inlineExpression))
            {
                inlineExpression.Write(writer, scope);
            }
            else if (expression.TryConvert(out SelectExpression selectExpression))
            {
                var selector = selectExpression.Selector.Resolve(scope);
                if (selector.TryConvert(out FluentString _)
                    || selector.TryConvert(out FluentNumber _))
                {
                    foreach (var variant in selectExpression.Variants)
                    {
                        IFluentType key;
                        switch (variant.Type)
                        {
                            case VariantType.NumberLiteral:
                                key = FluentNumber.TryNumber(variant.Key.Span);
                                break;
                            default:
                                key = new FluentString(variant.Key.Span);
                                break;
                        }

                        if (key.Matches(selector, scope))
                        {
                            variant.Value.Write(writer, scope, out errors);
                        }
                    }
                }
            }

            return errors.Count == 0;
        }

        public static void Write(this IInlineExpression self, TextWriter writer, Scope scope)
        {
            if (self.TryConvert(out TextLiteral textLiteral))
            {
                writer.Write(textLiteral.Value.Span);
                return;
            }

            if (self.TryConvert(out NumberLiteral numberLiteral))
            {
                writer.Write(FluentNumber.TryNumber(numberLiteral.Value.Span));
                return;
            }

            if (self.TryConvert(out MessageReference msgRef))
            {
                ProcessMsgRef(self, writer, scope, msgRef);
                return;
                ;
            }

            if (self.TryConvert(out TermReference termRef))
            {
                var res = scope.GetArguments(termRef.Arguments);
                var retVal = false;
                scope.SetLocalArgs(res.named);
                if (scope.Bundle.TryGetTerm(termRef.Id.ToString(), out var term))
                {
                    var attrName = termRef.Attribute;
                    var attr = term
                        .Attributes
                        .Find(a => a.Id.Equals(attrName));
                    if (attr != null)
                    {
                        scope.Track(writer, attr.Value, self);
                        retVal = true;
                    }
                    else
                    {
                        scope.Track(writer, term.Value, self);
                    }
                }
                else
                {
                    scope.WriteRefError(writer, self);
                }

                scope.SetLocalArgs(null);
                return;
            }

            if (self.TryConvert(out FunctionReference funcRef))
            {
                var (resolvedPosArgs, resolvedNamedArgs) = scope.GetArguments(funcRef.Arguments);

                if (scope.Bundle.TryGetFunction(funcRef.Id, out var func))
                {
                    var result = func.Function(resolvedPosArgs, resolvedNamedArgs);
                    if (result.IsError())
                    {
                        self.WriteError(writer);
                    }
                    else
                    {
                        writer.Write(result.AsString());
                    }
                }
                else
                {
                    scope.WriteRefError(writer, self);
                }
            }

            if (self.TryConvert(out VariableReference varRef))
            {
                var id = varRef.Id;
                var args = scope.LocalArgs ?? scope.Args;

                if (args != null 
                    && args.TryGetValue(id.ToString(), out var arg))
                {
                    arg.Write(writer, scope);
                }
                else
                {
                    if (scope.LocalArgs == null)
                    {
                        scope.AddError(ResolverFluentError.Reference(self));
                    }

                    writer.Write('{');
                    self.WriteError(writer);
                    writer.Write('}');
                }
            }

            if (self.TryConvert(out Placeable placeable))
            {
                placeable.Expression.TryWrite(writer, scope, out var _);
            }
        }

        public static void Write(this IFluentType self, TextWriter writer, Scope scope)
        {
            if (scope.Bundle.FormatterFunc != null)
            {
                writer.Write(scope.Bundle.FormatterFunc(self));
            }

            writer.Write(self.AsString());
        }

        private static bool ProcessMsgRef(IInlineExpression self, TextWriter writer, Scope scope,
            MessageReference msgRef)
        {
            var id = msgRef.Id;
            var attribute = msgRef.Attribute;

            if (scope.Bundle.TryGetMessage(id.Name.ToString(), out var msg))
            {
                if (attribute != null)
                {
                    var found = msg.Attributes.Find(e => e.Id.Equals(attribute));
                    if (found != null)
                    {
                        scope.Track(writer, found.Value, self);
                    }
                    else
                    {
                        return scope.WriteRefError(writer, self);
                    }
                }
                else
                {
                    if (msg.Value != null)
                    {
                        scope.Track(writer, msg.Value, self);
                    }
                    else
                    {
                        scope.AddError(ResolverFluentError.NoValue(id.Name));
                        writer.Write('{');
                        self.WriteError(writer);
                        writer.Write('}');
                    }
                }
            }
            else
            {
                return scope.WriteRefError(writer, self);
            }

            return true;
        }

        public static void WriteError(this IExpression self, TextWriter writer)
        {
            if (self.TryConvert(out IInlineExpression expr))
            {
                expr.WriteError(writer);
            }

            throw new ArgumentException("Unexpected select expression!");
        }

        public static void WriteError(this IInlineExpression self, TextWriter writer)
        {
            if (self.TryConvert(out MessageReference? msgRef))
            {
                if (msgRef.Attribute == null)
                {
                    writer.Write($"{msgRef.Id}");
                    return;
                }

                writer.Write($"{msgRef.Id}.{msgRef.Attribute}");
                return;
            }
            else if (self.TryConvert(out TermReference? termRef))
            {
                if (termRef.Attribute == null)
                {
                    writer.Write($"-{termRef.Id}");
                    return;
                }

                writer.Write($"-{termRef.Id}.{termRef.Attribute}");
            }
            else if (self.TryConvert(out FunctionReference? funcRef))
            {
                writer.Write($"{funcRef.Id}()");
                return;
            }
            else if (self.TryConvert(out VariableReference? varRef))
            {
                writer.Write($"{varRef.Id}");
                return;
            }
            
            throw new ArgumentException($"Unexpected inline expression `{self.GetType()}`!");
        }
    }
}