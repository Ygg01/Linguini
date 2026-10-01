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
public class SerializationErrorsTest
{
    private const string Placeable1 = @"{
        ""type"": ""XXX""
    }";

    private const string Placeable2 = @"{
        ""type"": ""Placeable"",
        ""expression"": { ""type"": ""x""}
    }";

    private const string Resource1 = @"{
        ""type"": ""Resource"",
        ""body"": [
            {
                ""type"": ""Testing"",
                ""content"": ""Test1""
            }
        ]
    }";

    private const string MessageReference1 = @"{
        ""type"": ""MessageReference"",
        ""attribute"": []
    }";

    private const string AstMessage1 = @"{
        ""type"": ""Message"",
        ""id"": 3,
        ""attribute"": []
    }";

    private const string AstMessage2 = @"{
        ""type"": ""Message""
    }";


    private const string DynamicReference1 = @"{
        ""type"": ""MessageReference"",
        ""attribute"": []
    }";

    private const string VariableReference1 = @"{
        ""type"": ""VariableReference"",
        ""attribute"": []
    }";

    private const string CallArguments1 = @"{
        ""type"": ""CallArguments"",
        ""attribute"": []
    }";

    private const string Junk1 = @"{
        ""type"": ""Junk""
    }";

    private const string Selector1 = @"{
        ""type"": ""SelectExpression""
    }";

    private const string Selector2 = @"{
        ""type"": ""SelectExpression"",
        ""selector"": []
    }";

    private const string Selector3 = @"{
        ""type"": ""SelectExpression"",
        ""selector"": 2,
        ""variants"": 3
    }";

    private const string Selector4 = @"{
        ""type"": ""SelectExpression"",
        ""selector"": { ""type"": ""test""}
    }";

    private const string Selector5 = @"{
        ""type"": ""SelectExpression"",
        ""selector"": { ""type"": ""TextLiteral"", ""value"": ""test"" },
        ""variants"": 3
    }";

    private const string FunctionRef1 = @"{
        ""type"": ""FunctionReference"",
        ""id"": {}
    }";

    private const string FunctionRef2 = @"{
        ""type"": ""FunctionReference"",
        ""id"": {""type"":""Identifier"", ""name"": ""tist""},
        ""arguments"": {}
    }";

    private const string AttributeRef1 = @"{
        ""type"": ""Attribute"",
        ""id"": {""type"":""Identifier"", ""name"": ""tist""},
        ""value"": []
    }";

    private const string AttributeRef2 = @"{
        ""type"": ""xxz"",
        ""id"": {""type"":""Identifier"", ""name"": ""tist""},
        ""value"": []
    }";


    private const string AttributeRef3 = @"{
        ""type"": ""Attribute"",
        ""unknwon"": 3,
        ""id"": {""type"":""Identifier"", ""name"": ""tist""},
        ""value"": []
    }";

    private const string NamedArgs1 = @"{
        ""type"": ""Attribute"",
        ""unknwon"": 3,
        ""id"": {""type"":""Identifier"", ""name"": ""tist""},
        ""value"": []
    }";

    private const string Pattern = @"{
        ""type"": ""Pattern"",
        ""elements"": [
            {
                ""type"": ""Placeable"",
                ""expression_not"": []
            }
        ]
    }";

    private const string Identifier1 = @"{
        ""type"": ""Identifier"",
        ""unknown"": null
    }";

    private const string Identifier2 = @"{
        ""type"": ""Identifier"",
        ""name"": null
    }";

    private const string Variant1 = @"{
        ""type"": ""Variant""
    }";

    private const string Variant2 = @"{
        ""type"": ""Variant"",
        ""key"": {""type"": ""Junk""}
    }";

    private const string Variant3 = @"{
        ""type"": ""Variant"",
        ""key"": {""type"": ""Identifier"", ""name"": ""variant""},
        ""value"": {""type"": ""Junk""}
    }";

    private const string ParseError1 = @"{
        ""kind"": ""ExpectedToken"",
        ""message"": 3.9
    }";

    private const string ParseError2 = @"{
        ""kind"": ""Unknown"",
        ""message"": ""This sucks""
    }";

    private const string ParseError3 = @"{
        ""kind"": ""ExpectedToken"",
        ""message"": ""This sucks"",
        ""row"": ""Should be number""
    }";

    private const string ParseError4 = @"{
        ""kind"": ""ExpectedToken"",
        ""message"": ""This sucks"",
        ""row"": 3,
        ""position"": { ""start"": ""2"", ""end"": 3 }
    }";

    private const string ParseError5 = @"{
        ""kind"": ""ExpectedToken"",
        ""message"": ""This sucks"",
        ""row"": 3,
        ""position"": { ""start"": 2, ""end"": 3 },
        ""slice"": 3
    }";

    private const string AstTerm1 = @"{
        ""type"": ""Term"",
        ""id"": {}
    }";

    private const string AstTerm2 = @"{
        ""type"": ""Term"",
        ""id"": {""type"": ""Identifier"", ""name"": ""id-x"" },
        ""value"": { ""type"": ""wrong""}
    }";

    private const string AstTerm3 = @"{
        ""type"": ""Term"",
        ""id"": {""type"": ""Identifier"", ""name"": ""id-x"" },
        ""value"": {  
            ""type"": ""Pattern"",
            ""elements"": [
                {
                    ""type"": ""TextLiteral"",
                    ""value"": ""text""
                }
            ]
        },
        ""attributes"": {}
    }";

    private const string AstTerm4 = @"{
        ""type"": ""Term"",
        ""id"": {""type"": ""Identifier"", ""name"": ""id-x"" },
        ""value"": {  
            ""type"": ""Pattern"",
            ""elements"": [
                {
                    ""type"": ""TextLiteral"",
                    ""value"": ""text""
                }
            ]
        },
        ""attributes"": [
            {""type"": ""Attribute"", ""id"": {""type"": ""Identifier"", ""name"": ""attr-x"" }, ""value"": {} }
        ]
    }";

    private const string AstTerm5 = @"{
        ""type"": ""Term"",
        ""id"": {""type"": ""Identifier"", ""name"": ""id-x"" },
        ""value"": {  
            ""type"": ""Pattern"",
            ""elements"": [
                {
                    ""type"": ""TextLiteral"",
                    ""value"": ""text""
                }
            ]
        },
        ""comment"": { ""type"": ""Comment"", ""unknown"": 2}
    }";

    private static IEnumerable<TestCaseData> ErrorExamples()
    {
        yield return new TestCaseData(MessageReference1, typeof(MessageReference)).Returns("JsonException");
        yield return new TestCaseData(DynamicReference1, typeof(DynamicReference)).Returns("JsonException");
        yield return new TestCaseData(VariableReference1, typeof(VariableReference)).Returns("JsonException");
        // Function Reference test
        yield return new TestCaseData(Junk1, typeof(FunctionReference)).Returns("JsonException");
        yield return new TestCaseData(FunctionRef1, typeof(FunctionReference)).Returns("JsonException");
        yield return new TestCaseData(FunctionRef2, typeof(FunctionReference)).Returns("JsonException");
        // Attribute tests
        yield return new TestCaseData(AttributeRef1, typeof(Attribute)).Returns("JsonException");
        yield return new TestCaseData(AttributeRef2, typeof(Attribute)).Returns("JsonException");
        yield return new TestCaseData(AttributeRef3, typeof(Attribute)).Returns("JsonException");

        // Function Selector test
        yield return new TestCaseData(Junk1, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector1, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector2, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector3, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector4, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector5, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Junk1, typeof(Variant)).Returns("JsonException");
        yield return new TestCaseData(Variant1, typeof(Variant)).Returns("JsonException");
        yield return new TestCaseData(Variant2, typeof(Variant)).Returns("JsonException");
        yield return new TestCaseData(Variant3, typeof(Variant)).Returns("JsonException");


        yield return new TestCaseData("[]", typeof(Attribute)).Returns("JsonException");
        yield return new TestCaseData("{}", typeof(NamedArgument)).Returns("JsonException");
        yield return new TestCaseData(NamedArgs1, typeof(NamedArgument)).Returns("JsonException");
        yield return new TestCaseData(Junk1, typeof(Placeable)).Returns("JsonException");
        yield return new TestCaseData(Pattern, typeof(Pattern)).Returns("JsonException");
        yield return new TestCaseData("[]", typeof(Identifier)).Returns("JsonException");
        yield return new TestCaseData(Junk1, typeof(Identifier)).Returns("JsonException");
        yield return new TestCaseData(Identifier1, typeof(Identifier)).Returns("JsonException");
        yield return new TestCaseData(Identifier2, typeof(Identifier)).Returns("JsonException");
        yield return new TestCaseData(Placeable1, typeof(CallArguments)).Returns("JsonException");
        yield return new TestCaseData(CallArguments1, typeof(CallArguments)).Returns("JsonException");

        yield return new TestCaseData(ParseError1, typeof(ParseError)).Returns("JsonException");
        yield return new TestCaseData(ParseError2, typeof(ParseError)).Returns("JsonException");
        yield return new TestCaseData(ParseError3, typeof(ParseError)).Returns("JsonException");
        yield return new TestCaseData(ParseError4, typeof(ParseError)).Returns("JsonException");
        yield return new TestCaseData(ParseError5, typeof(ParseError)).Returns("JsonException");
        // Resource level
        yield return new TestCaseData(AstTerm1, typeof(AstTerm)).Returns("JsonException");
        yield return new TestCaseData(AstTerm2, typeof(AstTerm)).Returns("JsonException");
        yield return new TestCaseData(AstTerm3, typeof(AstTerm)).Returns("JsonException");
        yield return new TestCaseData(AstTerm4, typeof(AstTerm)).Returns("JsonException");
        yield return new TestCaseData(AstTerm5, typeof(AstTerm)).Returns("JsonException");
        yield return new TestCaseData(AstMessage1, typeof(AstMessage)).Returns("JsonException");
        yield return new TestCaseData(AstMessage2, typeof(AstMessage)).Returns("JsonException");
        yield return new TestCaseData(Junk1, typeof(Junk)).Returns("JsonException");
        yield return new TestCaseData(Placeable1, typeof(Placeable)).Returns("JsonException");
        yield return new TestCaseData(Placeable2, typeof(Placeable)).Returns("JsonException");
        yield return new TestCaseData("{}", typeof(Resource)).Returns("JsonException");
        yield return new TestCaseData(Resource1, typeof(Resource)).Returns("JsonException");
    }

    private static IEnumerable<TestCaseData> SingleExamples()
    {
        yield return new TestCaseData(AstTerm5, typeof(AstTerm)).Returns("JsonException");
    }

    [Test]
    [TestCaseSource(nameof(ErrorExamples))]
    [Parallelizable]
    public string TestErrors(string jsonString, Type deserializeType)
    {
        try
        {
            JsonSerializer.Deserialize(jsonString, deserializeType, Options);
        }
        catch (Exception e)
        {
            return e.GetType().Name;
        }

        return "";
    }

    [Test]
    [TestCaseSource(nameof(SingleExamples))]
    [Parallelizable]
    public string TestSingleErrors(string jsonString, Type deserializeType)
    {
        try
        {
            JsonSerializer.Deserialize(jsonString, deserializeType, Options);
        }
        catch (Exception e)
        {
            return e.GetType().Name;
        }

        return "";
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