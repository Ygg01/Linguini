using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// Provides custom JSON serialization and deserialization for the <c>AstTerm</c> class.
    /// This class is a JSON converter that handles converting <c>AstTerm</c> instances
    /// to and from JSON format during serialization and deserialization processes.
    /// </summary>
    public class TermSerializer : JsonConverter<AstTerm>
    {

        /// <summary>
        /// Read and convert the JSON to AstTerm. NOT IMPLEMENTED!
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override AstTerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, AstTerm term, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue("Term");
            writer.WritePropertyName("id");
            JsonSerializer.Serialize(writer, term.Id, options);
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, term.Value, options);

            writer.WritePropertyName("attributes");
            writer.WriteStartArray();
            foreach (var attribute in term.Attributes)
            {
                JsonSerializer.Serialize(writer, attribute, options);
            }

            writer.WriteEndArray();


            if (term.Comment != null || options.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
            {
                writer.WritePropertyName("comment");
                JsonSerializer.Serialize(writer, term.Comment, options);
            }

            writer.WriteEndObject();
        }
    }
}
