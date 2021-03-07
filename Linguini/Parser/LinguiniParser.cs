#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Linguini.Ast;
using Linguini.IO;
using Attribute = Linguini.Ast.Attribute;

namespace Linguini.Parser
{
    public class LinguiniParser
    {
        private readonly ZeroCopyReader _reader;

        public LinguiniParser(string input)
        {
            _reader = new ZeroCopyReader(input);
        }

        public LinguiniParser(TextReader input)
        {
            // TODO add buffering
            _reader = new ZeroCopyReader(input.ReadToEnd());
        }

        public Resource Parse()
        {
            var body = new List<IEntry>();
            var errors = new List<ParseError>();
            _reader.SkipBlankBlock();

            Comment? lastComment = null;
            var lastBlankCount = 0;

            while (_reader.IsNotEof)
            {
                var entryStart = _reader.Position;
                (IEntry entry, ParseError? error) = GetEntry(entryStart);

                if (lastComment != null)
                {
                    if (entry.TryConvert<IEntry, Message>(out var message)
                        && lastBlankCount < 2)
                    {
                        message.Comment = lastComment;
                    }
                    else if (entry.TryConvert<IEntry, Term>(out var term)
                             && lastBlankCount < 2)
                    {
                        term.Comment = lastComment;
                    }
                    else
                    {
                        body.Add(lastComment);
                    }

                    lastComment = null;
                }

                if (error != null)
                {
                    _reader.SkipToNextEntry();
                    error.Slice = new Range(entryStart, _reader.Position);
                    errors.Add(error);
                    Junk junk = (Junk) entry;
                    junk.Content = _reader.ReadSlice(entryStart, _reader.Position);
                    body.Add(entry);
                }
                else if (entry.TryConvert<IEntry, Comment>(out var comment) 
                         && comment.CommentLevel == CommentLevel.Comment)
                {
                    lastComment = comment;
                }
                else
                {
                    body.Add(entry);
                }

                lastBlankCount = _reader.SkipBlankBlock();
            }

            if (lastComment != null)
            {
                body.Add(lastComment);
            }


            return new Resource(body, errors);
        }

        private (IEntry, ParseError?) GetEntry(int entryStart)
        {
            var charSpan = _reader.PeekCharSpan();

            if ('#'.EqualsSpans(charSpan))
            {
                IEntry entry = new Junk();
                if (TryGetComment(out var comment, out var error))
                {
                    entry = comment;
                }

                return (entry, error);
            }

            if ('-'.EqualsSpans(charSpan))
            {
                return GetTerm(entryStart);
            }

            return GetMessage(entryStart);
        }

        private bool TryGetComment([NotNullWhen(true)] out Comment? comment, out ParseError? error)
        {
            var level = CommentLevel.None;
            var content = new List<ReadOnlyMemory<char>>();

            while (_reader.IsNotEof)
            {
                var lineLevel = GetCommentLevel();
                if (lineLevel == CommentLevel.None)
                {
                    _reader.Position -= 1;
                    break;
                }

                if (level != CommentLevel.None && lineLevel != level)
                {
                    _reader.Position -= (int) lineLevel;
                    break;
                }

                level = lineLevel;

                if (_reader.IsEof)
                {
                    break;
                }

                if ('\n'.EqualsSpans(_reader.PeekCharSpan()))
                {
                    content.Add(_reader.GetCommentLine());
                }
                else
                {
                    if (!TryExpectByte(' ', out var e))
                    {
                        if (content.Count == 0)
                        {
                            error = e;
                            comment = default!;
                            return false;
                        }

                        _reader.Position -= (int) lineLevel;
                        break;
                    }

                    content.Add(_reader.GetCommentLine());
                }

                _reader.SkipEol();
            }

            comment = new Comment(level, content);
            error = null;
            return true;
        }

        private bool TryExpectByte(char c, out ParseError? error)
        {
            if (_reader.ReadCharIf(c))
            {
                error = null;
                return true;
            }

            error = ParseError.ExpectedToken(c, _reader.Position);
            return false;
        }

        private CommentLevel GetCommentLevel()
        {
            if (_reader.ReadCharIf('#'))
            {
                if (_reader.ReadCharIf('#'))
                {
                    if (_reader.ReadCharIf('#'))
                    {
                        return CommentLevel.ResourceComment;
                    }

                    return CommentLevel.GroupComment;
                }

                return CommentLevel.Comment;
            }

            return CommentLevel.None;
        }

        private (IEntry, ParseError?) GetTerm(int entryStart)
        {
            throw new NotImplementedException();
        }

