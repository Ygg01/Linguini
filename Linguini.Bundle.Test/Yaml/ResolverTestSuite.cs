using System.Collections.Generic;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;

namespace Linguini.Bundle.Test.Yaml
{
    public class ResolverTestSuite
    {
        public string Name = default!;
        public string Resources = default!;
        public ResolverTestBundle? Bundle;
        public List<ResolverTest> Tests = new();

        public class ResolverTestBundle
        {
            public List<string> Functions = new();
            public List<ResolverTestError> Errors = new();
        }

        public class ResolverTest
        {
            public string TestName = default!;
            public bool Skip;
            public List<ResolverAssert> Asserts = new();
            public ResolverTestBundle? Bundle;
        }

        public class ResolverAssert
        {
            public string Id = default!;
            public string? Attribute;
            public Dictionary<string, IFluentType> Args = new();
            public string ExpectedValue = default!;
            public List<ResolverTestError> ExpectedErrors = new();
            public bool? Missing;
        }


        public class ResolverTestError
        {
            public ErrorType Type;
            public string? Description;
        }
    }
}