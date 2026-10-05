using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser.Error;
using NUnit.Framework;
using Attribute = Linguini.Syntax.Ast.Attribute;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace Linguini.Serialization.Test;

[TestFixture]
public class SerializeAndDeserializeTest
{
    [Test]
    [TestCaseSource(nameof(SyntaxExamples))]
    [TestCaseSource(nameof(ParseErrorExamples))]
    [TestCaseSource(nameof(ResourcesExample))]
    [Parallelizable]
    public void RoundTripTest(object actual)
    {
        // Serialize the object to JSON string.
        var jsonString = JsonSerializer.Serialize(actual, Options);

        // Deserialize the JSON string back into an object.
        Debug.Assert(actual != null, nameof(actual) + " != null");
        var deserializedObject = JsonSerializer.Deserialize(jsonString, actual.GetType(), Options);

        // Now you have a 'deserializedObject' which should be equivalent to the original 'expected' object.
        Assert.That(deserializedObject, Is.Not.Null);
        Assert.That(deserializedObject, Is.EqualTo(actual));
    }

    public static IEnumerable<object> ParseErrorExamples()
    {
        var errorWithSlice = ParseError.ExpectedCharRange("aZ", 3, 2);
        errorWithSlice.Slice = new Range(2, 3);
        // Errors
        yield return errorWithSlice;
        yield return ParseError.DuplicatedNamedArgument("aa", 3, 2);
        yield return ParseError.ExpectedCharRange("abc", 34, 19);
        yield return ParseError.ExpectedInlineExpression(3, 2);
        yield return ParseError.ExpectedLiteral(2, 44);
        yield return ParseError.ExpectedMessageField("z".AsMemory(), 2, 4, 6);
        yield return ParseError.ExpectedToken('c', '?', 3, 11);
        yield return ParseError.ExpectedTermField("z", 0, 41, 13245);
        yield return ParseError.ExpectedSimpleExpressionAsSelector(33, 431);
        yield return ParseError.ForbiddenCallee(45, 390);
        yield return ParseError.InvalidUnicodeEscapeSequence("zxc", 634, 72);
        yield return ParseError.MessageAttributeAsSelector(11, 96);
        yield return ParseError.MissingDefaultVariant(04, 78);
        yield return ParseError.MessageReferenceAsSelector(2222, 112);
        yield return ParseError.MissingValue(0, 2);
        yield return ParseError.MultipleDefaultVariants(94, 1113);
        yield return ParseError.PositionalArgumentFollowsNamed(48, 888);
        yield return ParseError.TermAttributeAsPlaceable(39, 912);
        yield return ParseError.TermReferenceAsSelector(44, 13);
        yield return ParseError.UnknownEscapeSequence('?', 123, 44);
        yield return ParseError.UnbalancedClosingBrace(33, 1134);
        yield return ParseError.UnterminatedStringLiteral(87, 99);
    }

    public static IEnumerable<object> SyntaxExamples()
    {
        // Other serializers
        yield return new Attribute("desc", Pattern.From("description"));
        yield return new Placeable(InlineExpressionBuilder.CreateDynamicReference("dyn-r").Build());
        yield return new Placeable(InlineExpressionBuilder.CreateMessageReference("msg-r").Build());
        yield return new Placeable(InlineExpressionBuilder.CreateVariableReferences("var-r").Build());
        yield return new Placeable(InlineExpressionBuilder.CreateTermReference("term-r").Build());
        yield return new Placeable(InlineExpressionBuilder.CreatePlaceable(new Placeable(new TextLiteral("id"))).Build());
        yield return new Placeable(InlineExpressionBuilder.CreateTextLiteral("32.0").Build());
        yield return new Placeable(InlineExpressionBuilder
                                       .CreateFunctionReference(
                                           "func-ref",
                                           CallArguments.Builder().AddPositionalArg(3.09d))
                                       .Build()
        );
        var selectionBuilder = new SelectExpressionBuilder(new TermReference("x", "y"))
            .AddVariant("x", Pattern.Builder().AddText("z"))
            .SetDefault(0)
            .Build();
        yield return new Placeable(selectionBuilder);
        var callArgs = CallArguments.Builder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3);
        yield return callArgs.Build();
        yield return new AstComment(CommentLevel.Comment, new List<ReadOnlyMemory<char>> { "test".AsMemory() });
        yield return new AstComment(CommentLevel.GroupComment, new List<ReadOnlyMemory<char>> { "test".AsMemory() });
        yield return new AstComment(CommentLevel.ResourceComment, new List<ReadOnlyMemory<char>> { "test".AsMemory() });

