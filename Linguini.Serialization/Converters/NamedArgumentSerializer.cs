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
            if (TryReadNamedArguments(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options,
                    out var namedArgument))
            {
                return namedArgument.Value;
            }

            throw new JsonException("Invalid `NamedArgument`!");
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
        /// Attempts to read a <see cref="NamedArgument"/> from the provided <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="el">The JSON element to be parsed.</param>
        /// <param name="options">The serializer options to use during parsing.</param>
        /// <param name="o">
        /// When this method returns, contains the parsed <see cref="NamedArgument"/> if the operation was successful;
        /// otherwise, the value is null. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if the <see cref="NamedArgument"/> was successfully read; otherwise, false.
        /// </returns>
        public static bool TryReadNamedArguments(JsonElement el, JsonSerializerOptions options,
            [NotNullWhen(true)] out NamedArgument? o)
        {
            if (el.TryGetProperty("name", out var namedArg)
                && IdentifierSerializer.TryGetIdentifier(namedArg, options, out var id)
                && el.TryGetProperty("value", out var valueArg)
                && ResourceSerializer.TryReadInlineExpression(valueArg, options, out var inline)
               )
            {
                o = new NamedArgument(id, inline);
                return true;
            }

            throw new JsonException("NamedArgument fields `name` and `value` properties are mandatory");
        }
    }
}