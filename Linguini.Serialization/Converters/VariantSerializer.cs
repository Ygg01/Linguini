using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides functionality to serialize and deserialize the <see cref="Variant"/> object.
    /// </summary>
    /// <remarks>
    /// This class is a custom JSON converter used to handle the serialization and deserialization
    /// processes for the <see cref="Variant"/> type. It reads and writes JSON representations
    /// of the <see cref="Variant"/> object in a structured format that includes type, key, value,
    /// and whether the variant is the default.
    /// </remarks>
    public class VariantSerializer : JsonConverter<Variant>
    {
        /// <inheritdoc />
        public override Variant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadVariant(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Variant variant, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Variant");
            WriteKey(writer, variant);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, variant.Value, options);
            writer.WriteBoolean("default", variant.IsDefault);
            writer.WriteEndObject();
        }

        private static void WriteKey(Utf8JsonWriter writer, Variant value)
        {
            writer.WritePropertyName("key");
            
            writer.WriteStartObject();
            
            switch (value.Type)
            {
                case VariantType.Identifier:
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("Identifier");
                    writer.WritePropertyName("name");
                    writer.WriteStringValue(value.Key.Span);
                    break;
                case VariantType.NumberLiteral:
                    writer.WritePropertyName("value");
                    writer.WriteStringValue(value.Key.Span);
                    writer.WritePropertyName("type");
                    writer.WriteStringValue("NumberLiteral");
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Unexpected argument `{value.Type}`");
            }


            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads and deserializes a <see cref="Variant"/> object from a JSON element using the specified <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="el">The JSON element containing the serialized data for a <see cref="Variant"/> object.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to control the deserialization process.</param>
        /// <returns>A deserialized <see cref="Variant"/> object.</returns>
        /// <exception cref="JsonException">Thrown when the JSON data is invalid or required properties are missing.</exception>
        public static Variant ReadVariant(JsonElement el, JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("type", out var jsonType)
                && "Variant".Equals(jsonType.GetString()))
            {
                throw new JsonException("Variant must have `type` equal to `Variant`.");
            }

            if (el.TryGetProperty("key", out var jsonKey)
                && TryReadKey(jsonKey, options, out var key))
            {
                if (el.TryGetProperty("value", out var jsonValue)
                    && PatternSerializer.TryReadPattern(jsonValue, options, out var pattern))
                {
                    var isDefault = false;
                    if (el.TryGetProperty("default", out var jsonDefault))
                    {
                        isDefault = jsonDefault.ValueKind == JsonValueKind.True;
                    }

                    return new Variant(key.Value.Item1, key.Value.Item2, pattern, isDefault);
                }
            }

            throw new JsonException("Variant must have `key` and `value`.");
        }

        private static bool TryReadKey(JsonElement jsonKey, JsonSerializerOptions options,
            [NotNullWhen(true)] out (VariantType, ReadOnlyMemory<char>)? key)
        {
            if (IdentifierSerializer.TryGetIdentifier(jsonKey, options, out var id))
            {
                key = (VariantType.Identifier, id.Name);
                return true;
            }

            if (ResourceSerializer.TryReadProcessNumberLiteral(jsonKey, options, out var num))
            {
                key = (VariantType.NumberLiteral, num.Value);
                return true;
            }

            key = null;
            return false;
        }

    }
}