        yield return new TermReference("dyn", "attr", CallArguments.Builder());

        yield return new DynamicReference("dyn", "attr", CallArguments.Builder()
                                              .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
                                              .AddNamedArg("y", 3));
        yield return new FunctionReference("foo", CallArguments.Builder()
                                               .AddPositionalArg(3)
                                               .AddNamedArg(
                                                   "test", InlineExpressionBuilder.CreateTermReference("x", "y"))
                                               .Build()
        );
        yield return new Identifier("test");
        yield return new NamedArgument("arg1", InlineExpressionBuilder.CreateDynamicReference("x", "y").Build());
        yield return new NamedArgument("arg2", InlineExpressionBuilder.CreateTermReference("term", "ref").Build());
        yield return new NamedArgument("arg3", InlineExpressionBuilder.CreateFunctionReference("term", CallArguments.Builder()).Build());
        yield return new NamedArgument("arg4", InlineExpressionBuilder.CreatePlaceable(new Placeable(new TextLiteral("id"))).Build());
        yield return new NamedArgument("arg5", InlineExpressionBuilder.CreateDynamicReference("x", "y").Build());
        yield return new MessageReference("message", "attribute");
        yield return Pattern.Builder().AddText("text ").AddMessage("x").AddText(" more text").Build();
        yield return new SelectExpressionBuilder(new VariableReference("x"))
            .AddVariant("one", Pattern.Builder().AddText("select 1"))
            .AddVariant("other", Pattern.Builder().AddText("select other"))
            .SetDefault(1)
            .Build();
        yield return new TermReference("x", "y");
        yield return new VariableReference("x");
        yield return new Variant(2.0f, Pattern.Builder().AddNumberLiteral(3));
    }

    public static IEnumerable<object> ResourcesExample()
    {
        yield return new Resource(
            new List<IEntry>
            {
                new AstComment(CommentLevel.ResourceComment, new List<ReadOnlyMemory<char>> { "test3".AsMemory() }),
                AstTerm.Builder("id", Pattern.From("test")).Build(),
                new AstComment(CommentLevel.Comment, new List<ReadOnlyMemory<char>> { "test2".AsMemory() }),
                new AstComment(CommentLevel.GroupComment, new List<ReadOnlyMemory<char>> { "test3".AsMemory() }),
                new Junk("junkie"),
                AstMessage.Builder("message").SetPattern(Pattern.From("xyz")).Build()
            },
            new List<ParseError>());
        var inlineExpressionBuilder = InlineExpressionBuilder
            .CreateFunctionReference("x", CallArguments.Builder().AddPositionalArg("e"));
        yield return new Resource(
            new List<IEntry>
            {
                AstMessage.Builder("message")
                    .SetPattern(Pattern.Builder()
                                    .AddNumberLiteral(3.0)
                                    .AddPlaceable(Placeable.FromInline(inlineExpressionBuilder))
                                    .AddFunctionReference("COUNT")
                                    .AddTermReference("ref-term")
                                    .AddDynamicReference("dyn-rf")
                    )
                    .Build()
            },
            new List<ParseError>());
        yield return new Junk("Test".AsMemory());
        yield return AstTerm.Builder("z", Pattern.From("x"))
            .SetComment("my comment")
            .AddAttribute( new Attribute("x0", Pattern.From(32)))
            .Build();
        yield return new AstMessage(
            new Identifier("x"),
            Pattern.From(3),
            new List<Attribute>
            {
                new("attr1", Pattern.From("value1")),
                new("attr2", Pattern.From("value2"))
            },
            AstLocation.Empty,
            new AstComment(CommentLevel.ResourceComment, new List<ReadOnlyMemory<char>>
            {
                "test".AsMemory()
            }));
    }


    private static readonly JsonSerializerOptions Options = new()
    {
        IgnoreReadOnlyFields = false,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new AttributeSerializer(),
            new CallArgumentsSerializer(),
            new CommentSerializer(),
            new FunctionReferenceSerializer(),
            new IdentifierSerializer(),
            new JunkSerializer(),
            new MessageReferenceSerializer(),
            new MessageSerializer(),
            new DynamicReferenceSerializer(),
            new NamedArgumentSerializer(),
            new ParseErrorSerializer(),
            new PatternSerializer(),
            new PlaceableSerializer(),
            new ResourceSerializer(),
            new PlaceableSerializer(),
            new SelectExpressionSerializer(),
            new TermReferenceSerializer(),
            new TermSerializer(),
            new VariantSerializer(),
            new VariableReferenceSerializer()
        }
    };
}