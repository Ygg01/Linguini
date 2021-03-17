using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class NamedArgumentSerializer: JsonConverter<NamedArgument>

    {
        public override NamedArgument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, NamedArgument value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("NamedArgument");
            writer.WritePropertyName("name");
            JsonSerializer.Serialize(writer, value.Name, options);
            writer.WritePropertyName("value");
            ResourceSerializer.WriteInlineExpression(writer, value.Value, options);
            writer.WriteEndObject();
        }
    }
}
