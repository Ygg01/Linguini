using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linguini.Serialization.Converters;

namespace Linguini.Serialization.Test
{
    public static class TestUtil
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            IgnoreReadOnlyFields = false,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new AttributeSerializer(),
                new CallArgumentsSerializer(),
                new CommentSerializer(),
                new FunctionReferenceSerializer(),
                new IdentifierSerializer(),
                new JunkSerializer(),
                new MessageReferenceSerializer(),
                new MessageSerializer(),
                new DynamicReferenceSerializer(),
                new NamedArgumentSerializer(),
                new ParseErrorSerializer(),
                new PatternSerializer(),
                new PlaceableSerializer(),
                new ResourceSerializer(),
                new PlaceableSerializer(),
                new SelectExpressionSerializer(),
                new TermReferenceSerializer(),
                new TermSerializer(),
                new VariantSerializer(),
                new VariableReferenceSerializer(),
            }
        };
    }
}