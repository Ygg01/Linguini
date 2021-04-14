using System;
using System.Collections.Generic;
using System.IO;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Func;
using Linguini.Bundle.Test.Yaml;
using Linguini.Bundle.Types;
using Linguini.Syntax.Ast;
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

        static IEnumerable<TestCaseData> MyTestCases()
        {
            var defaultPath = GetFullPathFor("fixtures/defaults.yaml");
            var defaultBuilder = ParseDefault(defaultPath);

            string[] files = Directory.GetFiles(GetFullPathFor("fixtures"));
            // string[] files = {GetFullPathFor("fixtures/mre.yaml")};
            foreach (var path in files)
            {
                if (path.Equals(defaultPath))
                {
                    continue;
                }

                var (testSuites, suiteName) = ParseTest(path);
                foreach (var testCase in testSuites)
                {
                    TestCaseData testCaseData = new(testCase, defaultBuilder);
                    testCaseData.SetCategory(path);
                    testCaseData.SetName($"({path}) {suiteName} {testCase.Name}");
                    yield return testCaseData;
                }
            }
        }


        [TestCaseSource(nameof(MyTestCases))]
        [Parallelizable]
        public void MyTestMethod(ResolverTestSuite parsedTestSuite, LinguiniBundler.IReadyStep builder)
        {
            var bundle = builder.UncheckedBuild();
            var errors = new List<FluentError>();
            foreach (var res in parsedTestSuite.Resources)
            {
                bundle.AddResource(res, out var err);
                errors.AddRange(err);
            }

            if (parsedTestSuite.Bundle != null)
            {
                foreach (var funcName in parsedTestSuite.Bundle.Functions)
                {
                    switch (funcName)
                    {
                        case "CONCAT":
                            AddFunc(bundle, funcName, LinguiniFluentFunctions.Concat, errors);
                            break;
                        case "SUM":
                            AddFunc(bundle, funcName, LinguiniFluentFunctions.Sum, errors);
                            break;
                        case "NUMBER":
                            AddFunc(bundle, funcName, LinguiniFluentFunctions.Number, errors);
                            break;
                        case "IDENTITY":
                            AddFunc(bundle, funcName, LinguiniFluentFunctions.Identity, errors);
                            break;
                        default:
                            throw new ArgumentException($"Method name {funcName} doesn't exist");
                    }
                }

                var transformFunc = parsedTestSuite.Bundle.TransformFunc;
                if (transformFunc != null)
                {
                    switch (transformFunc)
                    {
                        case "example":
                            bundle.TransformFunc = s => s.Replace('a', 'A');
                            break;
                        default:
                            throw new ArgumentException($"Unknown method {transformFunc}");
                    }
                }

                bundle.UseIsolating = parsedTestSuite.Bundle.UseIsolating;
                AssertErrorCases(parsedTestSuite.Bundle.Errors, errors, parsedTestSuite.Name);
            }

            foreach (var test in parsedTestSuite.Tests)
            {
                var testBundle = bundle;
                if (test.Resources.Count > 0)
                {
                    testBundle = bundle.DeepClone();
                    foreach (var res in test.Resources)
                    {
                        testBundle.AddResource(res, out var errs);
                        errors.AddRange(errs);
                    }
                }

                foreach (var assert in test.Asserts)
                {
                    var actualValue = testBundle.GetMsg(assert.Id, assert.Attribute, assert.Args, out var errs);
                    Assert.AreEqual(assert.ExpectedValue, actualValue, test.TestName);
                    AssertErrorCases(assert.ExpectedErrors, errs, test.TestName);
                }
            }
        }

        private static void AddFunc(FluentBundle bundle, string funcName, ExternalFunction externalFunction,
            List<FluentError> errors)
        {
            bundle.AddFunction(funcName, externalFunction, out var errs);
            if (errs is {Count: > 0})
            {
                errors.AddRange(errs);
            }
        }

        private static string GetFullPathFor(string file)
        {
            List<string> list = new();
            list.Add(BaseTestDir);
            list.AddRange(file.Split(@"/"));
            return Path.Combine(list.ToArray());
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


        private static (List<ResolverTestSuite>, string) ParseTest(string name)
        {
            var doc = ParseYamlDoc(name);
            var suiteName = doc.RootNode["suites"][0]["name"].AsString();
            return (doc.ParseResolverTests(), suiteName);
        }

        private static YamlDocument ParseYamlDoc(string path)
        {
            using var reader = new StreamReader(path);
            YamlStream yamlStream = new();
            yamlStream.Load(reader);
            YamlDocument doc = yamlStream.Documents[0];
            return doc;
        }

        private static LinguiniBundler.IReadyStep ParseDefault(string path)
        {
            var yamlBundle = ParseYamlDoc(path)
                .RootNode["bundle"];

            List<string> locales = new();
            var isIsolating = false;
            if (yamlBundle.TryConvert(out YamlMappingNode map))
            {
                if (map.TryGetNode("useIsolating", out YamlScalarNode useIsolatingNode))
                {
                    isIsolating = useIsolatingNode.AsBool();
                }

                if (map.TryGetNode("locales", out YamlSequenceNode localesNode))
                {
                    foreach (var localeNode in localesNode.Children)
                    {
                        locales.Add(localeNode.AsString());
                    }
                }
            }

            var bundler = LinguiniBundler.New()
                .Locales(locales)
                .SkipResources()
                .SetUseIsolating(isIsolating);

            return bundler;
        }
    }
}