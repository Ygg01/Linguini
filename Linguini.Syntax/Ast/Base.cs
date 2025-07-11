using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ForCanBeConvertedToForeach

namespace Linguini.Syntax.Ast
{
    /// <summary>
    /// Represents an attribute within the syntax abstract syntax tree (AST),
    /// encapsulating an identifier and a pattern value.
    /// </summary>
    /// <remarks>
    /// An <see cref="Attribute"/> consists of an identifier that uniquely identifies it
    /// and a pattern, which represents the content associated with the attribute.
    /// </remarks>
    /// <seealso cref="Linguini.Syntax.Ast.Identifier"/>
    /// <seealso cref="Linguini.Syntax.Ast.Pattern"/>
    public class Attribute : IEquatable<Attribute>
    {
        /// <summary>
        /// Represents the identifier name for an <see cref="Attribute"/> within the Fluent template.
        /// <example>
        /// For example,
        /// <code>
        ///   term
        ///     .AttributeId = test
        /// </code>
        /// </example>
        /// </summary>
        /// <remarks>
        /// The <see cref="Id"/> is used to uniquely identify an <see cref="Attribute"/> within a <see cref="AstTerm"/> is
        /// implemented through the <see cref="Identifier"/> class. It encapsulates
        /// the symbolic name of the attribute.
        /// </remarks>
        public readonly Identifier Id;

        /// <summary>
        /// Represents the value of an <see cref="Attribute"/> in a Fluent template.
        /// </summary>
        /// <remarks>
        /// The <see cref="Value"/> is  a <see cref="Pattern"/> that can be resolved to a string within a Fluent message context.
        /// The <see cref="Value"/> is used extensively during serialization, evaluation, and formatting of Fluent resources.
        /// </remarks>
        public readonly Pattern Value;

        /// <summary>
        /// Provides a default instance of <see cref="Attribute.AttributeComparer"/> for comparing
        /// two <see cref="Attribute"/> instances based on their equality logic.
        /// </summary>
        /// <remarks>
        /// The <see cref="Comparer"/> is used to compare attributes for equality in scenarios
        /// where attributes are part of collections or need to be evaluated for sameness.
        /// It leverages the <see cref="AttributeComparer"/> implementation to perform custom
        /// equality checks.
        /// </remarks>
        public static AttributeComparer Comparer = new();

        /// <inheritdoc />
        public class AttributeComparer : IEqualityComparer<Attribute>
        {
            /// <inheritdoc />
            public bool Equals(Attribute? x, Attribute? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Identifier.Comparer.Equals(x.Id, y.Id) &&
                       x.Value.Equals(y.Value);
            }

            /// <inheritdoc />
            public int GetHashCode(Attribute obj)
            {
                return HashCode.Combine(obj.Id, obj.Value);
            }
        }

        /// <summary>
        /// Constructs an attribute from <see cref="Identifier"/> and a <see cref="Pattern"/>.
        /// </summary>
        /// <param name="id"><see cref="Identifier"/> of the attribute</param>
        /// <param name="value">pattern of the attribute</param>
        public Attribute(Identifier id, Pattern value)
        {
            Id = id;
            Value = value;
        }

        /// <summary>
        /// Constructs an attribute from a <see cref="string"/> and a <see cref="PatternBuilder"/>.
        /// </summary>
        /// <param name="id">string identifier of the attribute</param>
        /// <param name="builder"><see cref="PatternBuilder"/> that can be used to create a pattern programatically.</param>
        public Attribute(string id, PatternBuilder builder)
        {
            Id = new Identifier(id);
            Value = builder.Build();
        }

        /// <summary>
        /// Deconstructs the current instance into its component parts.
        /// </summary>
        /// <param name="id">The identifier associated with this attribute.</param>
        /// <param name="value">The pattern value associated with this attribute.</param>
        public void Deconstruct(out Identifier id, out Pattern value)
        {
            id = Id;
            value = Value;
        }

        /// <summary>
        /// Factory method for constructing an <see cref="Attribute"/> from the provided identifier and pattern builder.
        /// </summary>
        /// <param name="id">The string identifier used to initialize the <see cref="Identifier"/> of the <see cref="Attribute"/>.</param>
        /// <param name="patternBuilder">The <see cref="PatternBuilder"/> used to construct the <see cref="Pattern"/> of the <see cref="Attribute"/>.</param>
        /// <returns>A newly constructed <see cref="Attribute"/> instance.</returns>
        public static Attribute From(string id, PatternBuilder patternBuilder)
        {
            return new Attribute(new Identifier(id), patternBuilder.Build());
        }

