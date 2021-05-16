using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class SelectExpressionSerializer : JsonConverter<SelectExpression>
    {
        public override SelectExpression Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, SelectExpression value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("SelectExpression");
            writer.WritePropertyName("selector");
            ResourceSerializer.WriteInlineExpression(writer, value.Selector, options);
            writer.WritePropertyName("variants");
            writer.WriteStartArray();
            foreach (var variant in value.Variants)
            {
                JsonSerializer.Serialize(writer, variant, options);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
