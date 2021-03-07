using System;
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

        public override void Write(Utf8JsonWriter writer, Comment value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue(value.CommentLevel.ToString());
            writer.WritePropertyName("content");
            writer.WriteStringValue(value.ContentStr());
            writer.WriteEndObject();
        }
    }
}
