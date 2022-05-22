using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Attribute = Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Converters
{
    public class AttributeSerializer : JsonConverter<Attribute>
    {
        public override Attribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Attribute attribute, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Attribute");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, attribute.Id, options);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, attribute.Value, options);
            writer.WriteEndObject();
        }
    }
}
