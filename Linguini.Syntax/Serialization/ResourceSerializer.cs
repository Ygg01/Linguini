#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Shared.Util;
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
                if (entry.TryConvert(out AstComment? comment))
                {
                    JsonSerializer.Serialize(writer, comment, options);
                }
                else if (entry.TryConvert(out AstMessage? msg))
                {
                    JsonSerializer.Serialize(writer, msg, options);
                }
                else if (entry.TryConvert(out AstTerm? term))
                {
                    JsonSerializer.Serialize(writer, term, options);
                }
                else if (entry.TryConvert(out Junk? junk))
                {
                    JsonSerializer.Serialize(writer, junk, options);
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public static void WriteInlineExpression(Utf8JsonWriter writer, IInlineExpression value,
            JsonSerializerOptions options)
        {
            if (value.TryConvert(out TextLiteral? textLiteral))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(textLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("StringLiteral");
                writer.WriteEndObject();
            }
            else if (value.TryConvert(out NumberLiteral? numberLiteral))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                writer.WriteStringValue(numberLiteral.Value.Span);
                writer.WritePropertyName("type");
                writer.WriteStringValue("NumberLiteral");
                writer.WriteEndObject();
            }
            else if (value.TryConvert(out MessageReference? msgRef))
            {
                JsonSerializer.Serialize(writer, msgRef, options);
            }
            else if (value.TryConvert(out FunctionReference? funcRef))
            {
                JsonSerializer.Serialize(writer, funcRef, options);
            }
            else if (value.TryConvert(out Placeable? placeable))
            {
                JsonSerializer.Serialize(writer, placeable, options);
            }
            else if (value.TryConvert(out TermReference? termReference))
            {
                JsonSerializer.Serialize(writer, termReference, options);
            }
            else if (value.TryConvert(out VariableReference? variableReference))
            {
                JsonSerializer.Serialize(writer, variableReference, options);
            }
        }
    }
}
