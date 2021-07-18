using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class PlaceableSerializer : JsonConverter<Placeable>
    {
        public override Placeable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

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
    }
}
