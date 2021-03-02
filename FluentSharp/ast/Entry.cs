using System;
using System.Collections.Generic;

namespace FluentSharp.Ast
{
    public struct Resource
    {
        public List<IEntry> Body;
        public List<ParseError> Errors;

        public Resource(List<IEntry> body, List<ParseError> errors)
        {
            Body = body;
            Errors = errors;
        }
    }

    public struct Message : IEntry
    {
        public Identifier Id;
        public Pattern? Value;
        public List<Attribute> Attributes;
        public Comment? Comment;
    }

    public struct Term : IEntry
    {
        public Identifier Id;
        public Pattern Value;
        public List<Attribute> Attributes;
        public Comment? Comment;
    }

    public struct Comment : IEntry
    {
        public CommentLevel CommentLevel;
        public ReadOnlyMemory<char> Content;
    }

    public struct Junk : IEntry
    {
        public ReadOnlyMemory<char> Content;
    }
}
