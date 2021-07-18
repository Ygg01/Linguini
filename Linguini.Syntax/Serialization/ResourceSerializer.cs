#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class ResourceSerializer : JsonConverter<Resource>
    {
        public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Resource value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Resource");
            writer.WritePropertyName("body");
            writer.WriteStartArray();
            foreach (var entry in value.Entries)
            {
                switch (entry)
                {
                    case AstComment astComment:
                        JsonSerializer.Serialize(writer, astComment, options);
                        break;
                    case AstMessage astMessage:
                        JsonSerializer.Serialize(writer, astMessage, options);
                        break;
                    case AstTerm astTerm:
                        JsonSerializer.Serialize(writer, astTerm, options);
                        break;
                    case Junk junk:
                        JsonSerializer.Serialize(writer, junk, options);
                        break;
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void WriteInlineExpression(Utf8JsonWriter writer, IInlineExpression value,
            JsonSerializerOptions options)
        {
            if (value is TextLiteral textLiteral)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(textLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("StringLiteral");
                writer.WriteEndObject();
            }
            else if (value is NumberLiteral numberLiteral)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(numberLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("NumberLiteral");
                writer.WriteEndObject();
            }
            else if (value is MessageReference msgRef)
            {
                JsonSerializer.Serialize(writer, msgRef, options);
            }
            else if (value is FunctionReference funcRef)
            {
                JsonSerializer.Serialize(writer, funcRef, options);
            }
            else if (value is Placeable placeable)
            {
                JsonSerializer.Serialize(writer, placeable, options);
            }
            else if (value is TermReference termReference)
            {
                JsonSerializer.Serialize(writer, termReference, options);
            }
            else if (value is VariableReference variableReference)
            {
                JsonSerializer.Serialize(writer, variableReference, options);
            }
        }
    }
}
