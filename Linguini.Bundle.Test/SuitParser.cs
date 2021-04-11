using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
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
                    var testDirStrings = TestContext
                        .CurrentContext
                        .WorkDirectory
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
        [Parallelizable]
        public void MyTestMethod(ResolverTestSuite parsedTestSuite)
        {
            var (bundle, errors) = LinguiniBundler.New()
                .Locale("en-US")
                .AddResource(parsedTestSuite.Resources)
                .SetUseIsolating(false)
                .Build();

            if (parsedTestSuite.Bundle != null)
            {
                foreach (var funcName in parsedTestSuite.Bundle.Functions)
                {
                    switch (funcName)
                    {
                        case "CONCAT":
                            bundle.AddFunction(funcName, LinguiniFluentFunctions.Concat, out _);
                            break;

                        case "SUM":
                            bundle.AddFunction(funcName, LinguiniFluentFunctions.Sum, out _);
                            break;

                        case "NUMBER":
                            bundle.AddFunction(funcName, LinguiniFluentFunctions.Number, out _);
                            break;

                        case "IDENTITY":
                            bundle.AddFunction(funcName, LinguiniFluentFunctions.Identity, out _);
                            break;
                        
                        default:
                            throw new ArgumentException($"Method name {funcName} doesn't exist");
                    }
                }
                AssertErrorCases(parsedTestSuite.Bundle.Errors, errors, parsedTestSuite.Name);
            }

            foreach (var test in parsedTestSuite.Tests)
            {
                foreach (var assert in test.Asserts)
                {
                    var actualValue = bundle.GetMsg(assert.Id, assert.Attribute, assert.Args, out var errs);
                    Assert.AreEqual(assert.ExpectedValue, actualValue, test.TestName);
                    AssertErrorCases(assert.ExpectedErrors, errs, test.TestName);
                }
            }
        }

        private static void AssertErrorCases(List<ResolverTestSuite.ResolverTestError> expectedErrors,
            IList<FluentError> errs,
            String testName)
        {
            Assert.AreEqual(expectedErrors.Count, errs.Count, testName);
            for (var i = 0; i < expectedErrors.Count; i++)
            {
                var actualError = errs[i];
                var expectedError = expectedErrors[i];

                Assert.AreEqual(expectedError.Type, actualError.ErrorKind());
                if (expectedError.Description != null)
                {
                    Assert.AreEqual(expectedError.Description, actualError.ToString());
                }
            }
        }

        static IEnumerable<TestCaseData> MyTestCases()
        {
            var path = "fixtures/arguments.yaml";
            var testSuites = ParseTest(path);
            foreach (var testCase in testSuites)
            {
                TestCaseData testCaseData = new TestCaseData(testCase);
                testCaseData.SetCategory(path);
                testCaseData.SetName($"({path}) {testCase.Name}");
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