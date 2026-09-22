using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Syntax.Parser.Error;

namespace Linguini.Serialization.Converters
{
    /// <summary>
    /// A JSON converter for serializing and deserializing instances of the <see cref="ParseError"/> class.
    /// </summary>
    public class ParseErrorSerializer : JsonConverter<ParseError>
    {
        /// <inheritdoc />
        public override ParseError? Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var el = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return ProcessParseError(el);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, ParseError error, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("kind");
            writer.WriteStringValue(error.Kind.ToString());
            writer.WritePropertyName("message");
            writer.WriteStringValue(error.Message);
            writer.WritePropertyName("row");
            writer.WriteNumberValue(error.Row);
            WriteRange(writer, "position", error.Position);
            WriteRange(writer, "slice", error.Slice);
            writer.WriteEndObject();
        }

        private static void WriteRange(Utf8JsonWriter writer, string name, Range? range)
        {
            if (range == null)
                return;

            writer.WritePropertyName(name);
            writer.WriteStartObject();
            writer.WritePropertyName("start");
            writer.WriteNumberValue(range.Value.Start.Value);
            writer.WritePropertyName("end");
            writer.WriteNumberValue(range.Value.End.Value);
            writer.WriteEndObject();
        }


        private static ParseError ProcessParseError(JsonElement el)
        {
            if (!el.TryGetProperty("kind", out var jsonId) ||
                !TryGetErrorType(jsonId, out var kind))
            {
                throw new JsonException("Expected kind property");
            }

            if (!TryGetString(el, "message", out var message))
            {
                throw new JsonException("Expected message property");
            }

            if (!TryGetInt(el, "row", out var row))
            {
                throw new JsonException("Expected row property");
            }

            if (!el.TryGetProperty("position", out var jsonPosition) ||
                !TryGetRange(jsonPosition, out var position))
            {
                throw new JsonException("Expected position property");
            }

            Range? slice = null;
            if (el.TryGetProperty("slice", out var sliceJson))
            {
                TryGetRange(sliceJson, out slice);
            }

            return ParseError.SerializeParseError(
                kind.Value, message, position.Value, slice, row.Value);
        }

        private static bool TryGetErrorType(JsonElement jsonValue, [NotNullWhen(true)] out ErrorType? outType)
        {
            var kindStr = jsonValue.ToString();

            switch (kindStr)
            {
                case "ExpectedToken":
                    outType = ErrorType.ExpectedToken;
                    return true;
                case "ExpectedCharRange":
                    outType = ErrorType.ExpectedCharRange;
                    return true;
                case "ExpectedMessageField":
                    outType = ErrorType.ExpectedMessageField;
                    return true;
                case "MissingValue":
                    outType = ErrorType.MissingValue;
                    return true;
                case "UnbalancedClosingBrace":
                    outType = ErrorType.UnbalancedClosingBrace;
                    return true;
                case "TermAttributeAsPlaceable":
                    outType = ErrorType.TermAttributeAsPlaceable;
                    return true;
                case "ExpectedTermField":
                    outType = ErrorType.ExpectedTermField;
                    return true;
                case "MessageReferenceAsSelector":
                    outType = ErrorType.MessageReferenceAsSelector;
                    return true;
                case "MessageAttributeAsSelector":
                    outType = ErrorType.MessageAttributeAsSelector;
                    return true;
                case "TermReferenceAsSelector":
                    outType = ErrorType.TermReferenceAsSelector;
                    return true;
                case "ExpectedSimpleExpressionAsSelector":
                    outType = ErrorType.ExpectedSimpleExpressionAsSelector;
                    return true;
                case "UnterminatedStringLiteral":
                    outType = ErrorType.UnterminatedStringLiteral;
                    return true;
                case "UnknownEscapeSequence":
                    outType = ErrorType.UnknownEscapeSequence;
                    return true;
                case "ForbiddenCallee":
                    outType = ErrorType.ForbiddenCallee;
                    return true;
                case "ExpectedLiteral":
                    outType = ErrorType.ExpectedLiteral;
                    return true;
                case "ExpectedInlineExpression":
                    outType = ErrorType.ExpectedInlineExpression;
                    return true;
                case "DuplicatedNamedArgument":
                    outType = ErrorType.DuplicatedNamedArgument;
                    return true;
                case "PositionalArgumentFollowsNamed":
                    outType = ErrorType.PositionalArgumentFollowsNamed;
                    return true;
                case "MultipleDefaultVariants":
                    outType = ErrorType.MultipleDefaultVariants;
                    return true;
                case "MissingDefaultVariant":
                    outType = ErrorType.MissingDefaultVariant;
                    return true;
                case "InvalidUnicodeEscapeSequence":
                    outType = ErrorType.InvalidUnicodeEscapeSequence;
                    return true;
                default:
                    outType = null;
                    return false;
            }
        }

        private static bool TryGetRange(JsonElement el,
            [NotNullWhen(true)] out Range? outType)
        {
            el.TryGetProperty("start", out var startValue);
            el.TryGetProperty("end", out var endValue);
            if (startValue.ValueKind != JsonValueKind.Number || endValue.ValueKind != JsonValueKind.Number)
            {
                outType = null;
                return false;
            }

            outType = new Range(startValue.GetInt32(), endValue.GetInt32());
            return true;
        }

        private static bool TryGetString(JsonElement el, string name,
            [NotNullWhen(true)] out string? outType)
        {
            if (!el.TryGetProperty(name, out var valueElement) || valueElement.ValueKind != JsonValueKind.String)
            {
                outType = null;
                return false;
            }

            outType = valueElement.GetString() ?? "";
            return true;
        }

        private static bool TryGetInt(JsonElement el, string name,
            [NotNullWhen(true)] out int? outType)
        {
            if (!el.TryGetProperty(name, out var valueElement) || valueElement.ValueKind != JsonValueKind.Number)
            {
                outType = null;
                return false;
            }

            outType = valueElement.GetInt32();
            return true;
        }
    }
}