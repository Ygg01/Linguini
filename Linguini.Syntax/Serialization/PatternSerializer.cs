using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
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
            StringBuilder? textLiteralBuffer = null;
            foreach (var patternElement in pattern.Elements)
            {
                if (patternElement.TryConvert(out TextLiteral textLiteral))
                {
                    textLiteralBuffer ??= new StringBuilder();
                    textLiteralBuffer.Append(textLiteral.Value);
                }
                else if (patternElement.TryConvert(out Placeable placeable))
                {
                    WriteMergedText(writer, textLiteralBuffer);
                    textLiteralBuffer = null;
                    JsonSerializer.Serialize(writer, placeable, options);
                }
            }
            WriteMergedText(writer, textLiteralBuffer);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static void WriteMergedText(Utf8JsonWriter writer, StringBuilder? textLiteralBuffer)
        {
            if (textLiteralBuffer != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteStringValue("TextElement");
                writer.WritePropertyName("value");
                writer.WriteStringValue(textLiteralBuffer.ToString());
                writer.WriteEndObject();
            }
        }
    }
}
