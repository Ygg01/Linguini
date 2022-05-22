using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class VariantSerializer : JsonConverter<Variant>
    {
        public override Variant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Variant variant, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Variant");
            WriteKey(writer, variant);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, variant.Value, options);
            writer.WriteBoolean("default", variant.IsDefault);
            writer.WriteEndObject();
        }

        private static void WriteKey(Utf8JsonWriter writer, Variant value)
        {
            writer.WritePropertyName("key");
            
            writer.WriteStartObject();
            
            switch (value.Type)
            {
                case VariantType.Identifier:
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("Identifier");
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(value.Key.Span);
                    break;
                case VariantType.NumberLiteral:
                    writer.WritePropertyName("value");
                    writer.WriteStringValue(value.Key.Span);
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("NumberLiteral");
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Unexpected argument `{value.Type}`");
            }


            writer.WriteEndObject();
        }
    }
}