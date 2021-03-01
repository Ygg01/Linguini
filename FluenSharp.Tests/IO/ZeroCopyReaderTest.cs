using FluentSharp.IO;
using NUnit.Framework;

namespace FluenSharp.Tests.IO
{
    [Parallelizable]
    [TestFixture]
    [TestOf(typeof(ZeroCopyReader))]
    public class ZeroCopyReaderTest
    {
        
        [Test]
        [Parallelizable]
        [TestCase("string", 's')]
        [TestCase("漢字", '漢')]
        [TestCase("かんじ", 'か')]
        [TestCase("Северный поток",'С')]
        [TestCase("",'\0')]
        public void TestPeekChar(string text, char expected)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(reader.PeekChar().CompareContent(expected));
            Assert.That(reader.PeekChar().CompareContent(expected));
            Assert.That(reader.PeekChar().CompareContent(expected));
        }
        
        [Test]
        [Parallelizable]
        [TestCase("string", 's','t')]
        [TestCase("漢字", '漢','字')]
        [TestCase("かんじ", 'か', 'ん')]
        [TestCase("Северный поток",'С', 'е')]
        [TestCase("",'\0','\0')]
        public void TestPeekChar(string text, char expected1, char expected2)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(reader.PeekChar().CompareContent(expected1));
            Assert.That(reader.GetChar().CompareContent(expected1));
            Assert.That(reader.PeekChar().CompareContent(expected2));
        }
    }
}
