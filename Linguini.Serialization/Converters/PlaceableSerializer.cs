using System;
using System.Diagnostics.CodeAnalysis;
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
        /// Attempts to process a JSON element into a <see cref="Placeable"/> object.
        /// </summary>
        /// <param name="el">The JSON element to process.</param>
        /// <param name="options">The JSON serializer options to use during processing.</param>
        /// <param name="placeable">
        /// When this method returns, contains the <see cref="Placeable"/> instance
        /// if the operation was successful, or <c>null</c> if unsuccessful.
        /// </param>
        /// <returns>
        /// <c>true</c> if the processing was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="JsonException">
        /// Thrown when the required "expression" property is missing from the JSON element.
        /// </exception>
        public static bool TryProcessPlaceable(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out Placeable placeable)
        {
            if (!el.TryGetProperty("expression", out var expr))
            {
                throw new JsonException("Placeable must have `expression` value.");
            }

            placeable = new Placeable(ResourceSerializer.ReadExpression(expr, options));
            return true;
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
            if (!TryProcessPlaceable(el, options, out var placeable)) throw new JsonException("Expected placeable!");

            return placeable;
        }
    }
}