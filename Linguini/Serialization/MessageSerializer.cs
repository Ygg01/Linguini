using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Ast;

namespace Linguini.Serialization
{
    public class MessageSerializer : JsonConverter<Message>
    {
        public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Message msg, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteStringValue("Message");

            
            writer.WritePropertyName("id");
            ResourceSerializer.WriteIdentifier(writer, msg.Id);

            if (msg.Value != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, msg.Value, options);                
            }


            writer.WritePropertyName("attributes");
            writer.WriteStartArray();
            foreach (var attribute in msg.Attributes)
            {
                JsonSerializer.Serialize(writer, attribute, options);
            }

            writer.WriteEndArray();


            if (msg.Comment != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("comment");
                JsonSerializer.Serialize(writer, msg.Comment, options);
            }

            writer.WriteEndObject();
        }


    }
}
