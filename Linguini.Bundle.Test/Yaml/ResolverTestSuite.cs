using System.Collections.Generic;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Test.Yaml
{
    public class ResolverTestSuite
    {
        public string Name = default!;
        public List<string> Resources = new List<string>();
        public ResolverTestBundle? Bundle;
        public List<ResolverTest> Tests = new List<ResolverTest>();

        public class ResolverTestBundle
        {
            public List<string> Functions = new List<string>();
            public List<ResolverTestError> Errors = new List<ResolverTestError>();
            public string? TransformFunc;
            public bool UseIsolating;
            public bool Override;
        }

        public class ResolverTest
        {
            public string TestName = default!;
            public List<ResolverAssert> Asserts = new List<ResolverAssert>();
            public ResolverTestBundle? Bundle;
            public List<string> Resources = new List<string>();
            public List<ResolverTestError> ExpectedErrors = new List<ResolverTestError>();
        }

        public class ResolverAssert
        {
            public string Id = default!;
            public string? Attribute;
            public Dictionary<string, IFluentType> Args = new Dictionary<string, IFluentType>();
            public string ExpectedValue = default!;
            public List<ResolverTestError> ExpectedErrors = new List<ResolverTestError>();
            public bool? Missing = null;
        }


        public class ResolverTestError
        {
            public ErrorType Type;
            public string? Description;
        }
    }
}