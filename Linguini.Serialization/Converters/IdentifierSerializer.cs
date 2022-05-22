using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters

{
    public class IdentifierSerializer : JsonConverter<Identifier>
    {
        public override Identifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Identifier identifier, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Identifier");
            writer.WritePropertyName("name");
            writer.WriteStringValue(identifier.Name.Span);
            writer.WriteEndObject();
        }
    }
}