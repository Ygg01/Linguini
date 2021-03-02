#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Ast;
using Linguini.IO;

namespace Linguini
{
    public class Parser
    {
        private ZeroCopyReader _reader;
        private List<ParseError> _errors = new();

        public Parser(TextReader input)
        {
            // TODO add buffering
            _reader = new ZeroCopyReader(input.ReadToEnd());
        }

        public Resource Parse()
        {
            var body = new List<IEntry>();

            _reader.SkipBlankBlock();

            Comment? lastComment = null;
            var lastBlankCount = 0;

            while (_reader.IsNotEof)
            {
                var entryStart = _reader.Position;
                IEntry entry = GetEntry(entryStart);

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

                if (entry.TryConvert<Comment>(out var comment))
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

            
            return new Resource(body, _errors);
        }

        private IEntry GetEntry(int entryStart)
        {
            var charSpan = _reader.GetCharSpan();
            IEntry entry;
            if ('#'.EqualsSpans(charSpan))
            {
                entry = GetComment();
            }
            else if ('-'.EqualsSpans(charSpan))
            {
                entry = GetTerm(entryStart);
            }
            else
            {
                entry = GetMessage(entryStart);
            }

            return entry;
        }

        private Term GetTerm(int entryStart)
        {
            throw new NotImplementedException();
        }

        private Message GetMessage(int entryStart)
        {
            throw new NotImplementedException();
        }

        private Comment GetComment()
        {
            throw new NotImplementedException();
        }
    }

    public class ParseError
    {
        public ErrorType Type;
        public string? Message;
        public Range Position;
        public Range? Slice;
    }

    public enum ErrorType
    {
    }
}
