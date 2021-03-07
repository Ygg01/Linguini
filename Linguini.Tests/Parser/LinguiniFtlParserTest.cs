#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Linguini.Ast;
using Linguini.Parser;
using NUnit.Framework;

namespace Linguini.Tests.Parser
{
    [TestFixture]
    public class LinguiniFtlParserTest
    {
        private static string _baseTestDir = "";

        public static string BaseTestDir
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

        public static string GetFullPathFor(string file)
        {
            List<string> list = new();
            list.Add(BaseTestDir);
            list.AddRange(file.Split(@"\"));
            return Path.Combine(list.ToArray());
        }


        public static Resource ParseTextFile(string path)
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
        [TestCase(@"file_tests\empty")]
        [TestCase(@"file_tests\comment")]
        public void TestReadFile(string file)
        {
            var path = GetFullPathFor(file);
            var res = ParseTextFile(@$"{path}.ftl");
            JsonSerializerOptions options = new();
            options.IgnoreReadOnlyFields = false;
            options.WriteIndented = true;
            string parsed = JsonSerializer.Serialize(res, options);
            string expected = File.ReadAllText(@$"{path}.json");
            Assert.AreEqual(expected, parsed);
        }
    }
}
