using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;
using Attribute = Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides a custom JSON converter for serializing and deserializing instances of the <see cref="Attribute"/> class.
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
                    string? propertyName = reader.GetString();

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
    }
}
