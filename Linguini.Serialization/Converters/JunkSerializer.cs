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

        /// <summary>
        /// Processes a JSON element to deserialize an object of type <c>Junk</c>.
        /// </summary>
        /// <param name="el">The JSON element to process and convert into a <c>Junk</c> object.</param>
        /// <returns>A <c>Junk</c> instance containing the deserialized data from the JSON element.</returns>
        /// <exception cref="JsonException">
        /// Thrown when the JSON element does not have the required properties.
        /// </exception>
        public static Junk ProcessJunk(JsonElement el)
        {
            if (!el.TryGetProperty("type", out var typeEl) || !"Junk".Equals(typeEl.GetString()))
            {
                throw new JsonException("Junk must have type");
            }
            

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