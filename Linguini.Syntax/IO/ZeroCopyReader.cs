using System;
using Linguini.Shared.Util;

namespace Linguini.Syntax.IO
{
    /// <summary>
    /// Zero copy reader used to parse Fluent resources
    /// </summary>
    public class ZeroCopyReader
    {
        private readonly ReadOnlyMemory<char> _unconsumedData;
        private int _position;
        private int _row;

        /// <summary>
        /// Represents a reader that processes in-memory text fragment.
        /// </summary>
        /// <param name="text">Contents of the file.</param>
        /// <param name="fileName">"File name" of the content being given.</param>
        public ZeroCopyReader(string text, string? fileName = null) : this(text.AsMemory(), fileName)
        {
        }

        private ZeroCopyReader(ReadOnlyMemory<char> memory, string? fileName = null)
        {
            _unconsumedData = memory;
            _position = 0;
            _row = 1;
            FileName = fileName;
        }

        /// <summary>
        /// Gets or sets the current position of the reader within the input data.
        /// </summary>
        /// <remarks>
        /// This property represents a zero-based index that tracks the reader's current location
        /// in the parsed data. It is used for navigation and parsing operations within the input.
        /// Modifying this property will directly affect the reader's position.
        /// </remarks>
        public int Position
        {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// Gets or sets the current row of the reader within the input data.
        /// </summary>
        public int Row
        {
            get => _row;
            set => _row = value;
        }

        /// <summary>
        /// Read-only property telling you <c>Filename</c> of a given <see cref="ZeroCopyReader"/>.
        /// </summary>
        public string? FileName { get; }

        /// <summary>
        /// Is the reader still inside the documentation.
        /// </summary>
        public bool IsNotEof => _position < _unconsumedData.Length;
        /// <summary>
        /// Has the reader reached or passed the end of the file.
        /// </summary>
        public bool IsEof => !IsNotEof;
        internal ReadOnlyMemory<char> GetData => _unconsumedData;

        /// <summary>
        /// Peeks at a character at the specified position relative to the current position in the stream.
        /// </summary>
        /// <param name="offset">The relative position offset to peek at. Defaults to 0.</param>
        /// <returns>The character at the specified position if within bounds; otherwise, null.</returns>
        public char? PeekChar(int offset = 0)
        {
            if (_unconsumedData.TryReadChar(_position + offset, out var c))
            {
                return c;
            }

            return null;
        }

        /// <summary>
        /// Skips blank blocks while updating row counts.
        /// </summary>
        /// <returns>Number  many lines were skipped</returns>
        public int SkipBlankBlock()
        {
            var count = 0;
            while (true)
            {
                var start = _position;
                SkipBlankInline();
                if (!SkipEol())
                {
                    _position = start;
                    break;
                }

                count += 1;
            }

            return count;
        }

        /// <summary>
        /// Method for finding End-of-line faster.
        /// </summary>
        /// <returns><c></c></returns>
        public bool SeekEol()
        {
            var index = _unconsumedData.Span.Slice(_position).IndexOf('\n');
            if (index != -1)
            {
                _row += 1;
                _position += index + 1;
                return true;
            }

            _position = _unconsumedData.Length;
            return false;
        }

        /// <summary>
        /// Method for skipping End of Line regardless of platform.
        /// </summary>
        /// <returns></returns>
        public bool SkipEol()
        {
            if ('\n' == PeekChar())
            {
                _row += 1;
                _position += 1;
                return true;
            }

            if ('\r' == PeekChar()
                && '\n' == PeekChar(1))
            {
                _row += 1;
                _position += 2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Skips consecutive blank spaces starting from the current position of the reader
        /// and advances the position to the first non-blank character.
        /// </summary>
        /// <returns>
        /// The number of blank spaces skipped.
        /// </returns>
        public int SkipBlankInline()
        {
            var start = _position;
            while (_unconsumedData.TryReadChar(_position, out var c) && ' ' == c)
            {
                _position += 1;
            }

            return _position - start;
        }

        /// <summary>
        /// Reads a char if it matches the char exactly.
        /// </summary>
        /// <param name="c">input must match this <c>char</c></param>
        /// <returns><c>true</c> if next character is equal to <paramref name="c"/>; otherwise <c>false</c>.</returns>
        public bool ReadCharIf(char c)
        {
            if (_unconsumedData.TryReadChar(_position, out var c1) && c == c1)
            {
                if (c == '\n')
                {
                    _row += 1;
                }

                _position += 1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads the current line-up to the end-of-line and returns it
        /// as a slice of the unconsumed data.
        /// </summary>
        /// <returns>A slice of the remaining data representing the current line.</returns>
        public ReadOnlyMemory<char> GetCommentLine()
        {
            var startPosition = _position;

            while (!IsEol())
            {
                _position += 1;
            }

            return _unconsumedData.Slice(startPosition, _position - startPosition);
        }

        private bool IsEol()
        {
            var chr = PeekChar();

            if ('\n' == chr) return true;
            if ('\r' == chr
                && '\n' == PeekChar(1))
            {
                return true;
            }

            return IsEof;
        }

        /// <summary>
        /// Reads a substring from the underlying data within the specified range.
        /// </summary>
        /// <param name="start">The start index of the slice.</param>
        /// <param name="end">The end index of the slice, exclusive.</param>
        /// <returns>A memory slice of the data within the specified range.</returns>
        public ReadOnlyMemory<char> ReadSlice(int start, int end)
        {
            return _unconsumedData.Slice(start, end - start);
        }

        /// <summary>
        /// Reads a slice from the underlying data and converts it to a string.
        /// </summary>
        /// <param name="start">The starting position of the slice.</param>
        /// <param name="end">The ending position of the slice.</param>
        /// <returns>A string representing the sliced data.</returns>
        public string ReadSliceToStr(int start, int end)
        {
            return _unconsumedData.Slice(start, end - start).Span.ToString();
        }

        /// <summary>
        /// Updates position to next entry, skipping over any ne
        /// </summary>
        public void SkipToNextEntry()
        {
            while (_unconsumedData.TryReadChar(_position, out var c))
            {
                var newline = _position == 0
                              || '\n' == PeekChar(-1);

                if (newline)
                {
                    _row += 1;
                    if (c.IsAsciiAlphabetic() || c.IsOneOf('#', '-'))
                        break;
                }

                _position += 1;
            }
        }

        /// <summary>
        /// Searches the current unconsumed data for the first occurrence of any character in the specified set of characters, starting at the current position.
        /// </summary>
        /// <param name="values">A span containing the set of characters to search for.</param>
        /// <returns>The zero-based index of the first character in the specified set, relative to the current position.
        /// Returns -1 if no match is found.</returns>
        public int IndexOfAnyChar(ReadOnlySpan<char> values)
        {
            return _unconsumedData.Span.Slice(_position).IndexOfAny(values);
        }

        /// <summary>
        /// Attempts to peek at the current character without advancing the reader position.
        /// </summary>
        /// <param name="c">The character at the current position, if available.</param>
        /// <returns>True if a character is successfully peeked; otherwise, false.</returns>
        public bool TryPeekChar(out char c)
        {
            return _unconsumedData.TryReadChar(_position, out c);
        }

        /// <summary>
        /// Attempts to peek a character at a specific position relative to the current position.
        /// </summary>
        /// <param name="pos">The position from which to peek the character.</param>
        /// <param name="c">The character found at the specified position, if available.</param>
        /// <returns><c>true</c> if a character exists at the specified position; otherwise, <c>false</c>.</returns>
        public bool TryPeekCharAt(int pos, out char c)
        {
            return _unconsumedData.TryReadChar(pos, out c);
        }

        /// <summary>
        /// Gets the current character at the reader's current position in the data being processed.
        /// </summary>
        /// <returns>The character at the current position of the reader.</returns>
        public char CurrentChar()
        {
            return _unconsumedData.Span[_position];
        }

        /// <summary>
        /// Skips over any <c> </c>(0x20), <c>\n</c>(0x0A) and <c>\r</c>(0x0D) character.
        /// </summary>
        public void SkipBlank()
        {
            while (TryPeekChar(out var c))
            {
                if (' ' == c)
                {
                    _position += 1;
                }
                else if ('\n' == c)
                {
                    _position += 1;
                    _row += 1;
                }
                else if ('\r' == c && '\n' == PeekChar(1))
                {
                    _position += 2;
                    _row += 1;
                }
                else
                {
                    break;
                }
            } 
        }

        /// <summary>
        /// Decreases the row count by the specified offset, adjusting it based on the number of newline characters
        /// found within the offset range.
        /// </summary>
        /// <param name="offset">The number of characters to trace back to recalculate the row count.</param>
        public void DecrementRow(int offset)
        {
            var span = _unconsumedData.Slice(_position - offset, offset);
            foreach (var chr in span.ToArray())
            {
                if (chr == '\n')
                    _row -= 1;
            }
        }
    }
}
