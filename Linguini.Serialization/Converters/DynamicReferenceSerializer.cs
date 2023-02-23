using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    public class DynamicReferenceSerializer : JsonConverter<DynamicReference>
    {
        public override DynamicReference? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, DynamicReference dynRef, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("DynamicReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, dynRef.Id, options);
            
            if (dynRef.Attribute != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, dynRef.Attribute, options);
            }
            
            if (dynRef.Arguments != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, dynRef.Arguments, options);
            }
            
            writer.WriteEndObject();
        }
    }
}