using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Linguini.Parser;
using Linguini.Serialization;

namespace Linguini.Ast
{
    [JsonConverter(typeof(ResourceSerializer))]
    public class Resource
    {
        public List<IEntry> Body;
        public List<ParseError> Errors;

        public Resource(List<IEntry> body, List<ParseError> errors)
        {
            Body = body;
            Errors = errors;
        }
    }

    [JsonConverter(typeof(MessageSerializer))]
    public class Message : IEntry
    {
        public Identifier Id;
        public Pattern? Value;
        public List<Attribute> Attributes;
        public Comment? Comment;

        public Message(Identifier id)
        {
            Id = id;
            Value = null;
            Attributes = new();
            Comment = null;
        }

        public Message(Identifier id, Pattern? pattern, List<Attribute> attrs, Comment? comment)
        {
            Id = id;
            Value = pattern;
            Attributes = attrs;
            Comment = comment;
        }
    }

    [JsonConverter(typeof(TermSerializer))]
    public class Term : IEntry
    {
        public Identifier Id;
        public Pattern Value;
        public List<Attribute> Attributes;
        public Comment? Comment;

        public Term(Identifier id, Pattern value, List<Attribute> attributes, Comment? comment)
        {
            Id = id;
            Value = value;
            Attributes = attributes;
            Comment = comment;
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
    }

    [JsonConverter(typeof(JunkSerializer))]
    public class Junk : IEntry
    {
        public ReadOnlyMemory<char> Content;

        public string AsStr(string lineEnd = "\n")
        {
            return new(Content.Span);
        }
    }
}
