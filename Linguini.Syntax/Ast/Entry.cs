using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linguini.Syntax.IO;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Syntax.Ast
{
    /// <summary>
    ///     Represents a Fluent AST resource that contains entries and parse errors.
    /// </summary>
    public record Resource
    {
        /// <summary>
        ///     A collection of entries that represent the structural components of a Fluent AST resource.
        /// </summary>
        /// <remarks>
        ///     Entries can be of various types defined by the <see cref="IEntry" /> interface, including
        ///     messages, terms, comments, or junk elements. Each entry corresponds to a specific unit
        ///     or structural element in the Fluent syntax.
        /// </remarks>
        public readonly List<IEntry> Entries;

        /// <summary>
        ///     A collection of parse errors encountered during the processing of a Fluent AST resource.
        /// </summary>
        /// <remarks>
        ///     Each parse error is represented as an instance of the <see cref="ParseError" /> class, which provides
        ///     detailed diagnostic information, such as error type, position, and context.
        ///     The <c>Errors</c> collection allows consumers of the Fluent AST to examine and handle
        ///     issues that were identified during parsing.
        /// </remarks>
        public readonly List<ParseError> Errors;

        /// <summary>
        ///     Basic constructor of the Resources
        /// </summary>
        /// <param name="body">List of <see cref="IEntry" /> objects that are contained in this resource.</param>
        /// <param name="errors">List of <see cref="ParseError" />.</param>
        public Resource(List<IEntry> body, List<ParseError> errors)
        {
            Entries = body;
            Errors = errors;
        }
    }

    /// <summary>
    ///     Represents a message entry in the Fluent AST with an identifier, optional value, attributes, and location.
    /// </summary>
    public class AstMessage : IEntry, IEquatable<AstMessage>
    {
        /// <summary>
        ///     A collection of attributes associated with a Fluent AST message.
        /// </summary>
        /// <remarks>
        ///     Each attribute provides additional localized metadata or content for the associated Fluent message.
        ///     Attributes are key-value pairs represented by the <see cref="Attribute" /> class,
        ///     and are primarily used to store supplemental information such as alternate values or descriptive details.
        /// </remarks>
        public readonly List<Attribute> Attributes;

        /// <summary>
        ///     <see cref="Identifier" /> of the message
        /// </summary>
        public readonly Identifier Id;

        internal readonly AstComment? InternalComment;

        /// <summary>
        ///     Location which stores debugging information.
        /// </summary>
        /// <seealso cref="AstLocation" />
        public readonly AstLocation Location;

        /// <summary>
        ///     Represents the primary textual content of a message in the Fluent AST.
        /// </summary>
        /// <remarks>
        ///     The value defines the central message text associated with an identifier.
        ///     It is optional and represented by a <see cref="Pattern" />. If not present,
        ///     the message entry might rely solely on attributes for its definition.
        /// </remarks>
        public readonly Pattern? Value;

        /// <summary>
        ///     Basic constructor for <c>AstMessage</c>
        /// </summary>
        /// <param name="id">Identifier of the message</param>
        /// <param name="pattern">Optional pattern of the message.</param>
        /// <param name="attrs">The list of attributes, which may be empty.</param>
        /// <param name="location">Location of the <c>AstMessage</c>.</param>
        /// <param name="internalComment">Optional comment of the message.</param>
        public AstMessage(Identifier id, Pattern? pattern, List<Attribute> attrs, AstLocation location,
            AstComment? internalComment)
        {
            Id = id;
            Value = pattern;
            Attributes = attrs;
            Location = location;
            InternalComment = internalComment;
        }

        /// <summary>
        ///     Represents an optional comment associated with a Fluent AST message entry.
        /// </summary>
        /// <remarks>
        ///     The comment provides auxiliary information or context about the entry,
        ///     typically used to describe its purpose or clarify its usage within Fluent translations. When set to fast
        ///     parse the comments will be ignored.
        /// </remarks>
        public AstComment? Comment => InternalComment;

        /// <inheritdoc />
        public string GetId()
        {
            return Id.ToString();
        }


        /// <inheritdoc />
        public bool Equals(AstMessage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Comparer.Equals(Id, other.Id) && Equals(Value, other.Value) &&
                   Attributes.SequenceEqual(other.Attributes, Attribute.Comparer) &&
                   Equals(InternalComment, other.InternalComment);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AstMessage)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value, Attributes, Comment);
        }
    }

    /// <summary>
    ///     Represents a Term node in the Fluent AST.
    /// </summary>
    /// <remarks>
    ///     An AstTerm is used to define a named term with an identifier, value, optional attributes, and comments within the
    ///     Fluent syntax.
    /// </remarks>
    /// <seealso cref="IEntry" />
    /// <seealso cref="Identifier" />
    /// <seealso cref="Pattern" />
    /// <seealso cref="AstLocation" />
    /// <seealso cref="Attribute" />
    public class AstTerm : IEntry, IEquatable<AstTerm>
    {
        /// <summary>
        ///     A collection of attributes associated with a Fluent term.
        /// </summary>
        /// <remarks>
        ///     Each term can have zero or more <see cref="Attribute" />s.
        ///     Attributes are defined as key-value pairs and stored in a list. These attributes
        ///     can be accessed or resolved during runtime to provide localized information or
        ///     extended functionality.
        /// </remarks>
        public readonly List<Attribute> Attributes;

        /// <summary>
        ///     Identifier of the <c>AstTerm</c>
        /// </summary>
        public readonly Identifier Id;

        internal readonly AstComment? InternalComment;

        /// <summary>
        ///     Represents the location in a source file associated with a specific AST node.
        /// </summary>
        /// <remarks>
        ///     The Location is used to identify the precise row and filename where the AST node is declared
        ///     in the Fluent syntax. It provides context for debugging, error reporting, or source mapping.
        /// </remarks>
        public readonly AstLocation Location;

        /// <summary>
        ///     Identifier of the <c>AstTerm</c>
        /// </summary>
        public readonly Pattern Value;

        /// <summary>
        ///     Constructs an <see cref="AstTerm" />.
        /// </summary>
        /// <param name="id">An <see cref="Identifier" /> string that uniquely identifies the term.</param>
        /// <param name="value">A <see cref="Pattern" /> object representing the value of the term.</param>
        /// <param name="attributes">A list of <see cref="Attribute" /> that define additional properties of the term.</param>
        /// <param name="location">An <see cref="AstLocation" /> object indicating the term's location in the source.</param>
        /// <param name="comment">An optional <see cref="AstComment" /> associated with the term.</param>
        public AstTerm(Identifier id, Pattern value, List<Attribute> attributes, AstLocation location,
            AstComment? comment)
        {
            Id = id;
            Value = value;
            Attributes = attributes;
            Location = location;
            InternalComment = comment;
        }

        /// <summary>
        ///     Represents an optional comment associated with a specific term in the Fluent AST.
        /// </summary>
        /// <remarks>
        ///     The comment is a metadata added by the user. It can have several levels.
        /// </remarks>
        public AstComment? Comment => InternalComment;

        /// <inheritdoc />
        public string GetId()
        {
            return Id.ToString();
        }

        /// <inheritdoc />
        public bool Equals(AstTerm? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Comparer.Equals(Id, other.Id) && Equals(Value, other.Value) &&
                   Attributes.SequenceEqual(other.Attributes, Attribute.Comparer) &&
                   Equals(InternalComment, other.InternalComment);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AstMessage)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value, Attributes, Comment);
        }
    }

    /// <summary>
    ///     Represents the location of an AST node, providing debugging information
    ///     such as the row number and the file name from which the node originates.
    /// </summary>
    /// <remarks>
    ///     An AstLocation is used to track the origin of nodes in the Fluent AST,
    ///     which can be useful for error reporting and debugging purposes.
    /// </remarks>
    public class AstLocation
    {
        /// <summary>
        ///     Constant depicting that corresponds to unset <c>AstLocation</c>
        /// </summary>
        public static readonly AstLocation Empty = new(-1, "???");

        private AstLocation(int row, string fileName)
        {
            Row = row;
            FileName = fileName;
        }

        /// <summary>
        ///     Gets the name of the file associated with the AST node location.
        /// </summary>
        /// <remarks>
        ///     This property provides the file name from which the AST node originates, or "???" if it's unknown, like for
        ///     example if the <see cref="ZeroCopyReader" /> is generated on the fly.
        /// </remarks>
        public string FileName { get; }

        /// <summary>
        ///     Gets the row of the file associated with the AST node location.
        /// </summary>
        /// <remarks>
        ///     This property provides the row from which the AST node originates, if possible.
        /// </remarks>
        public int Row { get; }

        /// <summary>
        ///     Factory method that creates <see cref="AstLocation" /> from row and filename.
        /// </summary>
        /// <param name="row">row position in file</param>
        /// <param name="fileName">filename of the file</param>
        /// <returns>An <see cref="AstLocation" /> position in the file.</returns>
        public static AstLocation FromRowAndFilename(int row, string fileName)
        {
            return new AstLocation(row, fileName);
        }

        /// <summary>
        ///     Factory method that creates <see cref="AstLocation" /> from <see cref="ZeroCopyReader" />.
        /// </summary>
        /// <param name="reader">
        ///     An instance of <see cref="ZeroCopyReader" /> containing information about the current row and file
        ///     name for the location.
        /// </param>
        /// <returns>An <see cref="AstLocation" /> representing the row and file name extracted from the provided reader.</returns>
        public static AstLocation FromReader(ZeroCopyReader reader)
        {
            return new AstLocation(reader.Row, reader.FileName ?? "???");
        }
    }

    /// <summary>
    ///     Represents a Fluent AST comment, storing information about its level and content.
    /// </summary>
    public class AstComment : IEntry, IEquatable<AstComment>
    {
        /// <summary>
        ///     Represents the level or type of comment in the Fluent AST.
        /// </summary>
        /// <remarks>
        ///     Comment levels are used to distinguish between different types of comments, such as
        ///     regular comments, group comments (used for categorization), and resource-level
        ///     comments (describing the entire resource). This level is defined by the
        ///     <see cref="CommentLevel" /> enum.
        /// </remarks>
        public readonly CommentLevel CommentLevel;

        /// <summary>
        ///     Represents the content of a Fluent AST comment as a collection of text lines.
        /// </summary>
        public readonly List<ReadOnlyMemory<char>> Content;

        /// <summary>
        ///     Constructs a Fluent AST comment, from level and content/
        /// </summary>
        /// <param name="commentLevel">The level of the comment, represented as <see cref="CommentLevel" />.</param>
        /// <param name="content">The content of the comment, stored as a list of lines.</param>
        public AstComment(CommentLevel commentLevel, List<ReadOnlyMemory<char>> content)
        {
            CommentLevel = commentLevel;
            Content = content;
        }

        /// <inheritdoc />
        public string GetId()
        {
            return "Comment";
        }

        /// <inheritdoc />
        public bool Equals(AstComment? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (CommentLevel != other.CommentLevel) return false;
            if (Content.Count != other.Content.Count) return false;
            for (var i = 0; i < Content.Count; i++)
            {
                var l = Content[i];
                var r = other.Content[i];
                if (!l.Span.SequenceEqual(r.Span)) return false;
            }

            return true;
        }

        /// <summary>
        ///     Converts the content of the comment into a single string, with each line separated by the specified line ending.
        /// </summary>
        /// <param name="lineEnd">The string used to separate lines in the output. The default value is a newline ("\n").</param>
        /// <returns>A string representation of the comment content, where lines are joined by the specified line ending.</returns>
        public string AsStr(string lineEnd = "\n")
        {
            StringBuilder sb = new();
            for (var i = 0; i < Content.Count; i++)
            {
                if (i > 0) sb.Append(lineEnd);

                sb.Append(Content[i].Span.ToString());
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AstComment)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine((int)CommentLevel, Content);
        }
    }

    /// <summary>
    ///     Represents a malformed or non-conforming section of a Fluent syntax resource.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Junk" /> class stores content that could not be parsed properly
    ///     according to the Fluent syntax rules.
    /// </remarks>
    public class Junk : IEntry, IEquatable<Junk>
    {
        /// <summary>
        ///     Represents the content of a malformed or non-conforming section in a Fluent syntax resource.
        /// </summary>
        /// <remarks>
        ///     The <c>Content</c> field stores the raw unparsed text of the section that could not
        ///     be successfully parsed according to Fluent syntax rules.
        /// </remarks>
        public readonly ReadOnlyMemory<char> Content;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Junk()
        {
            Content = ReadOnlyMemory<char>.Empty;
        }

        /// <summary>
        ///     Constructs a Junk node from <paramref name="content" />.
        /// </summary>
        public Junk(ReadOnlyMemory<char> content)
        {
            Content = content;
        }

        /// <summary>
        ///     Constructs a Junk node from <see cref="string" /> <paramref name="content" />.
        /// </summary>
        public Junk(string content)
        {
            Content = content.AsMemory();
        }

        /// <inheritdoc />
        public string GetId()
        {
            return Content.Span.ToString();
        }

        /// <inheritdoc />
        public bool Equals(Junk? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Content.Span.SequenceEqual(other.Content.Span);
        }

        /// <summary>
        ///     Converts the content of the Junk node to a string representation.
        /// </summary>
        /// <returns>A string representation of the content stored in the Junk node.</returns>
        public string AsStr()
        {
            return Content.Span.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Junk)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }
    }
}