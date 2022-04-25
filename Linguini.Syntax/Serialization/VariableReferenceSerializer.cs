#if NET5_0_OR_GREATER

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class VariableReferenceSerializer : JsonConverter<VariableReference>
    {
        public override VariableReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, VariableReference variableReference, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("VariableReference");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, variableReference.Id, options);
            writer.WriteEndObject();
        }
    }
}
#endif