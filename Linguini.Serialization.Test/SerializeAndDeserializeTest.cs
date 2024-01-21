using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using NUnit.Framework;
using Attribute = Linguini.Syntax.Ast.Attribute;


namespace Linguini.Serialization.Test;

[TestFixture]
public class SerializeAndDeserializeTest
{
    [Test]
    [TestCaseSource(nameof(AstExamples))]
    [Parallelizable]
    public void SerializeDeserializeTest(object x)
    {
        SerializeAndDeserializeTest.SerializeDeserializeTest(x);
    }

    public static IEnumerable<object> AstExamples()
    {
        yield return new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3)
            .Build();
        yield return new Attribute("desc", new PatternBuilder("description"));
        yield return new DynamicReference("dyn", "attr", new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3));
        yield return new FunctionReference("foo", new CallArgumentsBuilder()
            .AddPositionalArg(3)
            .AddNamedArg("test", InlineExpressionBuilder.CreateTermReference("x", "y"))
            .Build()
        );
        yield return new Identifier("test");
    }

    private static void SerializeDeserializeTest<T>(T expected)
    {
        // Serialize the object to JSON string.
        var jsonString = JsonSerializer.Serialize(expected, Options);

        // Deserialize the JSON string back into an object.
        Debug.Assert(expected != null, nameof(expected) + " != null");
        var deserializedObject = JsonSerializer.Deserialize(jsonString, expected.GetType(), Options);

        // Now you have a 'deserializedObject' which should be equivalent to the original 'expected' object.
        Assert.That(deserializedObject, Is.Not.Null);
        Assert.That(deserializedObject, Is.EqualTo(expected));
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