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
        public Identifier Id;
        public Pattern? Value;
        public List<Attribute> Attributes;
        public AstComment? Comment;

        public AstMessage(Identifier id, Pattern? pattern, List<Attribute> attrs, AstComment? comment)
        {
            Id = id;
            Value = pattern;
            Attributes = attrs;
            Comment = comment;
        }

        public string GetId()
        {
            return Id.ToString();
        }
    }

    public class AstTerm : IEntry
    {
        public Identifier Id;
        public Pattern Value;
        public List<Attribute> Attributes;
        public AstComment? Comment;

        public AstTerm(Identifier id, Pattern value, List<Attribute> attributes, AstComment? comment)
        {
            Id = id;
            Value = value;
            Attributes = attributes;
            Comment = comment;
        }


        public string GetId()
        {
            return Id.ToString();
        }
    }

    public class AstComment : IEntry
    {
        public CommentLevel CommentLevel;
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
        public ReadOnlyMemory<char> Content;

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