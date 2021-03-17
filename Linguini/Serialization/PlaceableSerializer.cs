using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Ast;

namespace Linguini.Serialization
{
    public class PlaceableSerializer : JsonConverter<Placeable>
    {
        public override Placeable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Placeable value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Placeable");
            writer.WritePropertyName("expression");
            if (value.Expression.TryConvert(out IInlineExpression expr))
            {
                ResourceSerializer.WriteInlineExpression(writer, expr, options);
            }
            else if (value.Expression.TryConvert(out SelectExpression selectExpr))
            {
                JsonSerializer.Serialize(writer, selectExpr, options);
            }

            writer.WriteEndObject();
        }
    }
}
