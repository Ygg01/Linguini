using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Syntax.Serialization
{
    public class CommentSerializer : JsonConverter<AstComment>
    {
        public override AstComment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AstComment comment, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            switch (comment.CommentLevel)
            {
                case CommentLevel.Comment:
                    writer.WriteStringValue("Comment");
                    break;
                case CommentLevel.GroupComment:
                    writer.WriteStringValue("GroupComment");
                    break;
                case CommentLevel.ResourceComment:
                    writer.WriteStringValue("ResourceComment");
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Unexpected comment `{comment.CommentLevel}`");
            }
            writer.WritePropertyName("content");
            writer.WriteStringValue(comment.AsStr());
            writer.WriteEndObject();
        }
    }
}
