using System.Text.Json;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using NUnit.Framework;

namespace Linguini.Serialization.Test;

[TestFixture]
public class CallArgumentsSerializerTest
{
    [Test]
    [TestOf(typeof(CallArgumentsSerializer))]
    [Parallelizable]
    public void TestCallSerializer()
    {
        string callJson = @"
{
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
                ""value"": 3,
                ""type"": ""NumberLiteral""
            }
        }
    ]
}";
        var expected = new CallArgumentsBuilder()
            .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("x"))
            .AddNamedArg("y", 3)
            .Build();
        CallArguments? actual = JsonSerializer.Deserialize<CallArguments>(callJson, TestUtil.Options);
        Assert.That(actual, Is.EqualTo(expected));
    }
}