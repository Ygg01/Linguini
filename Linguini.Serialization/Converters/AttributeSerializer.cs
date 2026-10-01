using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;
using Attribute = Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    ///     Provides a custom JSON converter for serializing and deserializing instances of the <see cref="Attribute" /> class.
    /// </summary>
    public class AttributeSerializer : JsonConverter<Attribute>
    {
        /// <inheritdoc />
        public override Attribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var id = new Identifier("");
            var value = new Pattern();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();

                    reader.Read();

                    switch (propertyName)
                    {
                        case "id":
                            id = JsonSerializer.Deserialize<Identifier>(ref reader, options);
                            break;
                        case "value":
                            value = JsonSerializer.Deserialize<Pattern>(ref reader, options);
                            break;
                        case "type":
                            var typeField = reader.GetString();
                            if (typeField != "Attribute")
                            {
                                throw new JsonException(
                                    $"Invalid type: Expected 'Attribute' found {typeField} instead");
                            }

                            break;
                        default:
                            throw new JsonException($"Unexpected property: {propertyName}");
                    }
                }
            }

            return new Attribute(id!, value!);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Attribute attribute, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Attribute");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, attribute.Id, options);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, attribute.Value, options);
            writer.WriteEndObject();
        }

        /// <summary>
        ///     Attempts to parse a JSON element as an <see cref="Attribute" /> object.
        /// </summary>
        /// <param name="bodyEl">
        ///     The JSON element containing the attribute data.
        /// </param>
        /// <param name="options">
        ///     The serializer options to be used during the parsing process.
        /// </param>
        /// <param name="attribute">
        ///     When this method returns, contains the parsed <see cref="Attribute" />
        ///     if parsing was successful; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the JSON element was successfully parsed as an <see cref="Attribute" />;
        ///     otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetAttribute(JsonElement bodyEl, JsonSerializerOptions options,
            [NotNullWhen(true)] out Attribute? attribute)
        {
            if (!bodyEl.TryGetProperty("type", out var termEl) || !"Attribute".Equals(termEl.GetString()))
            {
                attribute = null;
                return false;
            }

            if (!bodyEl.TryGetProperty("id", out var idEl) ||
                !IdentifierSerializer.TryGetIdentifier(idEl, out var ident))
            {
                attribute = null;
                return false;
            }

            if (!bodyEl.TryGetProperty("value", out var valueEl) ||
                !PatternSerializer.TryReadPattern(valueEl, options, out var pattern, out _))
            {
                attribute = null;
                return false;
            }

            attribute = new Attribute(ident, pattern);
            return true;
        }
    }
}