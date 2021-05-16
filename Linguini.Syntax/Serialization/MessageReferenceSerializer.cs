using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class MessageReferenceSerializer : JsonConverter<MessageReference>
    {
        public override MessageReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, MessageReference msgRef, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("MessageReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, msgRef.Id, options);
            if (msgRef.Attribute != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, msgRef.Attribute, options);
            }

            writer.WriteEndObject();
        }
    }
}