        private (IEntry, ParseError?) GetMessage(int entryStart)
        {
            IEntry entry = new Junk();
            if (!TryGetIdentifier(out var id, out var error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();
            if (!TryExpectByte('=', out error))
            {
                return (entry, error);
            }


            if (!TryGetPattern(out var pattern, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankBlock();
            var attrs = GetAttributes();

            if (pattern == null && attrs.Count < 1)
            {
                string idStr = new(id.Name.ToArray());
                return (entry, ParseError.ExpectedMessageField(idStr, entryStart, _reader.Position));
            }

            return (new Message(id, pattern, attrs, null), null);
        }

        private bool TryGetIdentifier([NotNullWhen(true)] out Identifier? id, out ParseError? error)
        {
            if (_reader.PeekCharSpan().IsAsciiAlphabetic())
            {
                error = null;
                id = GetUncheckedIdentifier();
                return true;
            }

            error = ParseError.ExpectedCharRange("a-zA-Z", _reader.Position);
            id = default!;
            return false;
        }

        private Identifier GetUncheckedIdentifier()
        {
            // First character is already checked
            var ptr = _reader.Position + 1;
            while (_reader.PeekCharSpan(ptr).IsIdentifier())
            {
                ptr += 1;
            }

            Identifier id = new(_reader.ReadSlice(_reader.Position, ptr));
            _reader.Position = ptr;
            return id;
        }

        private bool TryGetPattern(out Pattern? pattern, out ParseError? error)
        {
            var elements = new List<IPatternElementPlaceholder>();
            TextElementPosition textElementRole;
            int? lastNonBlank = null;
            int? commonIndent = null;

            _reader.SkipBlankInline();

            if (_reader.SkipEol())
            {
                _reader.SkipBlankBlock();
                textElementRole = TextElementPosition.LineStart;
            }
            else
            {
                textElementRole = TextElementPosition.InitialLineStart;
            }

            while (_reader.IsNotEof)
            {
                if (_reader.ReadCharIf('{'))
                {
                    if (textElementRole == TextElementPosition.LineStart)
                    {
                        commonIndent = 0;
                    }

                    if (!TryGetPlaceable(out var exp, out error))
                    {
                        pattern = null;
                        return false;
                    }

                    lastNonBlank = elements.Count;
                    elements.Add(new Placeable(exp));
                    textElementRole = TextElementPosition.Continuation;
                }
                else
                {
                    var sliceStart = _reader.Position;
                    var indent = 0;
                    if (textElementRole == TextElementPosition.LineStart)
                    {
                        indent = _reader.SkipBlankInline();
                        if (_reader.TryPeekCharSpan(out var b))
                        {
                            if (indent == 0)
                            {
                                if ('\r'.EqualsSpans(b) && '\n'.EqualsSpans(b))
                                {
                                    break;
                                }
                            }
                            else if (b.IsOneOf('.', '}', '[', '*'))
                            {
                                _reader.Position = sliceStart;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!TryGetTextSlice(out var text, out error))
                    {
                        pattern = null;
                        return false;
                    }

                    if (text.Start != text.End)
                    {
                        if (textElementRole == TextElementPosition.LineStart
                            && text.ElementType == TextElementType.NonBlank)
                        {
                            if (commonIndent != null)
                            {
                                if (indent < commonIndent)
                                {
                                    commonIndent = indent;
                                }
                            }
                            else
                            {
                                commonIndent = indent;
                            }
                        }

                        if (textElementRole != TextElementPosition.LineStart
                            || text.ElementType == TextElementType.NonBlank
                            || text.TerminationReason == TextElementTermination.LF)
                        {
                            if (text.ElementType == TextElementType.NonBlank)
                            {
                                lastNonBlank = elements.Count;
                            }

                            elements.Add(new TextElementPlaceholder(
                                sliceStart,
                                text.End,
                                indent,
                                textElementRole
                            ));
                        }
                    }

                    textElementRole = text.TerminationReason switch
                    {
                        TextElementTermination.LF => TextElementPosition.LineStart,
                        TextElementTermination.CRLF => TextElementPosition.LineStart,
                        TextElementTermination.PlaceableStart => TextElementPosition.Continuation,
                        TextElementTermination.EndOfFile => TextElementPosition.Continuation,
                        _ => textElementRole
                    };
                }
            }

            if (lastNonBlank != null)
            {
                List<IPatternElement> patterns = new(lastNonBlank.Value + 1);

                for (int i = 0; i < lastNonBlank + 1; i++)
                {
                    IPatternElementPlaceholder? elem = null;
                    if (i < elements.Count)
                    {
                        elem = elements[i];
                        if (elem.TryConvert(out Placeable placeable))
                        {
                            patterns.Add(placeable);
                        }
                        else if (elem.TryConvert(out TextElementPlaceholder textLiteral))
                        {
                            int start = textLiteral.Start;
                            int indent = textLiteral.Indent;
                            var end = textLiteral.End;
                            if (textLiteral.Role == TextElementPosition.LineStart)
                            {
                                if (commonIndent == null)
                                {
                                    start = start + indent;
                                }
                                else
                                {
                                    start = start + Math.Min(indent, commonIndent.Value);
                                }
                            }

                            var value = _reader.ReadSlice(start, end);
                            if (lastNonBlank == i)
                            {
                                value = value.Trim();
                            }

                            patterns.Add(new TextLiteral(value));
                        }
                    }
                }

                pattern = new Pattern(patterns);
                error = null;
                return true;
            }

            // We successfully found nothing
            pattern = null;
            error = null;
            return true;
        }

        private bool TryGetTextSlice([NotNullWhen(true)] out TextSlice? textElement, out ParseError? error)
        {
            var startPos = _reader.Position;
            var textElementType = TextElementType.Blank;

            while (_reader.TryPeekCharSpan(out var span))
            {
                if (' '.EqualsSpans(span))
                {
                    _reader.Position += 1;
                }
                else if ('\n'.EqualsSpans(span))
                {
                    _reader.Position += 1;
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.LF
                    );
                    error = null;
                    return true;
                }
                else if ('\r'.EqualsSpans(span)
                         && '\n'.EqualsSpans(_reader.PeekCharSpan(1)))
                {
                    _reader.Position += 1;
                    // This takes one less element because it converts CRLF endings 
                    // to LF endings
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position - 1, 
                        textElementType,
                        TextElementTermination.CRLF
                    );
                    error = null;
                    return true;
                }
                else if ('{'.EqualsSpans(span))
                {
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.PlaceableStart
                    );
                    error = null;
                    return true;
                }
                else if ('}'.EqualsSpans(span))
                {
                    textElement = null;
                    error = ParseError.UnbalancedClosingBrace(_reader.Position);
                    return false;
                }
                else
                {
                    textElementType = TextElementType.NonBlank;
                    _reader.Position += 1;
                }
            }

            textElement = new TextSlice(
                startPos,
                _reader.Position,
                textElementType,
                TextElementTermination.EndOfFile
            );
            error = null;
            return true;
        }

        private bool TryGetPlaceable([NotNullWhen(true)] out IExpression? expr, out ParseError? error)
        {
            _reader.SkipBlank();
            if (TryGetExpression(out expr, out error))
            {
                return false;
            }

            _reader.SkipBlankInline();
            if (!TryExpectByte('}', out error))
            {
                expr = null;
                return false;
            }

            if (!expr.TryConvert(out TermReference termReference)
                || termReference.Attribute == null)
            {
                error = ParseError.TermAttributeAsPlaceable(_reader.Position);
                return false;
            }

            return true;
        }

        private bool TryGetExpression(out IExpression expr, out ParseError? error)
        {
            throw new NotImplementedException();
        }


        private List<Attribute> GetAttributes()
        {
            var attributes = new List<Attribute>();

            while (true)
            {
                var lineStart = _reader.Position;
                _reader.SkipBlankInline();
                if (!_reader.ReadCharIf('.'))
                {
                    _reader.Position = lineStart;
                    break;
                }

                if (TryGetAttribute(out var attr, out _))
                {
                    attributes.Add(attr.Value);
                }
                else
                {
                    _reader.Position = lineStart;
                    break;
                }
            }

            return attributes;
        }

        private bool TryGetAttribute([NotNullWhen(true)] out Attribute? attr, out ParseError? error)
        {
            if (!TryGetIdentifier(out var id, out var err))
            {
                error = err;
                attr = default!;
                return false;
            }

            _reader.SkipBlankInline();

            if (!TryExpectByte('=', out err))
            {
                error = err;
                attr = default!;
                return false;
            }

            if (TryGetPattern(out var pattern, out err) && pattern != null)
            {
                error = null;
                attr = new Attribute(id, pattern);
                return true;
            }

            error = ParseError.MissingValue(id, _reader.Position);
            attr = default!;
            return false;
        }
    }

    public class TextSlice
    {
        public int Start { get; }
        public int End { get; }
        public TextElementType ElementType { get; }
        public TextElementTermination TerminationReason { get; }

        public TextSlice(int start, int end, TextElementType elementType, TextElementTermination terminationReason)
        {
            Start = start;
            End = end;
            ElementType = elementType;
            TerminationReason = terminationReason;
        }
    }
}
