using System.Text;
using System.Text.Json;
using Linguini.Serialization.Converters;
using Linguini.Syntax.Ast;
using NUnit.Framework;
using Attribute = Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Test;

[TestFixture]
public class AttributeSerializerTest
{
    [Test]
    [TestOf(typeof(AttributeSerializer))]
    [Parallelizable]
    public void TestAttributeSerializer()
    {
        string attributeJson = @"
{
    ""type"": ""Attribute"",
    ""id"": {
        ""type"": ""Identifier"",
        ""name"": ""desc""
    },
    ""value"": {
        ""type"": ""Pattern"",
        ""elements"": [
            {
                ""type"": ""StringLiteral"",
                ""value"": ""description""
            }
        ]
    }
}";
        Attribute expected = new Attribute("desc", new PatternBuilder("description"));
        Attribute? actual = JsonSerializer.Deserialize<Attribute>(attributeJson, TestUtil.Options);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    [TestOf(typeof(AttributeSerializer))]
    [Parallelizable]
    public void Serde()
    {
        Attribute start = new Attribute("desc", new PatternBuilder("d1"));
        var text = "";
        using (var memoryStream = new MemoryStream())
        {
            JsonSerializer.Serialize(memoryStream, start, TestUtil.Options);
            text = Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        Attribute deserialized = JsonSerializer.Deserialize<Attribute>(text, TestUtil.Options)!;
        Assert.That(deserialized, Is.EqualTo(start));
    }
}