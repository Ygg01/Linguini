using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// A custom JSON converter for serializing and deserializing the <see cref="NamedArgument"/> class.
    /// </summary>
    /// <remarks>
    /// This class is used to handle the conversion of <see cref="NamedArgument"/> objects to and from their JSON representation.
    /// </remarks>
    public class NamedArgumentSerializer : JsonConverter<NamedArgument>
    {
        /// <inheritdoc />
        public override NamedArgument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadNamedArguments(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, NamedArgument value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("NamedArgument");
            writer.WritePropertyName("name");
            JsonSerializer.Serialize(writer, value.Name, options);
            writer.WritePropertyName("value");
            ResourceSerializer.WriteInlineExpression(writer, value.Value, options);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Read a <see cref="NamedArgument"/> from the provided <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="el">The JSON element to be parsed.</param>
        /// <param name="options">The serializer options to use during parsing.</param>
        /// <returns>
        /// Read <see cref="NamedArgument"/>.
        /// </returns>
        public static NamedArgument ReadNamedArguments(JsonElement el, JsonSerializerOptions options)
        {
            if (el.TryGetProperty("name", out var namedArg)
                && IdentifierSerializer.TryGetIdentifier(namedArg, options, out var id)
                && el.TryGetProperty("value", out var valueArg))
            {
                var inline = ResourceSerializer.ReadInlineExpression(valueArg, options);
                return new NamedArgument(id, inline);
            }

            throw new JsonException("NamedArgument fields `name` and `value` properties are mandatory");
        }
    }
}

