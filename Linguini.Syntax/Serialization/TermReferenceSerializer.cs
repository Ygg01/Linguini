using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class TermReferenceSerializer : JsonConverter<TermReference>
    {
        public override TermReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TermReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("TermReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, value.Id, options);
            
            if (value.Attribute != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, value.Attribute, options);
            }
            
            if (value.Arguments != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, value.Arguments, options);
            }
            
            writer.WriteEndObject();
        }
    }
}
