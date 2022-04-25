#if NET5_0_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class CallArgumentsSerializer: JsonConverter<CallArguments>

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
    }
}
#endif