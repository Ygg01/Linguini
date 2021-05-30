#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using FluentAssertions.Json;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Linguini.Syntax.Tests.Parser
{
    [TestFixture]
    public class LinguiniFtlParserTest
    {
        private const string FilterComments = "$.body[?(@.type != 'GroupComment' && @.type != 'Comment' && @.type !='ResourceComment')]";
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
            list.AddRange(file.Split(@"/"));
            return Path.Combine(list.ToArray());
        }


        private static Resource ParseFtlFile(string path)
        {
            LinguiniParser parser;
            using (var reader = new StreamReader(path))
            {
                parser = new LinguiniParser(reader);
            }

            return parser.ParseWithComments();
        }
        
        private static Resource ParseFtlFileFast(string path)
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
        [TestCase("fixtures/any_char")]
        [TestCase("fixtures/astral")]
        [TestCase("fixtures/call_expressions")]
        [TestCase("fixtures/callee_expressions")]
        [TestCase("fixtures/comments")]
        [TestCase("fixtures/cr")]
        [TestCase("fixtures/crlf")]
        [TestCase("fixtures/eof_comment")]
        [TestCase("fixtures/eof_empty")]
        [TestCase("fixtures/eof_id")]
        [TestCase("fixtures/eof_id_equals")]
        [TestCase("fixtures/eof_junk")]
        [TestCase("fixtures/eof_value")]
        [TestCase("fixtures/escaped_characters")]
        [TestCase("fixtures/junk")]
        [TestCase("fixtures/leading_dots")]
        [TestCase("fixtures/literal_expressions")]
        [TestCase("fixtures/member_expressions")]
        [TestCase("fixtures/messages")]
        [TestCase("fixtures/mixed_entries")]
        [TestCase("fixtures/multiline_values")]
        [TestCase("fixtures/numbers")]
        [TestCase("fixtures/obsolete")]
        [TestCase("fixtures/placeables")]
        [TestCase("fixtures/reference_expressions")]
        [TestCase("fixtures/select_expressions")]
        [TestCase("fixtures/select_indent")]
        [TestCase("fixtures/sparse_entries")]
        [TestCase("fixtures/special_chars")]
        [TestCase("fixtures/tab")]
        [TestCase("fixtures/term_parameters")]
        [TestCase("fixtures/terms")]
        [TestCase("fixtures/variables")]
        [TestCase("fixtures/variant_keys")]
        [TestCase("fixtures/whitespace_in_value")]
        [TestCase("fixtures/zero_length")]
        public void TestReadFile(string file)
        {
            var path = GetFullPathFor(file);
            var res = ParseFtlFile(@$"{path}.ftl");
            var ftlAstJson = JsonSerializer.Serialize(res, TestJsonOptions());

            var expected = JToken.Parse(File.ReadAllText($@"{path}.json"));
            var actual = JToken.Parse(ftlAstJson);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        [Parallelizable]
        [TestCase("fixtures/any_char")]
        [TestCase("fixtures/astral")]
        [TestCase("fixtures/call_expressions")]
        [TestCase("fixtures/callee_expressions")]
        [TestCase("fixtures/cr")]
        [TestCase("fixtures/crlf")]
        [TestCase("fixtures/eof_comment")]
        [TestCase("fixtures/eof_empty")]
        [TestCase("fixtures/eof_id")]
        [TestCase("fixtures/eof_id_equals")]
        [TestCase("fixtures/eof_junk")]
        [TestCase("fixtures/eof_value")]
        [TestCase("fixtures/escaped_characters")]
        [TestCase("fixtures/junk")]
        [TestCase("fixtures/leading_dots")]
        [TestCase("fixtures/literal_expressions")]
        [TestCase("fixtures/member_expressions")]
        [TestCase("fixtures/messages")]
        [TestCase("fixtures/mixed_entries")]
        [TestCase("fixtures/multiline_values")]
        [TestCase("fixtures/numbers")]
        [TestCase("fixtures/obsolete")]
        [TestCase("fixtures/placeables")]
        [TestCase("fixtures/reference_expressions")]
        [TestCase("fixtures/select_expressions")]
        [TestCase("fixtures/select_indent")]
        [TestCase("fixtures/sparse_entries")]
        [TestCase("fixtures/special_chars")]
        [TestCase("fixtures/tab")]
        [TestCase("fixtures/term_parameters")]
        [TestCase("fixtures/terms")]
        [TestCase("fixtures/variables")]
        [TestCase("fixtures/variant_keys")]
        [TestCase("fixtures/whitespace_in_value")]
        [TestCase("fixtures/zero_length")]
        // [TestCase("fixtures/comments")] Errors in ignored comments aren't detected
        public void TestReadFast(string file)
        {
            var path = GetFullPathFor(file);
            var expected = JToken.Parse(File.ReadAllText($@"{path}.json"));
            var fastRes = ParseFtlFileFast(@$"{path}.ftl");
            var ftlAstFastJson = JsonSerializer.Serialize(fastRes, TestJsonOptions());

            var fastExpected = RemoveComments(expected);
            var fastActual = JToken.Parse(ftlAstFastJson);
            fastActual.Should().BeEquivalentTo(fastExpected);
        }

        private JToken RemoveComments(JToken actual)
        {
            
            var selectCommentFields = actual.SelectTokens("$..comment");
            foreach (var el in selectCommentFields)
            {
                el.Replace(null);
            }
            
            var commentTypes = actual.SelectTokens(FilterComments);
            JContainer body = new JArray();
            foreach (var el in commentTypes)
            {
                body.Add(el);
            }

            JContainer newToken = new JObject();
            newToken.Add(new JProperty("type", "Resource"));
            newToken.Add(new JProperty("body", body));

            return newToken;
        }
    }
}

