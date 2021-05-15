using System;
using Linguini.Shared.IO;
using NUnit.Framework;

namespace Linguini.Syntax.Tests.IO
{
    [TestFixture]
    [TestOf(typeof(ZeroCopyUtil))]
    public class NonValidCharInputTest
    {
        [TestCase("string")]
        [TestCase("clear")]
        [TestCase("漢字")]
        [TestCase("단편")]
        [TestCase("かんじ")]
        [TestCase("Северный поток")]
        public void OperationOnNonCharSpanReturnFalse(string input)
        {
            var span = input.AsSpan();
            Assert.False('c'.EqualsSpans(span));
            Assert.False(span.IsEqual('c'));
            Assert.False(span.IsAsciiAlphabetic());
            Assert.False(span.IsAsciiDigit());
            Assert.False(span.IsAsciiHexdigit());
            Assert.False(span.IsAsciiUppercase());
            Assert.False(span.IsOneOf('c', 's'));
            Assert.False(span.IsOneOf('c', 's', 'l'));
            Assert.False(span.IsOneOf('c', 's', 'a', 'l'));
            Assert.False(span.IsNumberStart());
            Assert.False(input.AsMemory().IsCallee());
        }
    }
}
