using System;
using FluentSharp.IO;
using NUnit.Framework;

namespace FluentSharp.Tests.IO
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
        [TestCase("Северный поток", 'С')]
        [TestCase("", '\0')]
        public void TestPeekChar(string text, char expected)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected.Equals(reader.PeekChar()));
            Assert.That(expected.Equals(reader.PeekChar()));
            Assert.That(expected.Equals(reader.PeekChar()));
        }

        [Test]
        [Parallelizable]
        [TestCase("string", 's', 't')]
        [TestCase("漢字", '漢', '字')]
        [TestCase("かんじ", 'か', 'ん')]
        [TestCase("Северный поток", 'С', 'е')]
        [TestCase("", '\0', '\0')]
        public void TestPeekChar(string text, char expected1, char expected2)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected1.Equals(reader.PeekChar()));
            Assert.That(expected1.Equals(reader.GetChar()));
            Assert.That(expected2.Equals(reader.PeekChar()));
        }

        [Test]
        [Parallelizable]
        [TestCase("string", 's', 't')]
        [TestCase("漢字", '漢', '字')]
        [TestCase("かんじ", 'か', 'ん')]
        [TestCase("Северный поток", 'С', 'е')]
        [TestCase("", '\0', '\0')]
        public void TestPeekCharOffset(string text, char expected1, char expected2)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected1.Equals(reader.PeekChar()));
            Assert.That(expected2.Equals(reader.PeekChar(1)));
        }

        [Test]
        [Parallelizable]
        [TestCase("st", 's', 't', true)]
        [TestCase("str", 's', 't', false)]
        [TestCase("漢字", '漢', '字', true)]
        [TestCase("かんじ", 'か', 'ん', false)]
        [TestCase("Северный поток", 'С', 'е', false)]
        [TestCase("", '\0', '\0', true)]
        public void TestEof(string text, char expected1, char expected2, bool eof)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected1.Equals(reader.GetChar()));
            Assert.That(expected2.Equals(reader.GetChar()));
            Assert.That(reader.IsNotEof, Is.EqualTo(eof));
        }

        [Test]
        [Parallelizable]
        [TestCase("    \nb", 'b')]
        [TestCase("    \r\nb", 'b')]
        public void TestSkipBlank(string text, char postSkipChar)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            reader.SkipBlankBlock();
            Assert.That(postSkipChar.Equals(reader.GetChar()));
        }
    }
}
