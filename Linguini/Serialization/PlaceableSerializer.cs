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
            if (value.Expression.TryConvert(out TextLiteral textLiteral))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(textLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("StringLiteral");
                writer.WriteEndObject();
            }
            else if (value.Expression.TryConvert(out NumberLiteral numberLiteral))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(numberLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("NumberLiteral");
                writer.WriteEndObject();
            }
            else if (value.Expression.TryConvert(out SelectExpression selectExpr))
            {
                JsonSerializer.Serialize(writer, selectExpr, options);
            }
            else if (value.Expression.TryConvert(out MessageReference msgRef))
            {
                JsonSerializer.Serialize(writer, msgRef, options);
            }
            else if (value.Expression.TryConvert(out FunctionReference funcRef))
            {
                JsonSerializer.Serialize(writer, funcRef, options);
            }
            else if (value.Expression.TryConvert(out Placeable placeable))
            {
                JsonSerializer.Serialize(writer, placeable, options);
            }
            else if (value.Expression.TryConvert(out TermReference termReference))
            {
                JsonSerializer.Serialize(writer, termReference, options);
            }
            else if (value.Expression.TryConvert(out VariableReference variableReference))
            {
                JsonSerializer.Serialize(writer, variableReference, options);
            }
            writer.WriteEndObject();
        }
    }
}
