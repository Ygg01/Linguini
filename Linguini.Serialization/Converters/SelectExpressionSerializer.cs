using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides a custom JSON converter for the <see cref="SelectExpression"/> class, enabling serialization
    /// and deserialization of <see cref="SelectExpression"/> instances to and from JSON format.
    /// </summary>
    public class SelectExpressionSerializer : JsonConverter<SelectExpression>
    {
        /// <inheritdoc />
        public override SelectExpression Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return ProcessSelectExpression(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, SelectExpression value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("SelectExpression");
            writer.WritePropertyName("selector");
            ResourceSerializer.WriteInlineExpression(writer, value.Selector, options);
            writer.WritePropertyName("variants");
            writer.WriteStartArray();
            foreach (var variant in value.Variants)
            {
                JsonSerializer.Serialize(writer, variant, options);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        /// <summary>
        /// Processes the JSON element to deserialize it into a <see cref="SelectExpression"/> object.
        /// </summary>
        /// <param name="el">The JSON element containing the SelectExpression to deserialize.</param>
        /// <param name="options">The options used for JSON deserialization.</param>
        /// <returns>An instance of <see cref="SelectExpression"/> deserialized from the provided JSON element.</returns>
        /// <exception cref="JsonException">
        /// Thrown if the required "selector" property is missing or invalid in the JSON,
        /// if no inline expression can be found in the <c>selector</c>,
        /// or if the <c>variants</c> property is missing or is not an array.
        /// </exception>
        public static SelectExpression ProcessSelectExpression(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("selector", out var prop)) throw new JsonException("Select needs a `selector`");
            if (!ResourceSerializer.TryReadInlineExpression(prop, options, out var selector))
            {
                throw new JsonException("No inline expression found!");
            }


            if (el.TryGetProperty("variants", out var variantsProp) && variantsProp.ValueKind != JsonValueKind.Array)
                throw new JsonException("Select `variants` must be a an array");

            var variants = new List<Variant>();
            foreach (var variantEl in variantsProp.EnumerateArray())
            {
                variants.Add(VariantSerializer.ReadVariant(variantEl, options));
            }

            return new SelectExpression(selector, variants);
        }
    }
}