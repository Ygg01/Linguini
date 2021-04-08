using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Test.Yaml;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;

namespace Linguini.Bundle.Test
{
    public class SuitParser
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

        private static string GetFullPathFor(string file)
        {
            List<string> list = new();
            list.Add(BaseTestDir);
            list.AddRange(file.Split(@"/"));
            return Path.Combine(list.ToArray());
        }

        [TestCaseSource(nameof(MyTestCases))]
        public void MyTestMethod(ResolverTestSuite parsedTestSuite)
        {
            // Your test code here
        }

        static IEnumerable<TestCaseData> MyTestCases()
        {
            var testSuites = ParseTest("fixtures/test.yaml");
            foreach (var testCase in testSuites)
            {
                TestCaseData testCaseData = new TestCaseData(testCase);
                testCaseData.SetName(testCase.Name);
                yield return testCaseData;
            } 
                
        }

        private static List<ResolverTestSuite> ParseTest(string name)
        {
            var path = GetFullPathFor(name);
            using var reader = new StreamReader(path);
            YamlStream yamlStream = new();
            yamlStream.Load(reader);
            YamlDocument doc = yamlStream.Documents[0];
            return doc.ParseResolverTests();
        }
    }
}