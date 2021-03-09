using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Ast;

namespace Linguini.Serialization
{
    public class PatternSerializer : JsonConverter<Pattern>

    {
        public override Pattern? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Pattern pattern, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Pattern");
            writer.WritePropertyName("elements");
            writer.WriteStartArray();
            foreach (var patternElement in pattern.Elements)
            {
                if (patternElement.TryConvert(out TextLiteral textLiteral))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("TextElement");
                    writer.WritePropertyName("value");
                    writer.WriteStringValue(textLiteral.Value.Span);
                    writer.WriteEndObject();
                }
                else if (patternElement.TryConvert(out Placeable placeable))
                {
                    JsonSerializer.Serialize(writer, placeable, options);
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
