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
            var yamlTest = YamlTest();


            yield return new TestCaseData(yamlTest);
        }

        private static ResolverTestSuite YamlTest()
        {
            var path = GetFullPathFor(@"fixtures/test.yaml");
            using var reader = new StreamReader(path);
            YamlStream yamlStream = new YamlStream();
            yamlStream.Load(reader);
            YamlDocument doc = yamlStream.Documents[0];
            var yamlTest =  ResolverTestSuite.FromDoc(doc);
          
        
            return yamlTest;
        }
        
    }



    
 
}