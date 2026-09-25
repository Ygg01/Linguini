using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using NUnit.Framework;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Linguini.Serialization.Test;

[TestFixture]
public class SerializationErrorsTest
{
    private const string PlaceableError1 = @"{
        ""type"": ""XXX""
    }";
    private const string PlaceableError2 = @"{
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
    
    private const string JunkError1 = @"{
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
    
    private static IEnumerable<TestCaseData> ErrorExamples()
    {
        yield return new TestCaseData(MessageReference1, typeof(MessageReference)).Returns("JsonException");
        yield return new TestCaseData(DynamicReference1, typeof(DynamicReference)).Returns("JsonException");
        yield return new TestCaseData(VariableReference1, typeof(VariableReference)).Returns("JsonException");
        yield return new TestCaseData(PlaceableError1, typeof(CallArguments)).Returns("JsonException");
        yield return new TestCaseData(CallArguments1, typeof(CallArguments)).Returns("JsonException");
        yield return new TestCaseData(JunkError1, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector1, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector2, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector3, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector4, typeof(SelectExpression)).Returns("JsonException");
        yield return new TestCaseData(Selector5, typeof(SelectExpression)).Returns("JsonException");

        
        yield return new TestCaseData(AstMessage1, typeof(AstMessage)).Returns("JsonException");
        yield return new TestCaseData(AstMessage2, typeof(AstMessage)).Returns("JsonException");
        
        yield return new TestCaseData(JunkError1, typeof(Junk)).Returns("JsonException");
        yield return new TestCaseData(PlaceableError1, typeof(Placeable)).Returns("JsonException");
        yield return new TestCaseData(PlaceableError2, typeof(Placeable)).Returns("JsonException");
        yield return new TestCaseData(Resource1, typeof(Resource)).Returns("JsonException");
    }

    [Test]
    [TestCaseSource(nameof(ErrorExamples))]
    [Parallelizable]
    public string TestErrors(string jsonString, Type deserializeType)
    {
        try
        {
            JsonSerializer.Deserialize(jsonString,deserializeType, Options);
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
            new VariableReferenceSerializer(),
        }
    };
}