#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Linguini.Ast;
using Linguini.Parser;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using FluentAssertions.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Linguini.Tests.Parser
{
    [TestFixture]
    public class LinguiniFtlParserTest
    {
        private static string _baseTestDir = "";

        private static string BaseTestDir
        {
            get
            {
                if (_baseTestDir == "")
                {
                    // We discard the last three folders from WorkDirectory
                    // to get into common test directory
                    var testDirStrings = TestContext.CurrentContext.WorkDirectory
                        .Split(Path.DirectorySeparatorChar)[new Range(0, Index.FromEnd(3))];
                    _baseTestDir = Path.Combine(testDirStrings);
                }

                return _baseTestDir;
            }
        }

        private static JsonSerializerOptions TestJsonOptions()
        {
            return new()
            {
                IgnoreReadOnlyFields = false,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        private static string GetFullPathFor(string file)
        {
            List<string> list = new();
            list.Add(BaseTestDir);
            list.AddRange(file.Split(@"\"));
            return Path.Combine(list.ToArray());
        }


        private static Resource ParseFtlFile(string path)
        {
            LinguiniParser parser;
            using (var reader = new StreamReader(path))
            {
                parser = new LinguiniParser(reader);
            }

            return parser.Parse();
        }

        [Test]
        [Parallelizable]
        [TestCase(@"fixtures\eof_id")]
        [TestCase(@"fixtures\any_char")]
        [TestCase(@"fixtures\cr")]
        [TestCase(@"fixtures\comments")]
        [TestCase(@"fixtures\eof_comment")]
        [TestCase(@"fixtures\eof_empty")]
        [TestCase(@"fixtures\eof_id")]
        [TestCase(@"fixtures\eof_id_equals")]
        [TestCase(@"fixtures\eof_value")]
        [TestCase(@"fixtures\zero_length")]
        [TestCase(@"fixtures\special_chars")]
        [TestCase(@"fixtures\junk")]
        [TestCase(@"fixtures\eof_junk")]
        [TestCase(@"fixtures\tab")]
        
        [TestCase(@"fixtures\crlf")]
        // Don't work
        // [TestCase(@"fixtures\leading_dots")]
        // [TestCase(@"fixtures\escaped_characters")]
        // [TestCase(@"fixtures\astral")]
        // [TestCase(@"fixtures\literal_expressions")]
        // [TestCase(@"fixtures\member_expressions")]
        // [TestCase(@"fixtures\mixed_entries")]
        // [TestCase(@"fixtures\multiline_values")]
        // [TestCase(@"fixtures\numbers")]
        // [TestCase(@"fixtures\placeables")]
        // [TestCase(@"fixtures\tab")]
        // [TestCase(@"fixtures\terms")]
        // [TestCase(@"fixtures\reference_expressions")]
        // [TestCase(@"fixtures\select_expressions")]
        // [TestCase(@"fixtures\whitespace_in_value")]
        // [TestCase(@"fixtures\select_indent")]
        // [TestCase(@"fixtures\sparse_entries")]
        // [TestCase(@"fixtures\term_parameters")]
        // [TestCase(@"fixtures\variables")]
        // [TestCase(@"fixtures\variant_keys")]
        // [TestCase(@"fixtures\messages")]
        public void TestReadFile(string file)
        {
            var path = GetFullPathFor(file);
            var res = ParseFtlFile(@$"{path}.ftl");

            var expected = JToken.Parse( File.ReadAllText($@"{path}.json"));
            var actual = JToken.Parse(JsonSerializer.Serialize(res, TestJsonOptions()));
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

