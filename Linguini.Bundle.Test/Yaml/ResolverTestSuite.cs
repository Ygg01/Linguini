using System;
using System.Collections.Generic;
using Linguini.Bundle.Errors;
using Linguini.Syntax.Ast;
using YamlDotNet.RepresentationModel;

namespace Linguini.Bundle.Test.Yaml
{
    public class ResolverTestSuite
    {
        public string Name;
        public string Resources;
        public ResolverTestBundle? Bundle;
        public List<ResolverTest> Tests = new List<ResolverTest>();

        public static ResolverTestSuite FromDoc(YamlDocument doc)
        {
            var testSuite = new ResolverTestSuite();
            var suites = (YamlSequenceNode) doc.RootNode["suites"][0]["suites"];
            foreach (var suiteProp in suites.Children)
            {
                if (suiteProp.TryConvert(out YamlMappingNode mapNode))
                {
                    if (mapNode.TryGetNode<YamlScalarNode>("name", out var name))
                    {
                        testSuite.Name = name.Value!;
                    }

                    if (mapNode.TryGetNode<YamlSequenceNode>("resources", out var resources))
                    {
                        ProcessResources(resources, testSuite);
                    }

                    if (mapNode.TryGetNode<YamlSequenceNode>("bundles", out var bundles))
                    {
                        ProcessBundles(bundles, out testSuite.Bundle);
                    }

                    if (mapNode.TryGetNode<YamlSequenceNode>("tests", out var tests))
                    {
                        testSuite.Tests = ProcessTests(tests);
                    }
                }
            }

            return testSuite;
        }

        private static List<ResolverTest> ProcessTests(YamlSequenceNode testsNode)
        {
            var testCollection = new List<ResolverTest>();
            foreach (var test in testsNode.Children)
            {
                testCollection.Add(ProcessTest((YamlMappingNode) test));
            }

            return testCollection;
        }

        private static ResolverTest ProcessTest(YamlMappingNode test)
        {
            var resolverTest = new ResolverTest();
            if (test.TryGetNode("name", out YamlScalarNode nameProp))
            {
                resolverTest.TestName = nameProp.Value!;
            }

            if (test.TryGetNode("asserts", out YamlSequenceNode asserts))
            {
                resolverTest.Asserts = ProcessAsserts(asserts);
            }

            return resolverTest;
        }

        private static List<ResolverAssert> ProcessAsserts(YamlSequenceNode assertsNodes)
        {
            var retVal = new List<ResolverAssert>();
            foreach (var assertNode in assertsNodes.Children)
            {
                if (assertNode.TryConvert(out YamlMappingNode assertMap))
                {
                    var resolverAssert = new ResolverAssert();

                    if (assertMap.TryGetNode("id", out YamlScalarNode idNode))
                    {
                        resolverAssert.Id = idNode.Value!;
                    }

                    if (assertMap.TryGetNode("value", out resolverAssert.ExpectedValue))
                    {
                        
                    }
                    
                    
                    retVal.Add(resolverAssert);
                }
            }

            return retVal;
        }

        private static void ProcessBundles(YamlSequenceNode bundles, out ResolverTestBundle? testBundle)
        {
            testBundle = new ResolverTestBundle();
            foreach (var bundleNode in bundles.Children)
            {
                if (bundleNode.TryConvert(out YamlMappingNode bundleMap))
                {
                    foreach (var keyValueNode in bundleMap.Children)
                    {
                        if (keyValueNode.Key.ToString().Equals("functions"))
                        {
                            ProcessFunctions((YamlSequenceNode) keyValueNode.Value, out testBundle.Functions);
                        }
                        else if (keyValueNode.Key.ToString().Equals("errors"))
                        {
                            testBundle.Errors = ProcessErrors((YamlSequenceNode) bundleMap["errors"]);
                        }
                    }
                }
            }
        }

        private static void ProcessFunctions(YamlSequenceNode functionsNode, out List<string> bundle)
        {
            bundle = new List<string>(functionsNode.Children.Count);
            foreach (var function in functionsNode)
            {
                if (function.TryConvert(out YamlScalarNode funcName))
                {
                    bundle.Add(funcName.Value!);
                }
            }
        }

        private static void ProcessResources(YamlSequenceNode returnNode, ResolverTestSuite testSuite)
        {
            foreach (var resNode in returnNode.Children)
            {
                if (resNode.TryConvert(out YamlMappingNode map))
                {
                    if (map.TryGetNode("source", out YamlScalarNode sourceValue))
                    {
                        testSuite.Resources = sourceValue.Value!;
                    }

                    if (map.TryGetNode("errors", out YamlSequenceNode errorNode))
                    {
                        if (testSuite.Bundle == null)
                        {
                            testSuite.Bundle = new ResolverTestBundle();
                        }

                        testSuite.Bundle.Errors = ProcessErrors(errorNode);
                    }
                }
            }
        }

        private static List<ResolverTestError> ProcessErrors(YamlSequenceNode errorNode)
        {
            List<ResolverTestError> resolverTestErrors = new();
            foreach (var error in errorNode.Children)
            {
                if (error.TryConvert(out YamlMappingNode errMap))
                {
                    var err = new ResolverTestError();
                    if (errMap.TryGetNode("type", out YamlScalarNode errType))
                    {
                        Enum.TryParse(errType.Value, out err.Type);
                    }

                    if (errMap.TryGetNode("desc", out YamlScalarNode errDesc))
                    {
                        err.Description = errDesc.Value;
                    }

                    resolverTestErrors.Add(err);
                }
            }

            return resolverTestErrors;
        }
    }

    public class ResolverTestBundle
    {
        public List<string> Functions = new();
        public List<ResolverTestError> Errors = new();
    }

    public class ResolverTest
    {
        public string TestName;
        public List<ResolverAssert> Asserts = new();
    }

    public class ResolverAssert
    {
        public string Id;
        public string? Attribute;
        public Dictionary<string, string> Args = new();
        public string ExpectedValue;
        public List<ResolverTestError> ExpectedErrors = new();
    }


    public class ResolverTestError
    {
        public ErrorType Type;
        public string? Description;
    }
}