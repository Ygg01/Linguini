using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides JSON serialization and deserialization for the <see cref="VariableReference"/> type.
    /// </summary>
    /// <remarks>
    /// This class is responsible for converting <see cref="VariableReference"/> objects to and from JSON format.
    /// The serialization process ensures that the object is represented with a specific structure containing
    /// predefined property names and values for compatibility with the JSON format.
    /// </remarks>
    public class VariableReferenceSerializer : JsonConverter<VariableReference>
    {
        /// <inheritdoc />
        public override VariableReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ProcessVariableReference(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, VariableReference variableReference,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("VariableReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, variableReference.Id, options);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Processes a JSON element to deserialize it into a <see cref="VariableReference"/> instance.
        /// </summary>
        /// <param name="el">The JSON element containing the data for the variable reference.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> used during the deserialization process.</param>
        /// <returns>A deserialized instance of <see cref="VariableReference"/> based on the provided JSON element.</returns>
        /// <exception cref="JsonException">Thrown when the JSON element does not contain a valid `id` field.</exception>
        public static VariableReference ProcessVariableReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (el.TryGetProperty("id", out var value) &&
                IdentifierSerializer.TryGetIdentifier(value, options, out var ident))
            {
                return new VariableReference(ident);
            }

            throw new JsonException("Variable reference must contain `id` field");
        }
    }
}