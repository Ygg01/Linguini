using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides custom serialization and deserialization logic for the
    /// <see cref="FunctionReference"/> class when using System.Text.Json.
    /// </summary>
    public class FunctionReferenceSerializer : JsonConverter<FunctionReference>
    {
        /// <inheritdoc />
        public override FunctionReference Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return ProcessFunctionReference(el, options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, FunctionReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("FunctionReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, value.Id, options);
            writer.WritePropertyName("arguments");
            JsonSerializer.Serialize(writer, value.Arguments, options);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Processes a JSON element and deserializes it into a <see cref="FunctionReference"/> object.
        /// </summary>
        /// <param name="el">The JSON element representing the function reference.</param>
        /// <param name="options">The options to use when deserializing the JSON element.</param>
        /// <returns>A deserialized <see cref="FunctionReference"/> object constructed from the provided JSON element.</returns>
        /// <exception cref="JsonException">
        /// Thrown when the JSON element does not contain the required <c>id</c> or <c>arguments</c> properties,
        /// or when the properties cannot be properly parsed into a valid function reference.
        /// </exception>
        public static FunctionReference ProcessFunctionReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("id", out JsonElement value) ||
                !IdentifierSerializer.TryGetIdentifier(value, options, out var ident))
            {
                throw new JsonException("Function reference must contain `id` field");
            }

            CallArguments? arguments;

            if (!el.TryGetProperty("arguments", out var jsonArguments) ||
                !CallArgumentsSerializer.TryGetCallArguments(jsonArguments, options, out arguments)
               )
            {
                throw new JsonException("Function reference must contain `arguments` field");
            }

            return new FunctionReference(ident, arguments.Value);
        }
    }
}