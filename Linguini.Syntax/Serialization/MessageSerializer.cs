using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class MessageSerializer : JsonConverter<AstMessage>
    {
        public override AstMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AstMessage msg, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteStringValue("Message");


            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, msg.Id, options);

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
