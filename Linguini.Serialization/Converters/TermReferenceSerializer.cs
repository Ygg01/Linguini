﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides JSON serialization and deserialization support for the <see cref="TermReference"/> type.
    /// </summary>
    /// <remarks>
    /// This class is used to convert instances of <see cref="TermReference"/> to and from their JSON representation.
    /// It handles the processing of the JSON object structure corresponding to a <see cref="TermReference"/>,
    /// including its attributes and arguments, during serialization and deserialization.
    /// </remarks>
    public class TermReferenceSerializer : JsonConverter<TermReference>
    {
        /// <inheritdoc />
        public override TermReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ProcessTermReference(JsonSerializer.Deserialize<JsonElement>(ref reader, options), options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TermReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("TermReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, value.Id, options);

            if (value.Attribute != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("attribute");
                JsonSerializer.Serialize(writer, value.Attribute, options);
            }

            if (value.Arguments != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("arguments");
                JsonSerializer.Serialize(writer, value.Arguments, options);
            }

            writer.WriteEndObject();
        }


        /// <summary>
        /// Processes a JSON element and deserializes it into a <see cref="TermReference"/> object.
        /// </summary>
        /// <param name="el">The JSON element to process, containing the serialized <see cref="TermReference"/>.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use during deserialization.</param>
        /// <returns>A deserialized <see cref="TermReference"/> object constructed from the provided JSON element.</returns>
        /// <exception cref="JsonException">Thrown if the JSON element does not contain the required "id" property or if deserialization fails.</exception>
        public static TermReference ProcessTermReference(JsonElement el,
            JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("id", out JsonElement value) ||
                !IdentifierSerializer.TryGetIdentifier(value, options, out var id))
            {
                throw new JsonException("Term reference must contain at least `id` field");
            }

            Identifier? attribute = null;
            CallArguments? arguments = null;
            if (el.TryGetProperty("attribute", out var attr))
            {
                IdentifierSerializer.TryGetIdentifier(attr, options, out attribute);
            }

            if (el.TryGetProperty("arguments", out var callarg))
            {
                CallArgumentsSerializer.TryGetCallArguments(callarg, options, out arguments);
            }

            return new TermReference(id, attribute, arguments);
        }
    }
}