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
        ""Body"": [
            {
                ""type"": ""Testing"",
                ""content"": ""Test1""
            }
        ]
    }";
    
    private static IEnumerable<TestCaseData> ErrorExamples()
    {
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