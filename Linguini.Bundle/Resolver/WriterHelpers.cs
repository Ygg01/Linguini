#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Entry;
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
            throw new NotImplementedException();
        }

        public static bool TryWrite(this IExpression expression, TextWriter writer, Scope scope,
            out IList<FluentError> errors)
        {
            if (expression.TryConvert(out IInlineExpression inlineExpression))
            {
                inlineExpression.TryWrite(writer, scope, out errors);
            }
            else if (expression.TryConvert(out SelectExpression selectExpression))
            {
                var selector = selectExpression.Selector.Resolve(scope);
            }

            // TODO
            throw new NotImplementedException();
        }

        public static bool TryWrite(this IInlineExpression self, TextWriter writer, Scope scope,
            out IList<FluentError> errors)
        {
            errors = new List<FluentError>();
            if (self.TryConvert(out TextLiteral textLiteral))
            {
                writer.Write(textLiteral.Value.Span);
                return true;
            }

            if (self.TryConvert(out NumberLiteral numberLiteral))
            {
                writer.Write(FluentNumber.TryNumber(numberLiteral.Value.Span));
                return true;
            }

            if (self.TryConvert(out MessageReference msgRef))
            {
                return ProcessMsgRef(self, writer, scope, msgRef);;
            }

            if (self.TryConvert(out TermReference termRef))
            {
                var res = scope.GetArguments(termRef.Arguments);
                var retVal = false;
                scope.SetLocalArgs(res.named);
                if (scope.Bundle.TryGetTerm(termRef.Id.ToString(), out var term))
                {
                    var attr = term.Attributes.Find(a => a.Id.Equals(termRef.Attribute));
                    if (attr != null)
                    {
                        scope.Track(writer, attr.Value, self);
                        retVal = true;
                    }
                }
                scope.SetLocalArgs(null);
                return retVal;
            }

            // TODO
            throw new NotImplementedException();
        }

        private static bool ProcessMsgRef(IInlineExpression self, TextWriter writer, Scope scope, MessageReference msgRef)
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
            // TODO
            throw new NotImplementedException();
        }
        
        public static void WriteError(this IInlineExpression self, TextWriter writer)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
