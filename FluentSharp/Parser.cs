#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using FluentSharp.IO;

namespace FluentSharp
{
    public class Parser
    {
        private ZeroCopyReader _reader;
        private List<ParseError> _errors = new();

        public Resource Parse(TextReader input)
        {
            // TODO add buffering
            _reader = new ZeroCopyReader(input.ReadToEnd());

            var body = new List<IEntry>();

            _reader.SkipBlankBlock();

            Comment? lastComment = null;
            var lastBlankCount = 0;

            while (_reader.IsNotEof)
            {
                var entry_start = _reader.Position;
                IEntry entry = GetEntry(entry_start);

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
        public ErrorType type;
        public string? message;
        public Range Position;
        public Range? Slice;
    }

    public enum ErrorType
    {
    }
}
