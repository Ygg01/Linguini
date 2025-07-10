using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    /// <summary>
    /// A JSON converter for serializing and deserializing instances of the <see cref="Identifier"/> class.
    /// </summary>
    public class IdentifierSerializer : JsonConverter<Identifier>
    {
        /// <inheritdoc/>
        public override Identifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string? id = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();

                    reader.Read();

                    switch (propertyName)
                    {
                        case "type":
                            var typeField = reader.GetString();
                            if (typeField != "Identifier")
                            {
                                throw new JsonException(
                                    $"Invalid type: Expected 'Attribute' found {typeField} instead");
                            }

                            break;
                        case "name":
                            id = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected property: {propertyName}");
                    }
                }
            }

            if (id == null)
            {
                throw new JsonException("No id for Identifier found");
            }

            return new Identifier(id);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Identifier identifier, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Identifier");
            writer.WritePropertyName("name");
            writer.WriteStringValue(identifier.Name.Span);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Attempts to extract an <see cref="Identifier"/> from a JSON element.
        /// </summary>
        /// <param name="el">The JSON element to process.</param>
        /// <param name="options">The JSON serializer options to be applied.</param>
        /// <param name="ident">When the method returns, contains the extracted <see cref="Identifier"/>, if successful.</param>
        /// <returns><c>true</c> if the <see cref="Identifier"/> was successfully extracted; otherwise, <c>false</c>.</returns>
        public static bool TryGetIdentifier(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out Identifier ident)
        {
            if (!el.TryGetProperty("name", out var valueElement) || valueElement.ValueKind != JsonValueKind.String)
            {
                ident = null;
                return false;
            }

            var value = valueElement.GetString() ?? "";
            ident = new Identifier(value);
            return true;
        }
    }
}