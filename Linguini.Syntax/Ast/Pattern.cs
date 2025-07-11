using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Linguini.Syntax.Ast
{
    /// <summary>
    /// Shows which position text element takes.
    /// </summary>
    public enum TextElementPosition : byte
    {
        /// <summary>
        /// Element at start of the first line.
        /// </summary>
        InitialLineStart,
        
        /// <summary>
        /// Text element at line start.
        /// </summary>
        LineStart,
        
        /// <summary>
        /// Text element continues on line.
        /// </summary>
        Continuation,
    }

    /// <summary>
    /// Determines how text elements ends.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TextElementTermination : byte
    {
        /// <summary>
        /// Newline text termination.
        /// </summary>
        LF,
        /// <summary>
        /// Carriage and Newline text termination.
        /// </summary>
        CRLF,
        /// <summary>
        /// Placeable ends text.
        /// </summary>
        PlaceableStart,
        
        /// <summary>
        /// End of file text termination.
        /// </summary>
        EndOfFile
    }

    /// <summary>
    /// Determines if text element is blank or not.
    /// </summary>
    public enum TextElementType : byte
    {
        /// <summary>
        /// Blank text element.
        /// </summary>
        Blank,
        
        /// <summary>
        /// Non-blank text element.
        /// </summary>
        NonBlank,
    }

    /// <summary>
    /// Common interface for Placeable text
    /// </summary>
    /// <seealso cref="TextElementPlaceholder"/>
    /// <seealso cref="Placeable"/>
    public interface IPatternElementPlaceholder
    {
    }

    /// <summary>
    /// Interface for elements used in <see cref="Pattern"/>
    /// </summary>
    public interface IPatternElement
    {
        /// <summary>
        /// Provides a default instance of <see cref="PatternComparer"/> for comparing
        /// two <see cref="Pattern"/> instances based on their equality logic.
        /// </summary>
        public static PatternComparer PatternComparer = new();
    }

    /// <inheritdoc />
    public class PatternComparer : IEqualityComparer<IPatternElement>
    {
        /// <inheritdoc />
        public bool Equals(IPatternElement? left, IPatternElement? right)
        {
            return (left, right) switch
            {
                (TextLiteral l, TextLiteral r) => l.Equals(r),
                (Placeable l, Placeable r) => l.Equals(r),
                _ => false,
            };
        }

        /// <inheritdoc />
        public int GetHashCode(IPatternElement obj)
        {
            switch (obj)
            {
                case TextLiteral textLiteral:
                    return textLiteral.GetHashCode();
                case Placeable placeable:
                    return placeable.GetHashCode();
                default:
                    throw new ArgumentException("Unexpected type", nameof(obj));
            }
        }
    }

    /// <inheritdoc />
    public class TextElementPlaceholder : IPatternElementPlaceholder
    {
        /// <summary>
        /// Start of text slice
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// End of text slice
        /// </summary>
        public int End { get; }
        /// <summary>
        /// Indent of Text element
        /// </summary>
        public int Indent { get; }
        
        /// <summary>
        /// Text element position used in pattern processing
        /// </summary>
        public TextElementPosition Role { get; }
        
        /// <summary>
        /// Boolean flag to add an EOL to text slice. It's used on Windows to make sure newlines are processed correctly
        /// </summary>
        public bool MissingEol { get; } 

        /// <summary>
        /// Constructs a text element placeholder
        /// </summary>
        /// <param name="start">Start of text.</param>
        /// <param name="end">End of text.</param>
        /// <param name="indent">Current indent of text.</param>
        /// <param name="role">Text position on a line.</param>
        /// <param name="missingEol">Is the element missing End-of-line.</param>
        public TextElementPlaceholder(int start, int end, int indent, TextElementPosition role, bool missingEol)
        {
            Start = start;
            End = end;
            Indent = indent;
            Role = role;
            MissingEol = missingEol;
        }
    }
}
