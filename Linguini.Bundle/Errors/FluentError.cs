using System;
using Linguini.Shared.Util;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Bundle.Errors
{
    public abstract class FluentError
    {
        public abstract ErrorType ErrorKind();

        public virtual ErrorSpan? GetSpan()
        {
            return null;
        }
    }

    /// <summary>
    /// Record used to pretty print the error if possible
    /// </summary>
    /// <param name="Row">Row in which error occured</param>
    /// <param name="StartSpan">Start of error context</param>
    /// <param name="EndSpan">End of error context</param>
    /// <param name="StartMark">Start of error mark</param>
    /// <param name="EndMark">End of error mark</param>
    public class ErrorSpan
    {
        protected bool Equals(ErrorSpan other)
        {
            return Row == other.Row && StartSpan == other.StartSpan && EndSpan == other.EndSpan && StartMark == other.StartMark && EndMark == other.EndMark;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ErrorSpan)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, StartSpan, EndSpan, StartMark, EndMark);
        }

        public static bool operator ==(ErrorSpan? left, ErrorSpan? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ErrorSpan? left, ErrorSpan? right)
        {
            return !Equals(left, right);
        }

        public int Row;
        public int StartSpan;
        public int EndSpan;
        public int StartMark;
        public int EndMark;

        public ErrorSpan(int row, int startSpan, int endSpan, int startMark, int endMark)
        {
            Row = row;
            StartSpan = startSpan;
            EndSpan = endSpan;
            StartMark = startMark;
            EndMark = endMark;
        }

        public override string ToString()
        {
            return $"ErrorSpan (Row = {Row}, StartSpan = {StartSpan}, EndSpan = {EndSpan}," +
                   $" StartSpan = {StartMark}, StartSpan = {EndMark})";
        }
    }

    public class OverrideFluentError : FluentError
    {
        private readonly string _id;
        private readonly EntryKind _kind;

        public OverrideFluentError(string id, EntryKind kind)
        {
            _id = id;
            _kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return ErrorType.Overriding;
        }

        public override string ToString()
        {
            return $"For id:{_id} already exist entry of type: {_kind.ToString()}";
        }
    }

    public class ResolverFluentError : FluentError
    {
        private string Description;
        private ErrorType Kind;

        private ResolverFluentError(string desc, ErrorType kind)
        {
            Description = desc;
            Kind = kind;
        }

        public override ErrorType ErrorKind()
        {
            return Kind;
        }

        public override string ToString()
        {
            return Description;
        }

        public static ResolverFluentError NoValue(ReadOnlyMemory<char> idName)
        {
            return new ResolverFluentError($"No value: {idName.Span.ToString()}", ErrorType.NoValue);
        }

        public static ResolverFluentError NoValue(string pattern)
        {
            return new ResolverFluentError($"No value: {pattern}", ErrorType.NoValue);
        }

        public static ResolverFluentError UnknownVariable(VariableReference outType)
        {
            return new ResolverFluentError($"Unknown variable: ${outType.Id}", ErrorType.Reference);
        }

        public static ResolverFluentError TooManyPlaceables()
        {
            return new ResolverFluentError("Too many placeables", ErrorType.TooManyPlaceables);
        }

        public static ResolverFluentError Reference(IInlineExpression self)
        {
            // TODO only allow references here
            if (self is FunctionReference funcRef)
            {
                return new ResolverFluentError($"Unknown function: {funcRef.Id}()", ErrorType.Reference);
            }

            if (self is MessageReference msgRef)
            {
                if (msgRef.Attribute == null)
                {
                    return new ResolverFluentError($"Unknown message: {msgRef.Id}", ErrorType.Reference);
                }

                return new ResolverFluentError($"Unknown attribute: {msgRef.Id}.{msgRef.Attribute}", ErrorType.Reference);
            }

            if (self is TermReference termReference)
            {
                if (termReference.Attribute == null)
                {
                    return new ResolverFluentError($"Unknown term: -{termReference.Id}", ErrorType.Reference);
                }

                return new ResolverFluentError($"Unknown attribute: -{termReference.Id}.{termReference.Attribute}", ErrorType.Reference);
            }

            if (self is VariableReference varRef)
            {
                return new ResolverFluentError($"Unknown variable: ${varRef.Id}", ErrorType.Reference);
            }

            throw new ArgumentException($"Expected reference got ${self.GetType()}");
        }

        public static ResolverFluentError Cyclic(Pattern pattern)
        {
            return new ResolverFluentError($"Cyclic reference in {pattern.Stringify()} detected!", ErrorType.Cyclic);
        }

        public static ResolverFluentError MissingDefault()
        {
            return new ResolverFluentError("No default", ErrorType.MissingDefault);
        }
    }

    public class ParserFluentError : FluentError
    {
        private readonly ParseError _error;

        private ParserFluentError(ParseError error)
        {
            _error = error;
        }

        public static ParserFluentError ParseError(ParseError parseError)
        {
            return new ParserFluentError(parseError);
        }

        public override ErrorType ErrorKind()
        {
            return ErrorType.Parser;
        }

        public override string ToString()
        {
            return _error.Message;
        }

        public override ErrorSpan? GetSpan()
        {
            if (_error.Slice == null)
                return null;

            return new ErrorSpan(_error.Row, _error.Slice.Value.Start.Value, _error.Slice.Value.End.Value,
                _error.Position.Start.Value, _error.Position.End.Value);
        }
    }

    public enum EntryKind : byte
    {
        Message,
        Term,
        Unknown,
    }

    public static class EntryHelper
    {
        public static EntryKind ToKind(this IEntry self)
        {
            if (self is AstTerm)
            {
                return EntryKind.Term;
            }

            return self is AstMessage ? EntryKind.Message : EntryKind.Unknown;
        }
    }

    public enum ErrorType : byte
    {
        Parser,
        Reference,
        Cyclic,
        NoValue,
        TooManyPlaceables,
        Overriding,
        MissingDefault
    }
}
