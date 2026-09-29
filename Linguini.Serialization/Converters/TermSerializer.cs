using System;
using System.Diagnostics.CodeAnalysis;
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
        /// Reads and deserializes the JSON data into an <c>AstTerm</c> instance.
        /// </summary>
        /// <param name="reader">The <c>Utf8JsonReader</c> used to read the JSON data.</param>
        /// <param name="typeToConvert">The type of object to convert, expected to be <c>AstTerm</c>.</param>
        /// <param name="options">The <c>JsonSerializerOptions</c> to assist in deserialization.</param>
        /// <returns>The deserialized <c>AstTerm</c> object.</returns>
        /// <exception cref="JsonException">Thrown if the JSON does not contain a valid <c>AstTerm</c> representation.</exception>
        public override AstTerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var deserialize = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return TryGetAstTerm(deserialize, options, out var term) 
                ? term 
                : throw new JsonException("Expected to parse the term");
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

        /// <summary>
        /// Attempts to parse and convert a <c>JsonElement</c> instance into an <c>AstTerm</c> object.
        /// </summary>
        /// <param name="bodyArrayEl">The <c>JsonElement</c> representing the input data to be converted.</param>
        /// <param name="options">The JSON serialization options used for deserialization.</param>
        /// <param name="ast">
        /// When this method returns, contains the resulting <c>AstTerm</c> object if the conversion
        /// was successful, or <c>null</c> if it failed.
        /// </param>
        /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="JsonException">If error encountered.</exception>
        public static bool TryGetAstTerm(JsonElement bodyArrayEl, JsonSerializerOptions options,
            [NotNullWhen(true)] out AstTerm? ast)
        {
            if (!bodyArrayEl.TryGetProperty("type", out var termEl) || !"Term".Equals(termEl.GetString()))
            {
                ast = null;
                return false;
            }

            if (!bodyArrayEl.TryGetProperty("id", out var idEl) ||
                !IdentifierSerializer.TryGetIdentifier(idEl, out var id))
            {
                ast = null;
                return false;
            }

            var term = AstTermBuilder.Builder(id);


            if (!bodyArrayEl.TryGetProperty("value", out var valueEl) ||
                !PatternSerializer.TryReadPattern(valueEl, options, out var pattern, out _))
            {
                ast = null;
                return false;
            }

            term.SetPattern(pattern);
            
            // Attributes are optional but have to be properly formatted.
            if (bodyArrayEl.TryGetProperty("attributes", out var arrayEl) && arrayEl.ValueKind != JsonValueKind.Array)
            {
                ast = null;
                return false;
            }

            foreach (var attrEl in arrayEl.EnumerateArray())
            {
                if (!AttributeSerializer.TryGetAttribute(attrEl, options, out var attribute))
                {
                    ast = null;
                    return false;
                }

                term.AddAttribute(attribute);
            }
            

            ast = term.Build();
            return true;
        }
    }
}