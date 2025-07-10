using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides custom JSON serialization and deserialization logic for the <see cref="MessageReference"/> class.
    /// </summary>
    /// <remarks>
    /// This class is used to convert MessageReference objects to and from JSON representations.
    /// It customizes the serialization behavior to include specific properties and handles
    /// deserialization to ensure expected MessageReference structure.
    /// </remarks>
    public class MessageReferenceSerializer : JsonConverter<MessageReference>
    {
        /// <inheritdoc />
        public override MessageReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ProcessMessageReference(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, MessageReference msgRef, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("MessageReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, msgRef.Id, options);
            if (msgRef.Attribute != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, msgRef.Attribute, options);
            }

            writer.WriteEndObject();
        }

        /// Processes the given JsonElement to create a MessageReference.
        /// <param name="el">The JsonElement representing the serialized MessageReference.</param>
        /// <param name="options">The JsonSerializerOptions used during deserialization.</param>
        /// <returns>A fully constructed MessageReference instance.</returns>
        /// <exception cref="JsonException">Thrown when the required <c>id</c>
        /// field is missing or invalid in the JsonElement.</exception>
        public static MessageReference ProcessMessageReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (el.TryGetProperty("id", out var getProp)
                && IdentifierSerializer.TryGetIdentifier(getProp, options, out var ident))
            {
                Identifier? attr = null;
                if (el.TryGetProperty("attribute", out var prop) && prop.ValueKind != JsonValueKind.Null)
                {
                    IdentifierSerializer.TryGetIdentifier(prop, options, out attr);
                }

                return new MessageReference(ident, attr);
            }

            throw new JsonException("MessageReference requires `id` field");
        }
    }
}