using System;
using System.Collections.Generic;
using Linguini.Parser;

namespace Linguini.Ast
{
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

    public class Message : IEntry
    {
        public Identifier Id;
        public Pattern? Value;
        public List<Attribute> Attributes;
        public Comment? Comment;
    }

    public class Term : IEntry
    {
        public Identifier Id;
        public Pattern Value;
        public List<Attribute> Attributes;
        public Comment? Comment;
    }

    public class Comment : IEntry
    {
        public CommentLevel CommentLevel;
        public readonly List<ReadOnlyMemory<char>> Content;

        public Comment() : this(CommentLevel.Comment, new())
        {
        }

        public Comment(CommentLevel commentLevel, List<ReadOnlyMemory<char>> content)
        {
            CommentLevel = commentLevel;
            Content = content;
        }
    }

    public class Junk : IEntry
    {
        public ReadOnlyMemory<char> Content;
    }
}
