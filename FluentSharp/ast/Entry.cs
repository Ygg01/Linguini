using System;
using System.Collections.Generic;

namespace FluentSharp
{
    public struct Resource
    {
        public List<Entry> Body;
    }

    public interface Entry {}
    
    public struct Identifier : Entry
    {
        public ReadOnlyMemory<char> Name;
    }

    public struct Pattern: Entry
    {
        public List<ReadOnlyMemory<char>> Elements;
    }

    public struct Attribute: Entry
    {
        public Identifier Id;
        public Pattern Value;
    }

    public struct Message: Entry
    {
        public Identifier Id;
        public Pattern? Value;
        public List<Attribute> Attributes;
        public Comment? Comment;
    }

    public struct Term: Entry
    {
        public Identifier Id;
        public Pattern Value;
        public List<Attribute> Attributes;
        public Comment? Comment;
    }


    public enum CommentType : sbyte
    {
        Comment,
        GroupComment,
        ResourceComment,
    }

    public struct Comment
    {
        public CommentType CommentType;
        public ReadOnlyMemory<char> Content;
    }

    public struct Junk
    {
        public ReadOnlyMemory<char> Content;
    }
}
