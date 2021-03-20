using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Linguini.Syntax.Parser.Error;
using Linguini.Syntax.Serialization;

namespace Linguini.Syntax.Ast
{
    [JsonConverter(typeof(ResourceSerializer))]
    public record Resource
    {
        public readonly List<IEntry> Body;
        public readonly List<ParseError> Errors;

        public Resource(List<IEntry> body, List<ParseError> errors)
        {
            Body = body;
            Errors = errors;
        }
    }

    [JsonConverter(typeof(MessageSerializer))]
    public class AstMessage : IEntry
    {
        public Identifier Id;
        public Pattern? Value;
        public List<Attribute> Attributes;
        public Comment? Comment;

        public AstMessage(Identifier id, Pattern? pattern, List<Attribute> attrs, Comment? comment)
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

    [JsonConverter(typeof(TermSerializer))]
    public class AstTerm : IEntry
    {
        public Identifier Id;
        public Pattern Value;
        public List<Attribute> Attributes;
        public Comment? Comment;

        public AstTerm(Identifier id, Pattern value, List<Attribute> attributes, Comment? comment)
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


    [JsonConverter(typeof(CommentSerializer))]
    public class Comment : IEntry
    {
        public CommentLevel CommentLevel;
        public readonly List<ReadOnlyMemory<char>> _content;

        public Comment(CommentLevel commentLevel, List<ReadOnlyMemory<char>> content)
        {
            CommentLevel = commentLevel;
            _content = content;
        }

        public string AsStr(string lineEnd = "\n")
        {
            StringBuilder sb = new();
            for (int i = 0; i < _content.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(lineEnd);
                }

                sb.Append(_content[i].Span);
            }

            return sb.ToString();
        }

        public string GetId()
        {
            return "Comment";
        }
    }

    [JsonConverter(typeof(JunkSerializer))]
    public class Junk : IEntry
    {
        public ReadOnlyMemory<char> Content;

        public string AsStr(string lineEnd = "\n")
        {
            return new(Content.Span);
        }

        public string GetId()
        {
            return new(Content.Span);
        }
    }
}