        /// <inheritdoc />
        public bool Equals(Attribute? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Value.Equals(other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Attribute)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value);
        }
    }

    /// <summary>
    /// Represents a pattern in the abstract syntax tree (AST),
    /// which is composed of multiple pattern elements.
    /// </summary>
    /// <remarks>
    /// A <see cref="Pattern"/> contains a list of elements implementing the <see cref="IPatternElement"/> interface.
    /// It can be used for both parsing and constructing localized textual content.
    /// </remarks>
    /// <seealso cref="IPatternElement"/>
    public class Pattern : IEquatable<Pattern>
    {
        /// <summary>
        /// Represents the collection of <see cref="IPatternElement"/> that form the structure
        /// of a <see cref="Pattern"/> in the abstract syntax tree (AST).
        /// </summary>
        /// <remarks>
        /// The <see cref="Elements"/> is a readonly list storing elements that implement
        /// the <see cref="IPatternElement"/> interface. These elements can be used
        /// during pattern parsing, localization, and text generation processes. Each element
        /// serves a specific role in the representation of a structured pattern, which can
        /// include <see cref="TextLiteral"/>, <see cref="Placeable"/>, and other supported pattern components.
        /// </remarks>
        public readonly List<IPatternElement> Elements;

        /// <summary>
        /// Basic constructor that takes a list of <see cref="IPatternElement"/> elements.
        /// </summary>
        public Pattern(List<IPatternElement> elements)
        {
            Elements = elements;
        }

        /// <summary>
        /// Default constructor for <see cref="Pattern"/>
        /// </summary>
        public Pattern()
        {
            Elements = new List<IPatternElement>();
        }

        /// <inheritdoc />
        public bool Equals(Pattern? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Elements.Count != other.Elements.Count)
            {
                return false;
            }

            for (var index = 0; index < Elements.Count; index++)
            {
                var patternElement = Elements[index];
                var otherPatternElement = other.Elements[index];
                if (!IPatternElement.PatternComparer.Equals(patternElement, otherPatternElement))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Pattern)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Elements.GetHashCode();
        }
    }

    /// <summary>
    /// Builder for <see cref="Pattern"/>. Used to construct Patterns programmatically.
    /// </summary>
    public class PatternBuilder
    {
        private readonly List<IPatternElement> _patternElements = new();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PatternBuilder()
        {
        }

        /// <summary>
        /// Simple constructor that creates a <see cref="Pattern"/> with <see cref="TextLiteral"/>.
        /// </summary>
        public PatternBuilder(string text)
        {
            _patternElements.Add(new TextLiteral(text));
        }

        /// <summary>
        /// Simple constructor that creates a <see cref="Pattern"/> with <see cref="NumberLiteral"/>.
        /// </summary>
        public PatternBuilder(float number)
        {
            _patternElements.Add(new Placeable(new NumberLiteral(number)));
        }

        /// <summary>
        /// Adds <c>textLiteral</c> to the pattern.
        /// </summary>
        /// <param name="textLiteral">string to be converted to <see cref="TextLiteral"/></param>
        /// <returns><see cref="PatternBuilder"/> instance with the added <see cref="TextLiteral"/>.</returns>
        public PatternBuilder AddText(string textLiteral)
        {
            _patternElements.Add(new TextLiteral(textLiteral));
            return this;
        }

        /// <summary>
        /// Adds <c>number</c> to the pattern.
        /// </summary>
        /// <param name="number">string to be converted to <see cref="NumberLiteral"/></param>
        /// <returns><see cref="PatternBuilder"/> instance with the added <see cref="NumberLiteral"/>.</returns>
        public PatternBuilder AddNumberLiteral(float number)
        {
            _patternElements.Add(new Placeable(new NumberLiteral(number)));
            return this;
        }

        /// <summary>
        /// Adds <c>number</c> to the pattern builder.
        /// </summary>
        /// <param name="number">string to be converted to <see cref="NumberLiteral"/></param>
        /// <returns><see cref="PatternBuilder"/> instance with the added <see cref="NumberLiteral"/>.</returns>
        public PatternBuilder AddNumberLiteral(double number)
        {
            _patternElements.Add(new Placeable(new NumberLiteral(number)));
            return this;
        }

        /// <summary>
        /// Adds a message reference to the pattern builder.
        /// </summary>
        /// <param name="id">The identifier of the message to be referenced.</param>
        /// <param name="attribute">The optional attribute of the message to be referenced.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added message reference.</returns>
        public PatternBuilder AddMessage(string id, string? attribute = null)
        {
            _patternElements.Add(new Placeable(new MessageReference(id, attribute)));
            return this;
        }

        /// <summary>
        /// Adds a term reference to the pattern builder.
        /// </summary>
        /// <param name="id">The identifier of the term to be referenced.</param>
        /// <param name="attribute">The optional attribute of the message to be referenced.</param>
        /// <param name="callArguments">The optional call arguments for a term.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added term reference.</returns>
        public PatternBuilder AddTermReference(string id, string? attribute = null, CallArguments? callArguments = null)
        {
            _patternElements.Add(new Placeable(new TermReference(id, attribute, callArguments)));
            return this;
        }

        /// <summary>
        /// Adds a dynamic reference to the pattern builder.
        /// </summary>
        /// <param name="id">The identifier of the dynamic reference.</param>
        /// <param name="attribute">The optional attribute of the dynamic reference.</param>
        /// <param name="callArguments">The optional call arguments for dynamic reference.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added term reference.</returns>
        public PatternBuilder AddDynamicReference(string id, string? attribute = null,
            CallArguments? callArguments = null)
        {
            _patternElements.Add(new Placeable(new DynamicReference(id, attribute, callArguments)));
            return this;
        }

        /// <summary>
        /// Adds a function reference to the pattern builder.
        /// </summary>
        /// <param name="functionName">The name of the function reference.</param>
        /// <param name="funcArgs">The arguments of the function reference.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added function reference.</returns>
        public PatternBuilder AddFunctionReference(string functionName, CallArguments funcArgs = default)
        {
            _patternElements.Add(new Placeable(new FunctionReference(functionName, funcArgs)));
            return this;
        }

        /// <summary>
        /// Adds a function reference to the pattern builder.
        /// </summary>
        /// <param name="functionName">The name of the function reference.</param>
        /// <param name="builder">A <see cref="CallArgumentsBuilder"/> that constructs the function arguments.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added function reference.</returns>
        public PatternBuilder AddFunctionReference(string functionName, CallArgumentsBuilder builder)
        {
            _patternElements.Add(new Placeable(new FunctionReference(functionName, builder.Build())));
            return this;
        }

        /// <summary>
        /// Adds a message reference to the pattern builder.
        /// </summary>
        /// <param name="messageId">The identifier of the message to be referenced.</param>
        /// <param name="attribute">The optional attribute of the message to be referenced.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added message reference.</returns>
        public PatternBuilder AddMessageReference(string messageId, string? attribute = null)
        {
            _patternElements.Add(new Placeable(new MessageReference(messageId, attribute)));
            return this;
        }

        /// <summary>
        /// Adds a <see cref="SelectExpressionBuilder"/>.
        /// </summary>
        /// <param name="selectExpressionBuilder">The <see cref="SelectExpressionBuilder"/> .</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added selection expression.</returns>
        public PatternBuilder AddSelectExpression(SelectExpressionBuilder selectExpressionBuilder)
        {
            _patternElements.Add(new Placeable(selectExpressionBuilder.Build()));
            return this;
        }

        /// <summary>
        /// Adds a pattern element to the pattern.
        /// </summary>
        /// <param name="expr">The pattern element to be added to the pattern.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added <see cref="IPatternElement"/>.</returns>
        public PatternBuilder AddExpression(IPatternElement expr)
        {
            _patternElements.Add(expr);
            return this;
        }

        /// <summary>
        /// Adds a specified <see cref="Placeable"/> to the pattern elements in the builder.
        /// </summary>
        /// <param name="placeable">The <see cref="Placeable"/> to be added.</param>
        /// <returns>A <see cref="PatternBuilder"/> instance with the added <see cref="Placeable"/>.</returns>
        public PatternBuilder AddPlaceable(Placeable placeable)
        {
            _patternElements.Add(placeable);
            return this;
        }

        /// Builds a new Pattern instance using the elements stored in the builder.
        /// <returns>A Pattern object containing the collected elements.</returns>
        public Pattern Build()
        {
            return new Pattern(_patternElements);
        }
    }


    /// <summary>
    /// Represents a unique identifier used within the abstract syntax tree (AST).
    /// </summary>
    /// <remarks>
    /// The <see cref="Identifier"/> encapsulates a sequence of characters that serves as
    /// a distinct name or label in the AST. It supports comparison and equality checks
    /// to ensure uniqueness and consistency when identifiers are used across various
    /// structures within the syntax.
    /// </remarks>
    /// <threadsafety>
    /// This class is immutable and thread-safe.
    /// </threadsafety>
    /// <seealso cref="Linguini.Syntax.Ast.Attribute"/>
    /// <seealso cref="Linguini.Syntax.Ast.Pattern"/>
    /// <seealso cref="Linguini.Syntax.Ast.AstMessage"/>
    /// <seealso cref="Linguini.Syntax.Ast.AstTerm"/>
    public class Identifier : IEquatable<Identifier>
    {
        /// <inheritdoc />
        public class IdentifierComparer : IEqualityComparer<Identifier>
        {
            /// <inheritdoc />
            public bool Equals(Identifier? x, Identifier? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name.Span.SequenceEqual(y.Name.Span);
            }

            /// <inheritdoc />
            public int GetHashCode(Identifier obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        /// <summary>
        /// Reference to an <see cref="Identifier"/> in the Fluent resource.
        /// </summary>
        /// <remarks>
        /// The <see cref="Name"/> property encapsulates the primary textual representation of an identifier.
        /// It is used for comparing and resolving the name of an <see cref="Identifier"/> across different contexts,
        /// such as parsing and processing Fluent syntax elements. The comparison logic for the <see cref="Name"/> field
        /// is implemented in the <see cref="IdentifierComparer"/> which ensures accurate equality checks.
        /// The <see cref="Name"/> is a read-only value, stored as <see cref="ReadOnlyMemory{T}"/> to improve memory efficiency.
        /// </remarks>
        public readonly ReadOnlyMemory<char> Name;

        /// <summary>
        /// Provides a default instance of <see cref="Identifier.IdentifierComparer"/> for comparing
        /// two <see cref="Identifier"/> instances based on their contents.
        /// </summary>
        /// <remarks>
        /// The <see cref="Comparer{T}"/> is used to compare attributes for equality in scenarios
        /// where attributes are part of collections or need to be evaluated for sameness.
        /// It leverages the <see cref="IdentifierComparer"/> implementation to perform custom
        /// equality checks.
        /// </remarks>
        public static readonly IdentifierComparer Comparer = new();

        /// <summary>
        /// Constructs an Identifier from a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        /// <param name="name">identifier name</param>
        public Identifier(ReadOnlyMemory<char> name)
        {
            Name = name;
        }

        /// <summary>
        /// Constructs an Identifier from a <see cref="ReadOnlyMemory{T}"/>
        /// </summary>
        /// <param name="id">identifier name</param>
        public Identifier(string id)
        {
            Name = id.AsMemory();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name.Span.ToString();
        }

        /// <inheritdoc />
        public bool Equals(Identifier? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Comparer.Equals(this, other);
        }

        /// <inheritdoc />

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }

    /// <summary>
    /// Represents the base interface for syntax expression nodes within the
    /// abstract syntax tree (AST) structure.
    /// </summary>
    /// <remarks>
    /// The <see cref="IExpression"/> is the fundamental contract for all expression
    /// types, providing a unified abstraction for constructs such as <see cref="Placeable"/>,
    /// <see cref="IInlineExpression"/>, and <see cref="SelectExpression"/>
    /// </remarks>
    public interface IExpression
    {
    }

    /// <summary>
    /// Represents the various levels of comments in Fluent syntax.
    /// </summary>
    /// <remarks>
    /// The <see cref="CommentLevel"/> enum defines the categorization of comments based on their context
    /// and significance in the resource, ranging from no comment to high-level resource descriptions.
    /// </remarks>
    /// <seealso cref="Linguini.Syntax.Ast.AstComment"/>
    /// <seealso cref="Linguini.Syntax.Parser.LinguiniParser"/>
    public enum CommentLevel : byte
    {
        /// <summary>
        /// No comment exists.
        /// </summary>
        None = 0,
        /// <summary>
        /// Basic comment
        /// <code>
        /// # Comment
        /// </code>
        /// </summary>
        Comment = 1,
        /// <summary>
        /// Group comment
        /// <code>
        /// ## GroupComment
        /// </code>
        /// </summary>
        GroupComment = 2,
        /// <summary>
        /// Resource comment
        /// <code>
        /// ### Resource Comment
        /// </code>
        /// </summary>
        ResourceComment = 3,
    }

    /// <summary>
    /// Represents an entry in the Fluent syntax abstract syntax tree (AST).
    /// </summary>
    /// <remarks>
    /// An <see cref="IEntry"/> serves as a generic representation for objects
    /// that can exist as structural components within the Fluent AST. Examples include
    /// <see cref="AstMessage"/>, <see cref="AstTerm"/>, <see cref="AstComment"/>, or a<see cref="Junk"/> elements that conform to the Fluent syntax.
    /// </remarks>
    public interface IEntry
    {
        /// <summary>
        /// Gets id of the entry
        /// </summary>
        /// <returns>Identity of the <see cref="IEntry"/></returns>
        string GetId();
    }

    /// <summary>
    /// Expression which can be placed inline and evaluated.
    /// </summary>
    public interface IInlineExpression : IExpression
    {
        /// <summary>
        /// Provides a static instance of <see cref="InlineExpressionComparer"/> to compare instances of <see cref="IInlineExpression"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Comparer"/> is used to evaluate equality between two <see cref="IInlineExpression"/> objects.
        /// It is particularly useful in scenarios where <see cref="IInlineExpression"/> instances are to be stored in
        /// collections that require equality comparisons, such as dictionaries or sets.
        /// </remarks>
        public static readonly InlineExpressionComparer Comparer = new();
    }


    /// <inheritdoc />
    public class InlineExpressionComparer : IEqualityComparer<IInlineExpression>
    {
        /// <inheritdoc />
        public bool Equals(IInlineExpression? left, IInlineExpression? right)
        {
            return (left, right) switch
            {
                (DynamicReference l, DynamicReference r) => l.Equals(r),
                (FunctionReference l, FunctionReference r) => l.Equals(r),
                (MessageReference l, MessageReference r) => l.Equals(r),
                (NumberLiteral l, NumberLiteral r) => l.Equals(r),
                (Placeable l, Placeable r) => l.Equals(r),
                (TermReference l, TermReference r) => l.Equals(r),
                (TextLiteral l, TextLiteral r) => l.Equals(r),
                (VariableReference l, VariableReference r) => l.Equals(r),
                _ => false
            };
        }

        /// <inheritdoc />
        public int GetHashCode(IInlineExpression obj)
        {
            return obj switch
            {
                DynamicReference dr => dr.GetHashCode(),
                FunctionReference fr => fr.GetHashCode(),
                MessageReference mr => mr.GetHashCode(),
                NumberLiteral nl => nl.GetHashCode(),
                Placeable p => p.GetHashCode(),
                TermReference term => term.GetHashCode(),
                TextLiteral tl => tl.GetHashCode(),
                VariableReference vr => vr.GetHashCode(),
                _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
            };
        }
    }

    /// <summary>
    /// Provides extension methods for working with the <see cref="Pattern"/> class.
    /// </summary>
    /// <seealso cref="Pattern"/>
    public static class Base
    {
        /// <summary>
        /// Converts a <see cref="Pattern"/> to a string without any context provided. This is used for debugging purposes.
        /// </summary>
        /// <param name="pattern">The <see cref="Pattern"/> to be converted.</param>
        /// <returns>A string representation of the <see cref="Pattern"/>.</returns>
        public static string Stringify(this Pattern? pattern)
        {
            var sb = new StringBuilder();
            if (pattern == null || pattern.Elements.Count <= 0) return sb.ToString();
            for (var i = 0; i < pattern.Elements.Count; i++)
            {
                sb.Append(pattern.Elements[i]);
            }

            return sb.ToString();
        }
    }
}