using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides a JSON converter for the <see cref="DynamicReference"/> type.
    /// This serializer is designed to handle the serialization and deserialization
    /// of <see cref="DynamicReference"/> objects, ensuring correct processing
    /// of their properties and structure.
    /// </summary>
    public class DynamicReferenceSerializer : JsonConverter<DynamicReference>
    {
        /// <inheritdoc />
        public override DynamicReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return ProcessDynamicReference(el, options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DynamicReference dynRef, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("DynamicReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, dynRef.Id, options);

            if (dynRef.Attribute != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, dynRef.Attribute, options);
            }

            if (dynRef.Arguments != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, dynRef.Arguments, options);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Processes a <see cref="JsonElement" /> to create a <see cref="DynamicReference" /> instance.
        /// </summary>
        /// <param name="el">The JSON element containing the data to be deserialized.</param>
        /// <param name="options">The serializer options to use during processing.</param>
        /// <returns>A <see cref="DynamicReference" /> instance created from the provided JSON data.</returns>
        /// <exception cref="JsonException">
        /// Thrown when the required <c>id</c> field is missing or when other invalid conditions occur during processing.
        /// </exception>
        public static DynamicReference ProcessDynamicReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("id", out var jsonId) ||
                !IdentifierSerializer.TryGetIdentifier(jsonId, options, out var identifier))
            {
                throw new JsonException("Dynamic reference must contain at least `id` field");
            }

            Identifier? attribute = null;
            CallArguments? arguments = null;
            if (el.TryGetProperty("attribute", out var jsonAttribute))
            {
                IdentifierSerializer.TryGetIdentifier(jsonAttribute, options, out attribute);
            }

            if (el.TryGetProperty("arguments", out var jsonArgs))
            {
                CallArgumentsSerializer.TryGetCallArguments(jsonArgs, options, out arguments);
            }

            return new DynamicReference(identifier, attribute, arguments);
        }
    }
}