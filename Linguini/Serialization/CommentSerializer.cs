using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Ast;

namespace Linguini.Serialization
{
    public class CommentSerializer : JsonConverter<Comment>
    {
        public override Comment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Comment comment, JsonSerializerOptions options)
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
