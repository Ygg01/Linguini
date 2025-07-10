using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;
using Attribute = Linguini.Syntax.Ast.Attribute;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides custom JSON serialization and deserialization logic for the <see cref="AstMessage"/> class.
    /// </summary>
    /// <remarks>
    /// This class is used to handle the conversion of <see cref="AstMessage"/> objects to and from their JSON representation.
    /// </remarks>
    public class MessageSerializer : JsonConverter<AstMessage>
    {
        /// <inheritdoc />
        public override AstMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            if (!el.TryGetProperty("id", out var jsonId) ||
                !IdentifierSerializer.TryGetIdentifier(jsonId, options, out var identifier))
            {
                throw new JsonException("AstMessage must have at least `id` element");
            }

            Pattern? value = null;
            AstComment? comment = null;
            var attrs = new List<Attribute>();
            if (el.TryGetProperty("value", out var patternJson) && patternJson.ValueKind == JsonValueKind.Object)
            {
                PatternSerializer.TryReadPattern(patternJson, options, out value);
            }

            if (el.TryGetProperty("comment", out var commentJson) && patternJson.ValueKind == JsonValueKind.Object)
            {
                comment = JsonSerializer.Deserialize<AstComment>(commentJson.GetRawText(), options);
            }

            if (el.TryGetProperty("attributes", out var attrsJson) && attrsJson.ValueKind == JsonValueKind.Array)
            {
                foreach (var attributeJson in attrsJson.EnumerateArray())
                {
                    var attr = JsonSerializer.Deserialize<Attribute>(attributeJson.GetRawText(), options);
                    if (attr != null) attrs.Add(attr);
                }
            }

            return new AstMessage(identifier, value, attrs, AstLocation.Empty, comment);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, AstMessage msg, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteStringValue("Message");


            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, msg.Id, options);

            if (msg.Value != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, msg.Value, options);
            }

            writer.WritePropertyName("attributes");
            writer.WriteStartArray();
            foreach (var attribute in msg.Attributes)
            {
                JsonSerializer.Serialize(writer, attribute, options);
            }

            writer.WriteEndArray();


            if (msg.Comment != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("comment");
                JsonSerializer.Serialize(writer, msg.Comment, options);
            }

            writer.WriteEndObject();
        }
    }
}