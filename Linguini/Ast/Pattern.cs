using System.Diagnostics.CodeAnalysis;

namespace Linguini.Ast
{
    public enum TextElementPosition : byte
    {
        InitialLineStart,
        LineStart,
        Continuation,
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TextElementTermination : sbyte
    {
        LF,
        CRLF,
        PlaceableStart,
        EndOfFile
    }
    
    public enum TextElementType: sbyte
    {
        Blank,
        NonBlank,
    }

    public interface IPatternElementPlaceholder
    {
    }
    
    public interface IPatternElement
    {
    }

    public class TextElementPlaceholder : IPatternElementPlaceholder
    {
        public int Start { get; }
        public int End { get; }
        public int Indent { get; }
        public TextElementPosition Role { get; }

        public TextElementPlaceholder(int start, int end, int indent, TextElementPosition role)
        {
            Start = start;
            End = end;
            Indent = indent;
            Role = role;
        }
    }

}
