using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Ast;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// A serializer for the <see cref="Resource"/> class, used for handling JSON serialization and deserialization
    /// of Fluent resource objects. It extends <see cref="System.Text.Json.Serialization.JsonConverter{T}"/>.
    /// </summary>
    /// <remarks>
    /// This class provides implementations for reading and writing JSON representations of Fluent resources.
    /// It includes utilities for handling expressions, literals, and inline elements during serialization.
    /// </remarks>
    public class ResourceSerializer : JsonConverter<Resource>
    {

        /// <summary>
        /// Read and convert the JSON to ResourceSerializer. NOT IMPLEMENTED!
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Writes an inline expression to a JSON writer.
        /// </summary>
        /// <param name="writer">The Utf8JsonWriter to which the inline expression will be written.</param>
        /// <param name="value">The inline expression to write, implementing <see cref="IInlineExpression"/>.</param>
        /// <param name="options">The JSON serializer options to use during serialization.</param>
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
            else if (value is DynamicReference dynamicReference)
            {
                JsonSerializer.Serialize(writer, dynamicReference, options);
            }
        }

        /// <summary>
        /// Processes a JSON element to create a <see cref="TextLiteral"/> object.
        /// </summary>
        /// <param name="el">
        /// The JSON element containing the "value" property to be deserialized into a <see cref="TextLiteral"/>.
        /// </param>
        /// <param name="options">
        /// The JSON serializer options to use during processing.
        /// </param>
        /// <returns>
        /// A <see cref="TextLiteral"/> instance populated with the value extracted from the JSON element.
        /// </returns>
        public static TextLiteral ProcessTextLiteral(JsonElement el, JsonSerializerOptions options)
        {
            return new(el.GetProperty("value").GetString() ?? "");
        }

        /// <summary>
        /// Processes a JSON element to extract a <see cref="NumberLiteral"/> object.
        /// </summary>
        /// <param name="el">The JSON element containing the number literal to process.</param>
        /// <param name="options">Serialization options that influence how the processing is performed.</param>
        /// <returns>A <see cref="NumberLiteral"/> object extracted from the input JSON element.</returns>
        /// <exception cref="JsonException">Thrown if the input JSON element is not a valid number literal.</exception>
        public static NumberLiteral ProcessNumberLiteral(JsonElement el,
            JsonSerializerOptions options)
        {
            if (TryReadProcessNumberLiteral(el, options, out var numberLiteral))
            {
                return numberLiteral;
            }

            throw new JsonException("Expected value to be a valid number");
        }

        /// <summary>
        /// Attempts to read and process a JSON number literal from the given JSON element.
        /// </summary>
        /// <param name="el">The JSON element to read the number literal from.</param>
        /// <param name="options">The JSON serializer options to use during processing.</param>
        /// <param name="numberLiteral">When this method returns, contains the processed number literal if successful, or null if the operation failed.</param>
        /// <returns>True if the number literal is successfully read and processed; otherwise, false.</returns>
        public static bool TryReadProcessNumberLiteral(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out NumberLiteral numberLiteral)
        {
            if (el.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.String &&
                !"".Equals(v.GetString()))
            {
                numberLiteral = new NumberLiteral(v.GetString().AsMemory());
                return true;
            }

            numberLiteral = null;
            return false;
        }

        /// <summary>
        /// Reads and deserializes a JSON element into an implementation of the <see cref="IExpression"/> interface
        /// based on the specified "type" property in the JSON.
        /// </summary>
        /// <param name="el">The JSON element containing data for deserialization.</param>
        /// <param name="options">The options used to customize the JSON serialization and deserialization behavior.</param>
        /// <returns>An instance of an <see cref="IExpression"/> implementation corresponding to the type specified in the JSON.</returns>
        public static IExpression ReadExpression(JsonElement el, JsonSerializerOptions options)
        {
            var type = el.GetProperty("type").GetString();
            IExpression x = type switch
            {
                "DynamicReference" => DynamicReferenceSerializer.ProcessDynamicReference(el, options),
                "FunctionReference" => FunctionReferenceSerializer.ProcessFunctionReference(el, options),
                "MessageReference" => MessageReferenceSerializer.ProcessMessageReference(el, options),
                "NumberLiteral" => ProcessNumberLiteral(el, options),
                "Placeable" => PlaceableSerializer.ProcessPlaceable(el, options),
                "TermReference" => TermReferenceSerializer.ProcessTermReference(el, options),
                "StringLiteral" or "TextElement" or "TextLiteral" => ProcessTextLiteral(el, options),
                "VariableReference" => VariableReferenceSerializer.ProcessVariableReference(el, options),
                "SelectExpression" => SelectExpressionSerializer.ProcessSelectExpression(el, options),
                _ => throw new JsonException($"Unexpected type {type}")
            };
            return x;
        }

        /// <summary>
        /// Attempts to read an inline expression from the provided JSON element.
        /// </summary>
        /// <param name="el">The JSON element containing the data for the inline expression.</param>
        /// <param name="options">The JSON serializer options to customize reading behavior.</param>
        /// <param name="o">When this method returns, contains the parsed inline expression if the read operation is successful,
        /// or null if the read fails. This parameter is passed uninitialized.</param>
        /// <returns>True if the inline expression is successfully read; otherwise, false.</returns>
        /// <exception cref="JsonException">Thrown when the JSON data is invalid or contains unexpected types.</exception>
        public static bool TryReadInlineExpression(JsonElement el, JsonSerializerOptions options,
            [MaybeNullWhen(false)] out IInlineExpression o)
        {
            var type = el.GetProperty("type").GetString();
            o = type switch
            {
                "DynamicReference" => DynamicReferenceSerializer.ProcessDynamicReference(el, options),
                "FunctionReference" => FunctionReferenceSerializer.ProcessFunctionReference(el, options),
                "MessageReference" => MessageReferenceSerializer.ProcessMessageReference(el, options),
                "NumberLiteral" => ProcessNumberLiteral(el, options),
                "Placeable" => PlaceableSerializer.ProcessPlaceable(el, options),
                "TermReference" => TermReferenceSerializer.ProcessTermReference(el, options),
                "TextLiteral" => ProcessTextLiteral(el, options),
                "VariableReference" => VariableReferenceSerializer.ProcessVariableReference(el, options),
                _ => throw new JsonException($"Unexpected value {type}")
            };
            return true;
        }
    }
}