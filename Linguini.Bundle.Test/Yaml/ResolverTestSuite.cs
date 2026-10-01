using System.Collections.Generic;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;

namespace Linguini.Bundle.Test.Yaml
{
    public class ResolverTestSuite
    {
        public ResolverTestBundle? Bundle;
        public string Name = default!;
        public List<string> Resources = new();
        public List<ResolverTest> Tests = new();

        public class ResolverTestBundle
        {
            public List<ResolverTestError> Errors = new();
            public List<string> Functions = new();
            public bool Override;
            public string? TransformFunc;
            public bool UseIsolating;
        }

        public class ResolverTest
        {
            public List<ResolverAssert> Asserts = new();
            public ResolverTestBundle? Bundle;
            public List<ResolverTestError> ExpectedErrors = new();
            public List<string> Resources = new();
            public string TestName = default!;
        }

        public class ResolverAssert
        {
            public Dictionary<string, IFluentType> Args = new();
            public string? Attribute;
            public List<ResolverTestError> ExpectedErrors = new();
            public string ExpectedValue = default!;
            public string Id = default!;
            public bool? Missing = null;
        }


        public class ResolverTestError
        {
            public string? Description;
            public ErrorType Type;
        }
    }
}