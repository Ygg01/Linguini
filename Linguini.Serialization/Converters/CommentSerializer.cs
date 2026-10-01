using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    ///     Provides a custom JSON converter for the <see cref="AstComment" /> type.
    /// </summary>
    /// <remarks>
    ///     This class is used to serialize and deserialize comments within the Fluent resource syntax
    ///     represented by <see cref="AstComment" />. It distinguishes between different
    ///     comment levels such as Comment, GroupComment, and ResourceComment.
    /// </remarks>
    public class CommentSerializer : JsonConverter<AstComment>
    {
        /// <inheritdoc />
        public override AstComment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var commentLevel = CommentLevel.None;
            var content = new List<ReadOnlyMemory<char>>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();

                    reader.Read();

                    switch (propertyName)
                    {
                        case "type":
                            var type = reader.GetString();
                            commentLevel = type switch
                            {
                                "Comment"         => CommentLevel.Comment,
                                "GroupComment"    => CommentLevel.GroupComment,
                                "ResourceComment" => CommentLevel.ResourceComment,
                                _                 => CommentLevel.None
                            };
                            break;
                        case "content":
                            var s = reader.GetString();
                            content = s != null
                                ? s.Split().Select(x => x.AsMemory()).ToList()
                                // ReSharper disable once ArrangeObjectCreationWhenTypeNotEvident
                                : new();
                            break;
                        default:
                            throw new JsonException($"Unexpected property: {propertyName}");
                    }
                }
            }

            if (commentLevel == CommentLevel.None)
            {
                throw new JsonException("Comment must have some level of nesting");
            }

            return new AstComment(commentLevel, content);
        }

        /// <inheritdoc />
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
            }

            writer.WritePropertyName("content");
            writer.WriteStringValue(comment.AsStr());
            writer.WriteEndObject();
        }

        /// <summary>
        ///     Attempts to read a JSON element as an <see cref="AstComment" /> object.
        /// </summary>
        /// <param name="el">The JSON element to parse as a comment.</param>
        /// <param name="ident">
        ///     When the method returns <c>true</c>, contains the parsed <see cref="AstComment" /> object.
        ///     When the method returns <c>false</c>, contains <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the JSON element was successfully read as an <see cref="AstComment" /> object;
        ///     otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="JsonException">Thrown when the JSON element is invalid or is missing required properties.</exception>
        public static bool TryReadComment(JsonElement el, [NotNullWhen(true)] out AstComment? ident)
        {
            if (!el.TryGetProperty("type", out var typeStr) || typeStr.ValueKind != JsonValueKind.String ||
                typeStr.GetString() is null || (typeStr.GetString() != "Comment" &&
                                                typeStr.GetString() != "GroupComment" &&
                                                typeStr.GetString() != "ResourceComment"))
            {
                ident = null;
                return false;
            }

            var commentLevel = typeStr.GetString() switch
            {
                "Comment"         => CommentLevel.Comment,
                "GroupComment"    => CommentLevel.GroupComment,
                "ResourceComment" => CommentLevel.ResourceComment,
                _                 => CommentLevel.None
            };
            if (el.TryGetProperty("content", out var contentStr))
            {
                var content = contentStr.GetString() ?? "";
                ident = new AstComment(commentLevel, new List<ReadOnlyMemory<char>>
                {
                    content.AsMemory()
                });
                return true;
            }

            ident = null;
            return false;
        }
    }
}