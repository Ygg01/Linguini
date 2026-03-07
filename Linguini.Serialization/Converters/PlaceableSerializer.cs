using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides a custom JSON converter for serializing and deserializing <see cref="Placeable"/> objects.
    /// </summary>
    public class PlaceableSerializer : JsonConverter<Placeable>
    {
        /// <inheritdoc />
        public override Placeable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Placeable value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Placeable");
            writer.WritePropertyName("expression");

            switch (value.Expression)
            {
                case IInlineExpression inlineExpression:
                    ResourceSerializer.WriteInlineExpression(writer, inlineExpression, options);
                    break;
                case SelectExpression selectExpression:
                    JsonSerializer.Serialize(writer, selectExpression, options);
                    break;
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Processes a JSON element to create a <see cref="Placeable"/> instance.
        /// </summary>
        /// <param name="el">The JSON element representing the placeable structure.</param>
        /// <param name="options">Options for JSON deserialization.</param>
        /// <returns>The deserialized <see cref="Placeable"/> instance.</returns>
        /// <exception cref="JsonException">Thrown if the JSON element cannot be processed into a valid <see cref="Placeable"/>.</exception>
        public static Placeable ProcessPlaceable(JsonElement el, JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("expression", out var expr))
            {
                throw new JsonException("Placeable must have `expression` value.");
            }

            return new Placeable(ResourceSerializer.ReadExpression(expr, options));
        }
    }
}
