using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    public class CallArgumentsSerializer : JsonConverter<CallArguments>

    {
        public override CallArguments Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

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

        public static bool TryGetCallArguments(JsonElement el,
            JsonSerializerOptions options,
            [NotNullWhen(true)] out CallArguments? callArguments)
        {
            if (!el.TryGetProperty("positional", out var positional) || !el.TryGetProperty("named", out var named))
            {
                throw new JsonException("CallArguments fields `positional` and `named` properties are mandatory");
            }

            var positionalArgs = new List<IInlineExpression>();
            foreach (var arg in positional.EnumerateArray())
            {
                if (ResourceSerializer.TryReadInlineExpression(arg, options, out var posArgs))
                {
                    positionalArgs.Add(posArgs);
                }
            }

            var namedArgs = new List<NamedArgument>();
            foreach (var arg in named.EnumerateArray())
            {
                if (TryReadNamedArguments(arg, options, out var namedArg))
                {
                    namedArgs.Add(namedArg.Value);
                }
            }

            callArguments = new CallArguments(positionalArgs, namedArgs);
            return true;
        }


        public static bool TryReadNamedArguments(JsonElement el, JsonSerializerOptions options,
            [NotNullWhen(true)] out NamedArgument? o)
        {
            if (el.TryGetProperty("name", out var namedArg)
                && IdentifierSerializer.TryGetIdentifier(namedArg, options, out var id)
                && el.TryGetProperty("value", out var valueArg)
                && ResourceSerializer.TryReadInlineExpression(valueArg, options, out var inline)
               )
            {
                o = new NamedArgument(id, inline);
                return true;
            }

            throw new JsonException("NamedArgument fields `name` and `value` properties are mandatory");
        }
    }
}