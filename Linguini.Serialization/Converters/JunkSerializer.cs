using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    /// <summary>
    /// A JSON converter responsible for serializing and deserializing objects of type <c>Junk</c>.
    /// </summary>
    public class JunkSerializer : JsonConverter<Junk>
    {
        /// <inheritdoc />
        public override Junk Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ProcessJunk(JsonSerializer.Deserialize<JsonElement>(ref reader, options));
        }

        internal static Junk ProcessJunk(JsonElement el)
        {
            if (!el.TryGetProperty("content", out var content))
            {
                throw new JsonException("Junk must have content");
            }

            var str = content.GetString() ?? "";
            return new Junk(str);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Junk value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Junk");
            writer.WritePropertyName("annotations");
            writer.WriteStartArray();
            writer.WriteEndArray();
            writer.WritePropertyName("content");
            writer.WriteStringValue(value.AsStr());
            writer.WriteEndObject();
        }
    }
}