#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Ast;

namespace Linguini.Serialization
{
    public class ResourceSerializer : JsonConverter<Resource>
    {
        public override Resource? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Resource value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Resource");
            writer.WritePropertyName("body");
            writer.WriteStartArray();
            foreach (var entry in value.Body)
            {
                if (entry.TryConvert(out Comment comment))
                {
                    JsonSerializer.Serialize(writer, comment, options);
                }
                else if (entry.TryConvert(out Message msg))
                {
                    JsonSerializer.Serialize(writer, msg, options);
                }
                else if (entry.TryConvert(out Term term))
                {
                    JsonSerializer.Serialize(writer, term, options);
                }
                else if (entry.TryConvert(out Junk junk))
                {
                    JsonSerializer.Serialize(writer, junk, options);
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void WriteIdentifier(Utf8JsonWriter writer, Identifier id)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Identifier");
            writer.WritePropertyName("name");
            writer.WriteStringValue(id.Name.Span);
            writer.WriteEndObject();
        }
    }
}
