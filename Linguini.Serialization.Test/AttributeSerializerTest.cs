﻿using System.Text.Json;
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
        Attribute expected = new Attribute("desc", new PatternBuilder("description"));
        string attributeJson = @"
{
    ""type"": ""Attribute"",
    ""id"": {
        ""type"": ""Identifier"",
        ""value"": ""desc""
    },
    ""value"": {
        ""type"": ""Pattern"",
        ""elements"": [
            {
                ""type"": ""TextLiteral"",
                ""value"": ""description""
            }
        ]
    }
}";
        Attribute? actual = JsonSerializer.Deserialize<Attribute>(attributeJson, TestUtil.Options);
        Assert.That(actual, Is.EqualTo(expected));
    }
}