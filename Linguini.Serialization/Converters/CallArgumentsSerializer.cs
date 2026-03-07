using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    /// <summary>
    /// Provides JSON serialization and deserialization functionality for the <see cref="CallArguments"/> type.
    /// </summary>
    /// <remarks>
    /// The <see cref="CallArgumentsSerializer"/> is responsible for reading and writing
    /// the JSON representation of <see cref="CallArguments"/>, ensuring proper handling
    /// of its positional and named arguments.
    /// </remarks>
    public class CallArgumentsSerializer : JsonConverter<CallArguments>
    {
        /// <inheritdoc />
        public override CallArguments Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return ReadCallArguments(el, options);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, CallArguments value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("CallArguments");
            writer.WritePropertyName("positional");
            writer.WriteStartArray();
            foreach (var positionalArg in value.PositionalArgs)
            {
                ResourceSerializer.WriteInlineExpression(writer, positionalArg, options);
            }

            writer.WriteEndArray();
            writer.WritePropertyName("named");
            writer.WriteStartArray();
            foreach (var namedArg in value.NamedArgs)
            {
                JsonSerializer.Serialize(writer, namedArg, options);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        /// <summary>
        /// Extract and deserialize call arguments from a given JSON element.
        /// </summary>
        /// <param name="el">The JSON element containing call argument data.</param>
        /// <param name="options">The JSON serializer options used during the deserialization process.</param>
        /// <returns>
        /// Extracted call arguments.
        /// </returns>
        /// <exception cref="JsonException">
        /// Thrown when required fields `positional` or `named` are missing in the JSON element.
        /// </exception>
        public static CallArguments ReadCallArguments(JsonElement el, JsonSerializerOptions options)
        {
            if (!el.TryGetProperty("positional", out var positional) || !el.TryGetProperty("named", out var named))
            {
                throw new JsonException("CallArguments fields `positional` and `named` properties are mandatory");
            }

            var positionalArgs = new List<IInlineExpression>();
            foreach (var arg in positional.EnumerateArray())
            {
                var posArgs = ResourceSerializer.ReadInlineExpression(arg, options);
                positionalArgs.Add(posArgs);
            }

            var namedArgs = new List<NamedArgument>();
            foreach (var arg in named.EnumerateArray())
            {
                var namedArg = NamedArgumentSerializer.ReadNamedArguments(arg, options);
                namedArgs.Add(namedArg);
            }

            return new CallArguments(positionalArgs, namedArgs);
        }
    }
}

