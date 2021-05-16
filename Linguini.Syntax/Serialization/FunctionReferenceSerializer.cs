using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class FunctionReferenceSerializer : JsonConverter<FunctionReference>
    {
        public override FunctionReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, FunctionReference value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("FunctionReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, value.Id, options);
            writer.WritePropertyName("arguments");
            JsonSerializer.Serialize(writer, value.Arguments, options);
            writer.WriteEndObject();
        }
    }
}
