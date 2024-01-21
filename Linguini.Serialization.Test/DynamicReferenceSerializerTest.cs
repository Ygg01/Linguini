using System.Text;
using System.Text.Json;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using NUnit.Framework;

namespace Linguini.Serialization.Test;

[TestFixture]
public class DynamicReferenceSerializerTest
{
    [Test]
    [TestOf(typeof(DynamicReferenceSerializer))]
    [Parallelizable]
    public void TestDynamicReference()
    {
        string dynamicReference = @"
{
    ""type"": ""DynamicReference"",
    ""id"": {
        ""type"": ""Identifier"",
        ""name"": ""dyn""
    },
    ""attribute"": {
        ""type"": ""Identifier"",
        ""name"": ""attr""
    },
    ""arguments"": {
        ""type"": ""CallArguments"",
        ""positional"": [
            {
                ""type"": ""MessageReference"",
                ""id"": {
                    ""type"": ""Identifier"",
                    ""name"": ""x""
                },
                ""attribute"": null
            }
         ],
         ""named"": [
            {
                ""type"": ""NamedArgument"",
                ""name"": {
                    ""type"": ""Identifier"",
                    ""name"": ""y""
                },
                ""value"": {
                    ""value"": ""3"",
                    ""type"": ""NumberLiteral""
                }
            }
        ]
    }
}";
        var callArgs = new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3);
        var expected = new  DynamicReference("dyn", "attr", callArgs);
        DynamicReference? actual = JsonSerializer.Deserialize<DynamicReference>(dynamicReference, TestUtil.Options);
        Assert.That(actual, Is.EqualTo(expected));
    }
    
    [Test]
    [TestOf(typeof(DynamicReferenceSerializer))]
    [Parallelizable]
    public void Serde()
    {
        var callArgs = new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3)
            .AddPositionalArg("z")
            .AddNamedArg("om", InlineExpressionBuilder.CreateMessageReference("a", "z"));
        var start = new DynamicReference("dyn", "attr", callArgs);
        var text = "";
        using (var memoryStream = new MemoryStream())
        {
            JsonSerializer.Serialize(memoryStream, start, TestUtil.Options);
            text = Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        DynamicReference deserialized = JsonSerializer.Deserialize<DynamicReference>(text, TestUtil.Options)!;
        Assert.That(deserialized, Is.EqualTo(start));
    }
}