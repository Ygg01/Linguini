#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Linguini.Ast;
using Linguini.IO;

namespace Linguini.Parser
{

    public class LinguiniParser
    {
        private ZeroCopyReader _reader;

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
                    if (entry.TryConvert<Message>(out var message)
                        && lastBlankCount < 2)
                    {
                        message.Comment = lastComment;
                    }
                    else if (entry.TryConvert<Term>(out var term)
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
                else if (entry.TryConvert<Comment>(out var comment))
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

        private (IEntry, ParseError?) GetTerm(int entryStart)
        {
            throw new NotImplementedException();
        }

        private (IEntry, ParseError?) GetMessage(int entryStart)
        {
            throw new NotImplementedException();
        }

        private bool TryGetComment([NotNullWhen(true)] out Comment comment, out ParseError? error)
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
                    if (!_reader.ReadByteIf(' '))
                    {
                        ParseError e = ParseError.ExpectedToken(' ', _reader.Position);
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

        private CommentLevel GetCommentLevel()
        {
            if (_reader.ReadByteIf('#'))
            {
                if (_reader.ReadByteIf('#'))
                {
                    if (_reader.ReadByteIf('#'))
                    {
                        return CommentLevel.ResourceComment;
                    }

                    return CommentLevel.GroupComment;
                }

                return CommentLevel.Comment;
            }

            return CommentLevel.None;
        }
    }
}
