using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace Linguini.Syntax.Ast
{
    /// <summary>
    ///     Represents a literal text value in Fluent syntax.
    ///     This class is used to hold chunks of static text in patterns or other elements.
    /// </summary>
    public class TextLiteral : IInlineExpression, IPatternElement, IEquatable<TextLiteral>
    {
        /// <summary>
        ///     Represents the raw textual content encapsulated in the <see cref="TextLiteral" /> class.
        ///     This variable holds an immutable memory reference to a sequence of characters, which may
        ///     include Unicode data, forming the static text of Fluent patterns or other syntactical elements.
        /// </summary>
        public readonly ReadOnlyMemory<char> Value;

        /// <summary>
        ///     Constructs a <c>TextLiteral</c> from <see cref="ReadOnlyMemory{T}" />.
        /// </summary>
        /// <param name="value">input <see cref="ReadOnlyMemory{T}" /></param>
        public TextLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        /// <summary>
        ///     Constructs a <c>TextLiteral</c> from <see cref="string" />.
        /// </summary>
        /// <param name="id">input value as string</param>
        public TextLiteral(string id)
        {
            Value = id.AsMemory();
        }

        /// <inheritdoc />
        public bool Equals(TextLiteral? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Span.SequenceEqual(other.Value.Span);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.Span.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TextLiteral)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }


    /// <summary>
    ///     Represents a numeric literal value in Fluent syntax.
    ///     This class encapsulates either an integral or floating-point number and provides
    ///     functionality for storing and manipulating numeric data within Fluent expressions.
    /// </summary>
    public class NumberLiteral : IInlineExpression, IEquatable<NumberLiteral>
    {
        /// <summary>
        ///     Value of the numeric literal
        /// </summary>
        public readonly ReadOnlyMemory<char> Value;

        /// <summary>
        ///     Constructs a numeric literal from <see cref="ReadOnlyMemory{T}" />
        /// </summary>
        /// <param name="value">Numeric value as a <see cref="ReadOnlyMemory{T}" /> of characters.</param>
        public NumberLiteral(ReadOnlyMemory<char> value)
        {
            Value = value;
        }

        /// <summary>
        ///     Constructs a numeric literal from <see cref="float" />
        /// </summary>
        /// <param name="num"><c>float</c> to be converted to <see cref="NumberLiteral" /></param>
        public NumberLiteral(float num)
        {
            Value = num.ToString(CultureInfo.InvariantCulture).AsMemory();
        }

        /// <summary>
        ///     Constructs a numeric literal from <see cref="double" />
        /// </summary>
        /// <param name="num"><c>double</c> to be converted to <see cref="NumberLiteral" /></param>
        public NumberLiteral(double num)
        {
            Value = num.ToString(CultureInfo.InvariantCulture).AsMemory();
        }

        /// <inheritdoc />
        public bool Equals(NumberLiteral? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Span.SequenceEqual(other.Value.Span);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.Span.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NumberLiteral)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }


    /// <summary>
    ///     Represents a reference to a function in Fluent syntax, as a function name and the <see cref="CallArguments" />
    /// </summary>
    public class FunctionReference : IInlineExpression, IEquatable<FunctionReference>
    {
        /// <summary>
        ///     Function call arguments.
        /// </summary>
        public readonly CallArguments Arguments;

        /// <summary>
        ///     Function name
        /// </summary>
        public readonly Identifier Id;

        /// <summary>
        ///     Constructs function reference from function name, and arguments.
        /// </summary>
        /// <param name="id"><see cref="Identifier" /> as function name.</param>
        /// <param name="arguments">Function arguments.</param>
        public FunctionReference(Identifier id, CallArguments arguments)
        {
            Id = id;
            Arguments = arguments;
        }

        /// <summary>
        ///     Constructs function reference from function name, and arguments.
        /// </summary>
        /// <param name="id">string as a function name.</param>
        /// <param name="arguments">Function arguments.</param>
        public FunctionReference(string id, CallArguments arguments)
        {
            Id = new Identifier(id);
            Arguments = arguments;
        }

        /// <inheritdoc />
        public bool Equals(FunctionReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Arguments.Equals(other.Arguments);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FunctionReference)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Arguments);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id + "()";
        }
    }

    /// <summary>
    ///     Represents a reference to a Fluent message, optionally including an attribute.
    ///     This class is used to identify and interact with specific messages and their attributes
    ///     in Fluent syntax.
    /// </summary>
    public class MessageReference : IInlineExpression, IEquatable<MessageReference>
    {
        /// <summary>
        ///     Optional attribute identifier.
        /// </summary>
        public readonly Identifier? Attribute;

        /// <summary>
        ///     Message reference
        /// </summary>
        public readonly Identifier Id;

        /// <summary>
        ///     Constructs a reference to a Fluent message, from two <see cref="Identifier" />s.
        /// </summary>
        /// <param name="id">message identity</param>
        /// <param name="attribute">attribute identity</param>
        public MessageReference(Identifier id, Identifier? attribute)
        {
            Id = id;
            Attribute = attribute;
        }

        /// <summary>
        ///     Constructs a reference to a Fluent message, from two <see cref="Identifier">Identifiers</see>.
        /// </summary>
        /// <param name="id">Message identifier</param>
        /// <param name="attribute">Optional string attribute id.</param>
        public MessageReference(string id, string? attribute = null)
        {
            Id = new Identifier(id);
            if (attribute != null) Attribute = new Identifier(attribute);
        }

        /// <inheritdoc />
        public bool Equals(MessageReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Identifier.Comparer.Equals(Attribute, other.Attribute);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MessageReference)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Attribute);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Id + (Attribute != null ? "." + Attribute : "");
        }
    }

    /// <summary>
    ///     Represents a dynamic reference in Fluent syntax.
    ///     This class is used to another term with different call arguments.
    /// </summary>
    public class DynamicReference : IInlineExpression, IEquatable<DynamicReference>
    {
        /// <summary>
        ///     Optional list of <see cref="CallArguments" /> for given dynamic reference.
        /// </summary>
        public readonly CallArguments? Arguments;

        /// <summary>
        ///     Optional Attribute <see cref="Identifier" /> of given dynamic reference.
        /// </summary>
        public readonly Identifier? Attribute;

        /// <summary>
        ///     Name of referenced message.
        /// </summary>
        public readonly Identifier Id;


        /// <summary>
        ///     Constructs a dynamic reference in Fluent to reference
        ///     another term with possible call arguments and optional attribute.
        /// </summary>
        /// <param name="id">dynamic message reference.</param>
        /// <param name="attribute">attributes of dynamic reference.</param>
        /// <param name="arguments">call arguments of dynamic reference.</param>
        public DynamicReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }

        /// <summary>
        ///     Constructs a dynamic reference in Fluent used to reference
        ///     another term with possible call arguments and optional attribute.
        /// </summary>
        /// <param name="id">Dynamic reference id</param>
        /// <param name="attribute">Optional dynamic reference attribute</param>
        /// <param name="arguments">Optional dynamic reference call arguments</param>
        public DynamicReference(string id, string? attribute = null, CallArguments? arguments = null)
        {
            Id = new Identifier(id);
            if (attribute != null) Attribute = new Identifier(attribute);

            if (arguments != null) Arguments = arguments.Value;
        }

        /// <summary>
        ///     Constructs a dynamic reference in Fluent used to reference
        ///     another term with possible call arguments and optional attribute.
        /// </summary>
        /// <param name="id">Dynamic reference id.</param>
        /// <param name="attribute">Optional dynamic reference attribute.</param>
        /// <param name="callArgumentsBuilder">Optional dynamic reference <see cref="CallArgumentsBuilder" />.</param>
        public DynamicReference(string id, string? attribute, CallArgumentsBuilder? callArgumentsBuilder)
        {
            Id = new Identifier(id);
            if (attribute != null) Attribute = new Identifier(attribute);

            if (callArgumentsBuilder != null) Arguments = callArgumentsBuilder.Build();
        }

        /// <inheritdoc />
        public bool Equals(DynamicReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Identifier.Comparer.Equals(Attribute, other.Attribute) &&
                   Nullable.Equals(Arguments, other.Arguments);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DynamicReference)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Attribute, Arguments);
        }
    }

    /// <summary>
    ///     Represents a terfm reference in Fluent syntax.
    ///     This class is used to another term with different call arguments.
    /// </summary>
    public class TermReference : IInlineExpression, IEquatable<TermReference>
    {
        /// <summary>
        ///     Optional list of <see cref="CallArguments" /> for given dynamic reference.
        /// </summary>
        public readonly CallArguments? Arguments;

        /// <summary>
        ///     Optional Attribute <see cref="Identifier" /> of given dynamic reference.
        /// </summary>
        public readonly Identifier? Attribute;

        /// <summary>
        ///     Name of referenced message.
        /// </summary>
        public readonly Identifier Id;

        /// <summary>
        ///     Constructs a dynamic reference in Fluent to reference
        ///     another term with possible call arguments and optional attribute.
        /// </summary>
        /// <param name="id">dynamic message reference.</param>
        /// <param name="attribute">attributes of dynamic reference.</param>
        /// <param name="arguments">call arguments of dynamic reference.</param>
        public TermReference(Identifier id, Identifier? attribute, CallArguments? arguments)
        {
            Id = id;
            Attribute = attribute;
            Arguments = arguments;
        }

        /// <summary>
        ///     Constructs a dynamic reference in Fluent used to reference
        ///     another term with possible call arguments and optional attribute.
        /// </summary>
        /// <param name="id">Dynamic reference id</param>
        /// <param name="attribute">Optional dynamic reference attribute</param>
        /// <param name="arguments">Optional dynamic reference call arguments</param>
        public TermReference(string id, string? attribute = null, CallArguments? arguments = null)
        {
            Id = new Identifier(id);
            if (attribute != null) Attribute = new Identifier(attribute);

            if (arguments != null) Arguments = arguments.Value;
        }

        /// <summary>
        ///     Constructs a dynamic reference in Fluent used to reference
        ///     another term with possible call arguments and optional attribute.
        /// </summary>
        /// <param name="id">Dynamic reference id.</param>
        /// <param name="attribute">Optional dynamic reference attribute.</param>
        /// <param name="argumentsBuilder">Optional dynamic reference <see cref="CallArgumentsBuilder" />.</param>
        public TermReference(string id, string? attribute, CallArgumentsBuilder? argumentsBuilder)
        {
            Id = new Identifier(id);
            if (attribute != null) Attribute = new Identifier(attribute);

            if (argumentsBuilder != null) Arguments = argumentsBuilder.Build();
        }

        /// <inheritdoc />
        public bool Equals(TermReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Identifier.Comparer.Equals(Attribute, other.Attribute) &&
                   Nullable.Equals(Arguments, other.Arguments);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TermReference)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Attribute, Arguments);
        }
    
        /// <inheritdoc />
        public override string ToString()
        {
            return "-" + Id;
        }
    }

    /// <summary>
    ///     Represents a reference to a variable in Fluent syntax expressions.
    /// </summary>
    public class VariableReference : IInlineExpression, IEquatable<VariableReference>
    {
        /// <summary>
        ///     Identifies of referenced variable
        /// </summary>
        public readonly Identifier Id;

        /// <summary>
        ///     Constructs a variable reference from <see cref="Identifier" />.
        /// </summary>
        /// <param name="id">Identifier of a variable.</param>
        public VariableReference(Identifier id)
        {
            Id = id;
        }

        /// <summary>
        ///     Constructs a variable reference from <see cref="string" />.
        /// </summary>
        /// <param name="id">Identifier of a variable.</param>
        public VariableReference(string id)
        {
            Id = new Identifier(id);
        }

        /// <inheritdoc />
        public bool Equals(VariableReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier.Comparer.Equals(Id, other.Id);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VariableReference)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "$" + Id;
        }
    }

    /// <summary>
    ///     Represents a placeholder for a nesting <see cref="IExpression">experssions</see> within each other.
    /// </summary>
    public class Placeable : IInlineExpression, IPatternElementPlaceholder, IPatternElement, IEquatable<Placeable>
    {
        /// <summary>
        ///     Placed expression
        /// </summary>
        public readonly IExpression Expression;

        /// <summary>
        ///     Constructor for <c>Placeable</c>
        /// </summary>
        /// <param name="expression">Expression to be nested.</param>
        public Placeable(IExpression expression)
        {
            Expression = expression;
        }

        /// <inheritdoc />
        public bool Equals(Placeable? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Expression.Equals(other.Expression);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Placeable)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }
    }

    /// <summary>
    ///     Builder for <see cref="Placeable" />
    /// </summary>
    public class PlaceableBuilder
    {
        private readonly IExpression _expression;

        private PlaceableBuilder(IExpression expression)
        {
            _expression = expression;
        }

        /// <summary>
        ///     Factory method for creating <c>PlaceableBuilder</c> from <see cref="InlineExpressionBuilder" />.
        /// </summary>
        /// <param name="inlineBuilder">Inline builder used for creation.</param>
        /// <returns><see cref="PlaceableBuilder" /> instance created from <see cref="InlineExpressionBuilder" />.</returns>
        public static PlaceableBuilder InlineExpression(InlineExpressionBuilder inlineBuilder)
        {
            return new PlaceableBuilder(inlineBuilder.Build());
        }

        /// <summary>
        ///     Factory method for creating <c>PlaceableBuilder</c> from <see cref="SelectExpressionBuilder" />.
        /// </summary>
        /// <param name="selectorExpression">Selection expression builder used for creation.</param>
        /// <returns><see cref="PlaceableBuilder" /> instance created from <see cref="SelectExpressionBuilder" />.</returns>
        public static PlaceableBuilder InlineExpression(SelectExpressionBuilder selectorExpression)
        {
            return new PlaceableBuilder(selectorExpression.Build());
        }

        /// Builds a new Placeable instance using the elements stored in the builder.
        /// <returns>A Placeable object containing the collected elements.</returns>
        public Placeable Build()
        {
            return new Placeable(_expression);
        }
    }

    /// <summary>
    ///     Represents the arguments passed to a function or term call in Fluent syntax.
    ///     This includes a collection of positional arguments and a collection of named arguments.
    /// </summary>
    public readonly struct CallArguments : IEquatable<CallArguments>
    {
        /// <summary>
        ///     Represents a collection of positional arguments passed to a function or term call in Fluent syntax.
        ///     Each positional argument is an instance of <see cref="IInlineExpression" />, like a <see cref="NumberLiteral" /> or
        ///     <see cref="TextLiteral" />
        /// </summary>
        public readonly List<IInlineExpression> PositionalArgs;

        /// <summary>
        ///     Contains a collection of named arguments in a Fluent function or term call.
        ///     Each named argument is represented as a key-value pair, where the name acts as an identifier,
        ///     and the value is an expression providing the associated data.
        /// </summary>
        public readonly List<NamedArgument> NamedArgs;

        /// <summary>
        ///     Constructs <c>CallArguments</c> from list of positional arguments and a list of named arguments.
        /// </summary>
        /// <param name="positionalArgs">positional arguments that are consumed in way they are given</param>
        /// <param name="namedArgs">named arguments that are consumed depending on name provided</param>
        public CallArguments(List<IInlineExpression> positionalArgs, List<NamedArgument> namedArgs)
        {
            PositionalArgs = positionalArgs;
            NamedArgs = namedArgs;
        }

        /// <inheritdoc />
        public bool Equals(CallArguments other)
        {
            return PositionalArgs.SequenceEqual(other.PositionalArgs, IInlineExpression.Comparer)
                   && NamedArgs.SequenceEqual(other.NamedArgs);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is CallArguments other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(PositionalArgs, NamedArgs);
        }
    }

    /// <summary>
    ///     Represents a named argument in a function call or similar construct within Fluent syntax.
    ///     This struct pairs an name with a value, encapsulating key-value associations typically used in named arguments.
    /// </summary>
    public readonly struct NamedArgument : IEquatable<NamedArgument>
    {
        /// <summary>
        ///     Represents the name of a named argument.
        /// </summary>
        public readonly Identifier Name;

        /// <summary>
        ///     Represents the value of a named argument.
        /// </summary>
        public readonly IInlineExpression Value;

        /// <summary>
        ///     Constructor from <see cref="Identifier" /> and a provided <see cref="IInlineExpression" />.
        /// </summary>
        /// <param name="name"><see cref="Identifier" /> used as a name.</param>
        /// <param name="value"><see cref="IInlineExpression" /> used as a value.</param>
        public NamedArgument(Identifier name, IInlineExpression value)
        {
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(NamedArgument other)
        {
            return Name.Equals(other.Name) && IInlineExpression.Comparer.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is NamedArgument other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }
    }

    /// <summary>
    ///     Builder for creating <see cref="CallArguments" />
    /// </summary>
    public class CallArgumentsBuilder
    {
        private readonly List<NamedArgument> _namedArgs = new();
        private readonly List<IInlineExpression> _positionalArgs = new();

        /// <summary>
        ///     Adds an <see cref="InlineExpressionBuilder">inline expression</see> as a positional argument.
        /// </summary>
        /// <param name="arg"><see cref="InlineExpressionBuilder" /> as a positional argument</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="IInlineExpression" />.</returns>
        public CallArgumentsBuilder AddPositionalArg(InlineExpressionBuilder arg)
        {
            _positionalArgs.Add(arg.Build());
            return this;
        }

        /// <summary>
        ///     Adds an <see cref="string" /> as a positional argument.
        /// </summary>
        /// <param name="text"><see cref="string" /> as a positional argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="string" />.</returns>
        public CallArgumentsBuilder AddPositionalArg(string text)
        {
            _positionalArgs.Add(new TextLiteral(text));
            return this;
        }

        /// <summary>
        ///     Adds an <see cref="double" /> as a positional argument.
        /// </summary>
        /// <param name="number"><see cref="double" /> as a positional argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="double" />.</returns>
        public CallArgumentsBuilder AddPositionalArg(double number)
        {
            _positionalArgs.Add(new NumberLiteral(number));
            return this;
        }

        /// <summary>
        ///     Adds an <see cref="float" /> as a positional argument.
        /// </summary>
        /// <param name="number"><see cref="float" /> as a positional argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="float" />.</returns>
        public CallArgumentsBuilder AddPositionalArg(float number)
        {
            _positionalArgs.Add(new NumberLiteral(number));
            return this;
        }

        /// <summary>
        ///     Adds an <see cref="InlineExpressionBuilder">inline expression</see> as a named argument.
        /// </summary>
        /// <param name="identifier">name of named argument</param>
        /// <param name="inlineExpression"><see cref="InlineExpressionBuilder" /> as a value of named argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="IInlineExpression" />.</returns>
        public CallArgumentsBuilder AddNamedArg(string identifier, InlineExpressionBuilder inlineExpression)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), inlineExpression.Build()));
            return this;
        }

        /// <summary>
        ///     Adds an <see cref="IInlineExpression">inline expression</see> as a named argument.
        /// </summary>
        /// <param name="identifier">name of named argument</param>
        /// <param name="inlineExpression"><see cref="InlineExpressionBuilder" /> as a value of named argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="IInlineExpression" />.</returns>
        public CallArgumentsBuilder AddNamedArg(string identifier, IInlineExpression inlineExpression)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), inlineExpression));
            return this;
        }

        /// <summary>
        ///     Adds a <see cref="string" /> as a named argument.
        /// </summary>
        /// <param name="identifier">name of named argument</param>
        /// <param name="text"><see cref="string" /> as a value of named argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="string" />.</returns>
        public CallArgumentsBuilder AddNamedArg(string identifier, string text)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), new TextLiteral(text)));
            return this;
        }

        /// <summary>
        ///     Adds a <see cref="float" /> as a named argument.
        /// </summary>
        /// <param name="identifier">name of named argument</param>
        /// <param name="number"><see cref="float" /> as a value of named argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="float" />.</returns>
        public CallArgumentsBuilder AddNamedArg(string identifier, float number)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), new NumberLiteral(number)));
            return this;
        }

        /// <summary>
        ///     Adds a <see cref="double" /> as a named argument.
        /// </summary>
        /// <param name="identifier">name of named argument</param>
        /// <param name="number"><see cref="double" /> as a value of named argument.</param>
        /// <returns><see cref="CallArgumentsBuilder" /> instance with the added <see cref="double" />.</returns>
        public CallArgumentsBuilder AddNamedArg(string identifier, double number)
        {
            _namedArgs.Add(new NamedArgument(new Identifier(identifier), new NumberLiteral(number)));
            return this;
        }

        /// <summary>
        ///     Builds a new CallArgument instance using the elements stored in the builder.
        /// </summary>
        /// <returns>A <see cref="CallArguments" /> object containing collected elements.</returns>
        public CallArguments Build()
        {
            return new CallArguments(_positionalArgs, _namedArgs);
        }
    }

    /// <summary>
    ///     Builder for creating <see cref="IInlineExpression" />
    /// </summary>
    public class InlineExpressionBuilder
    {
        private readonly IInlineExpression _expression;

        private InlineExpressionBuilder(IInlineExpression expression)
        {
            _expression = expression;
        }

        /// <summary>
        ///     Creates a dynamic reference expression with the specified identifier, optional attribute,
        ///     and optional call arguments builder.
        /// </summary>
        /// <param name="id">The identifier of the dynamic reference.</param>
        /// <param name="attribute">The optional attribute of the dynamic reference.</param>
        /// <param name="callArgumentsBuilder">The optional builder for call arguments.</param>
        /// <returns>An instance of <see cref="InlineExpressionBuilder" /> representing the dynamic reference.</returns>
        public static InlineExpressionBuilder CreateDynamicReference(string id, string? attribute = null,
            CallArgumentsBuilder? callArgumentsBuilder = null)
        {
            return new InlineExpressionBuilder(new DynamicReference(id, attribute, callArgumentsBuilder));
        }

        /// <summary>
        ///     Creates a function reference expression with the specified identifier, optional attribute,
        ///     and optional call arguments builder.
        /// </summary>
        /// <param name="id">The identifier of the function.</param>
        /// <param name="callArgumentsBuilder">The optional builder for call arguments.</param>
        /// <returns>An instance of <see cref="InlineExpressionBuilder" /> representing the function reference.</returns>
        public static InlineExpressionBuilder CreateFunctionReference(string id,
            CallArgumentsBuilder callArgumentsBuilder)
        {
            return new InlineExpressionBuilder(new FunctionReference(id, callArgumentsBuilder.Build()));
        }

        /// <summary>
        ///     Creates a message reference expression with the specified identifier, and an optional attribute.
        /// </summary>
        /// <param name="id">The identifier of the dynamic reference.</param>
        /// <param name="attribute">The optional attribute of the message reference.</param>
        /// <returns>An instance of <see cref="InlineExpressionBuilder" /> representing the message reference.</returns>
        public static InlineExpressionBuilder CreateMessageReference(string id, string? attribute = null)
        {
            return new InlineExpressionBuilder(new MessageReference(id, attribute));
        }

        /// <summary>
        ///     Creates a <see cref="NumberLiteral" /> from a <see cref="double" /> value.
        /// </summary>
        /// <param name="numberLiteral">
        ///     The numeric value of type <see cref="double" /> to create a <see cref="NumberLiteral" />
        ///     from.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="InlineExpressionBuilder" /> initialized with the created
        ///     <see cref="NumberLiteral" />.
        /// </returns>
        public static InlineExpressionBuilder CreateNumber(double numberLiteral)
        {
            return new InlineExpressionBuilder(new NumberLiteral(numberLiteral));
        }

        /// <summary>
        ///     Creates a <see cref="NumberLiteral" /> from a <see cref="float" /> value.
        /// </summary>
        /// <param name="numberLiteral">
        ///     The numeric value of type <see cref="float" /> to create a <see cref="NumberLiteral" />
        ///     from.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="InlineExpressionBuilder" /> initialized with the created
        ///     <see cref="NumberLiteral" />.
        /// </returns>
        public static InlineExpressionBuilder CreateNumber(float numberLiteral)
        {
            return new InlineExpressionBuilder(new NumberLiteral(numberLiteral));
        }

        /// <summary>
        ///     Creates an <see cref="InlineExpressionBuilder" /> from a <see cref="Placeable" />.
        /// </summary>
        /// <param name="placeable">The <see cref="Placeable" /> used to initialize the builder.</param>
        /// <returns>An instance of <see cref="InlineExpressionBuilder" /> initialized with the specified <see cref="Placeable" />.</returns>
        public static InlineExpressionBuilder CreatePlaceable(Placeable placeable)
        {
            return new InlineExpressionBuilder(placeable);
        }

        /// <summary>
        ///     Creates an <see cref="InlineExpressionBuilder" /> from a <see cref="PlaceableBuilder" />.
        /// </summary>
        /// <param name="placeable">The <see cref="PlaceableBuilder" /> used to initialize the builder.</param>
        /// <returns>
        ///     An instance of <see cref="InlineExpressionBuilder" /> initialized with the specified
        ///     <see cref="PlaceableBuilder" />.
        /// </returns>
        public static InlineExpressionBuilder CreatePlaceable(PlaceableBuilder placeable)
        {
            return new InlineExpressionBuilder(placeable.Build());
        }


        /// <summary>
        ///     Creates a term reference expression with the specified identifier, optional attribute,
        ///     and optional call arguments builder.
        /// </summary>
        /// <param name="id">The identifier of the term reference.</param>
        /// <param name="attribute">The optional attribute of the term reference.</param>
        /// <param name="callArgumentsBuilder">The optional builder for call arguments.</param>
        /// <returns>An instance of <see cref="InlineExpressionBuilder" /> representing the term reference.</returns>
        public static InlineExpressionBuilder CreateTermReference(string id, string? attribute = null,
            CallArgumentsBuilder? callArgumentsBuilder = null)
        {
            return new InlineExpressionBuilder(new TermReference(id, attribute, callArgumentsBuilder));
        }

        /// <summary>
        ///     Creates an instance of <see cref="InlineExpressionBuilder" /> containing a <see cref="TextLiteral" /> with the
        ///     given text.
        /// </summary>
        /// <param name="textLiteral">The text to construct the <see cref="TextLiteral" /> from.</param>
        /// <returns>An <see cref="InlineExpressionBuilder" /> wrapping the created <see cref="TextLiteral" />.</returns>
        public static InlineExpressionBuilder CreateTextLiteral(string textLiteral)
        {
            return new InlineExpressionBuilder(new TextLiteral(textLiteral));
        }

        /// <summary>
        ///     Creates an instance of <see cref="InlineExpressionBuilder" /> containing a <see cref="VariableReference" />.
        /// </summary>
        /// <param name="textLiteral">The text used to counstruct a <see cref="VariableReference" /> from.</param>
        /// <returns>An <see cref="InlineExpressionBuilder" /> wrapping the created <see cref="VariableReference" />.</returns>
        public static InlineExpressionBuilder CreateVariableReferences(string textLiteral)
        {
            return new InlineExpressionBuilder(new VariableReference(textLiteral));
        }


        /// <summary>
        ///     Builds a new <see cref="IInlineExpression" /> instance using the elements stored in the builder.
        /// </summary>
        /// <returns>A <see cref="IInlineExpression" /> object containing collected elements.</returns>
        public IInlineExpression Build()
        {
            return _expression;
        }
    }

    /// <summary>
    ///     Represents a selection expression used for choosing among multiple options (variants)
    ///     based on a selector value.
    /// </summary>
    /// <remarks>
    ///     A <c>SelectExpression</c> consists of a selector, which determines the matching criteria,
    ///     and a collection of variants representing the possible choices.
    /// </remarks>
    public class SelectExpression : IExpression, IEquatable<SelectExpression>
    {
        /// <summary>
        ///     Represents the element in a selection expression that determines which of the variants is to be evaluated.
        /// </summary>
        public readonly IInlineExpression Selector;

        /// <summary>
        ///     Represents a collection of selectable variants.
        ///     Each variant encapsulates a choice that may include a key, a value, and a default indicator.
        /// </summary>
        public readonly List<Variant> Variants;

        /// <summary>
        ///     Consturcts a selection expression from a selector and a list of variants.
        /// </summary>
        /// <param name="selector"><see cref="IInlineExpression" /> used to determine which variant to use.</param>
        /// <param name="variants">List of variants to be chosen for evaluation.</param>
        public SelectExpression(IInlineExpression selector, List<Variant> variants)
        {
            Selector = selector;
            Variants = variants;
        }

        /// <inheritdoc />
        public bool Equals(SelectExpression? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IInlineExpression.Comparer.Equals(Selector, other.Selector)
                   && Variants.SequenceEqual(other.Variants);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SelectExpression)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Selector, Variants);
        }
    }

    /// <summary>
    ///     Represents a builder for constructing a <see cref="SelectExpression" />.
    ///     This class is used to define and configure a selection expression with multiple
    ///     variants and an optional default variant.
    /// </summary>
    public class SelectExpressionBuilder : IAddVariant
    {
        private readonly IInlineExpression _selector;
        private readonly List<Variant> _variants = new();

        /// <summary>
        ///     Creates selection builder from <see cref="IInlineExpression" />
        /// </summary>
        /// <param name="selector">Expression to be used as a selector</param>
        public SelectExpressionBuilder(IInlineExpression selector)
        {
            _selector = selector;
        }


        /// <inheritdoc />
        public IAddVariant AddVariant(string selector, PatternBuilder patternBuilder)
        {
            _variants.Add(new Variant(selector, patternBuilder));
            return this;
        }

        /// <inheritdoc />
        public IAddVariant AddVariant(float selector, PatternBuilder patternBuilder)
        {
            _variants.Add(new Variant(selector, patternBuilder));
            return this;
        }

        /// <inheritdoc />
        public SelectExpressionBuilder SetDefault(int? defaultSelector = null)
        {
            var selector = defaultSelector is >= 0 && defaultSelector < _variants.Count
                ? _variants.Count - 1
                : defaultSelector!.Value;
            _variants[selector].InternalDefault = true;
            return this;
        }

        /// <summary>
        ///     Builds and returns a new instance of <see cref="SelectExpression" />
        ///     with the configured selector and variants.
        /// </summary>
        /// <returns>A new instance of <see cref="SelectExpression" />.</returns>
        public SelectExpression Build()
        {
            return new SelectExpression(_selector, _variants);
        }
    }

    /// <summary>
    ///     Common interface for adding variants to the <see cref="SelectExpressionBuilder" />
    /// </summary>
    public interface IAddVariant
    {
        /// <summary>
        ///     Adds a textual selector to a <see cref="PatternBuilder" />
        /// </summary>
        /// <param name="selector">Text used in selector.</param>
        /// <param name="patternBuilder">Pattern to which the selected builder will resolve.</param>
        /// <returns>A <see cref="SelectExpressionBuilder" /> with added variant.</returns>
        public IAddVariant AddVariant(string selector, PatternBuilder patternBuilder);

        /// <summary>
        ///     Adds a numerical selector to a <see cref="PatternBuilder" />
        /// </summary>
        /// <param name="selector">Float used in selector.</param>
        /// <param name="patternBuilder">Pattern to which the selected builder will resolve.</param>
        /// <returns>A <see cref="SelectExpressionBuilder" /> with added variant.</returns>
        public IAddVariant AddVariant(float selector, PatternBuilder patternBuilder);

        /// <summary>
        ///     Sets default selector to given position.
        /// </summary>
        /// <param name="defaultSelector">Which variant will become default (picked in case of no match).</param>
        /// <returns>A <see cref="SelectExpressionBuilder" /> with given default variant.</returns>
        public SelectExpressionBuilder SetDefault(int? defaultSelector = null);
    }

    /// <summary>
    ///     Which value is used in <see cref="Variant" />
    /// </summary>
    public enum VariantType : byte
    {
        /// <summary>
        ///     Textual variant value.
        /// </summary>
        Identifier,

        /// <summary>
        ///     Numerical variant value.
        /// </summary>
        NumberLiteral
    }


    /// <summary>
    ///     Represents a variant value in Fluent syntax.
    ///     A Variant is typically used as part of a SelectExpression
    ///     to define multiple possible outcomes for a given selector.
    /// </summary>
    public class Variant : IEquatable<Variant>
    {
        /// <summary>
        ///     Represents the identifier or numeric key used to distinguish a specific
        ///     variant within a Fluent syntax <see cref="Variant" />.
        ///     This variable holds a read-only span of characters, encapsulating
        ///     either an identifier or a numerical string, depending on the
        ///     variant's <see cref="VariantType" />.
        /// </summary>
        public readonly ReadOnlyMemory<char> Key;

        /// <summary>
        ///     Defines the type of the variant key used within the <see cref="Variant" /> class.
        ///     This field indicates whether the variant is identified by an <see cref="VariantType.Identifier" />
        ///     or a <see cref="VariantType.NumberLiteral" />, determining how its key is interpreted and processed.
        /// </summary>
        public readonly VariantType Type;

        /// <summary>
        ///     Is the Variant picked when no other variant matches.
        /// </summary>
        protected internal bool InternalDefault;

        /// <summary>
        ///     Pattern returned when the <see cref="SelectExpression" /> picks this variant.
        /// </summary>
        protected internal Pattern InternalValue;

        /// <summary>
        ///     Constructs a simplified <c>Variant</c> from a type and key.
        /// </summary>
        /// <param name="type">Which <see cref="VariantType" /> is this <c>Variant</c>.</param>
        /// <param name="key">Key of the <c>Variant</c>.</param>
        public Variant(VariantType type, ReadOnlyMemory<char> key)
        {
            Type = type;
            Key = key;
            InternalValue = new Pattern();
            InternalDefault = false;
        }

        /// <summary>
        ///     Constructs a <c>Variant</c> from a type, key, pattern and if the variant is default.
        /// </summary>
        /// <param name="type">Which <see cref="VariantType" /> is this <c>Variant</c>.</param>
        /// <param name="key">Key of the <c>Variant</c>.</param>
        /// <param name="pattern">Value of the <c>Variant</c>.</param>
        /// <param name="isDefault">If the <c>Variant</c> is default.</param>
        public Variant(VariantType type, ReadOnlyMemory<char> key, Pattern pattern, bool isDefault = false)
        {
            Type = type;
            Key = key;
            InternalValue = pattern;
            InternalDefault = isDefault;
        }

        /// <summary>
        ///     Constructs a string keyed <c>Variant</c> with a given <see cref="PatternBuilder" /> as value.
        /// </summary>
        /// <param name="key">string used as a key.</param>
        /// <param name="builder"><see cref="PatternBuilder" /> that represents value.</param>
        public Variant(string key, PatternBuilder builder)
        {
            Type = VariantType.Identifier;
            Key = key.AsMemory();
            InternalValue = builder.Build();
            InternalDefault = false;
        }


        /// <summary>
        ///     Constructs a flaot keyed <c>Variant</c> with a given <see cref="PatternBuilder" /> as value.
        /// </summary>
        /// <param name="key">number used as a key.</param>
        /// <param name="builder"><see cref="PatternBuilder" /> that represents value.</param>
        public Variant(float key, PatternBuilder builder)
        {
            Type = VariantType.NumberLiteral;
            Key = key.ToString(CultureInfo.InvariantCulture).AsMemory();
            InternalValue = builder.Build();
            InternalDefault = false;
        }

        /// <summary>
        ///     Retrieves the resolved <see cref="Pattern" /> value associated with this <see cref="Variant" /> instance.
        /// </summary>
        public Pattern Value => InternalValue;

        /// <summary>
        ///     Return whether this <c>Variant</c> is default, or not.
        /// </summary>
        public bool IsDefault => InternalDefault;

        /// <inheritdoc />
        public bool Equals(Variant? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && Key.Span.SequenceEqual(other.Key.Span) &&
                   InternalDefault == other.InternalDefault &&
                   InternalValue.Equals(other.InternalValue);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Variant)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, Key, InternalDefault, InternalValue);
        }
    }
}