using System;
using System.Collections.Generic;
using System.Text;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Syntax.Ast
{
    public record Resource
    {
        public readonly List<IEntry> Entries;
        public readonly List<ParseError> Errors;

        public Resource(List<IEntry> body, List<ParseError> errors)
        {
            Entries = body;
            Errors = errors;
        }
    }

    public class AstMessage : IEntry
    {
        public readonly Identifier Id;
        public readonly Pattern? Value;
        public readonly List<Attribute> Attributes;

        public AstComment? Comment => InternalComment;
        protected internal AstComment? InternalComment;

        public AstMessage(Identifier id, Pattern? pattern, List<Attribute> attrs, AstComment? internalComment)
        {
            Id = id;
            Value = pattern;
            Attributes = attrs;
            InternalComment = internalComment;
        }

        public string GetId()
        {
            return Id.ToString();
        }
    }

    public class AstTerm : IEntry
    {
        public readonly Identifier Id;
        public readonly Pattern Value;
        public readonly List<Attribute> Attributes;
        public AstComment? Comment => InternalComment;
        protected internal AstComment? InternalComment;

        public AstTerm(Identifier id, Pattern value, List<Attribute> attributes, AstComment? comment)
        {
            Id = id;
            Value = value;
            Attributes = attributes;
            InternalComment = comment;
        }


        public string GetId()
        {
            return Id.ToString();
        }
    }

    public class AstComment : IEntry
    {
        public readonly CommentLevel CommentLevel;
        public readonly List<ReadOnlyMemory<char>> Content;

        public AstComment(CommentLevel commentLevel, List<ReadOnlyMemory<char>> content)
        {
            CommentLevel = commentLevel;
            Content = content;
        }

        public string AsStr(string lineEnd = "\n")
        {
            StringBuilder sb = new();
            for (int i = 0; i < Content.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(lineEnd);
                }

                sb.Append(Content[i].Span.ToString());
            }

            return sb.ToString();
        }

        public string GetId()
        {
            return "Comment";
        }
    }

    public class Junk : IEntry
    {
        public readonly ReadOnlyMemory<char> Content;

        public Junk()
        {
            Content = ReadOnlyMemory<char>.Empty;
        }
        
        public Junk(ReadOnlyMemory<char> content)
        {
            Content = content;
        }

        public string AsStr()
        {
            return Content.Span.ToString();
        }

        public string GetId()
        {
            return Content.Span.ToString();
        }
    }
}