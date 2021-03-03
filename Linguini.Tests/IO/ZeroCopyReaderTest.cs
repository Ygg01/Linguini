using System;
using Linguini.IO;
using NUnit.Framework;

namespace Linguini.Tests.IO
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
        [TestCase("단편", '단')]
        [TestCase("かんじ", 'か')]
        [TestCase("Северный поток", 'С')]
        public void TestPeekChar(string text, char expected)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected.EqualsSpans(reader.PeekCharSpan()));
            Assert.That(expected.EqualsSpans(reader.PeekCharSpan()));
            Assert.That(expected.EqualsSpans(reader.PeekCharSpan()));
        }


        [Test]
        [Parallelizable]
        [TestCase("string", 's', true, 't')]
        [TestCase("string", 'x', false, 's')]
        [TestCase("漢字", '漢', true, '字')]
        [TestCase("漢字", 'か', false, '漢')]
        [TestCase("かんじ", 'か', true, 'ん')]
        [TestCase("かんじ", 'ん', false, 'か')]
        [TestCase("단편", '단', true, '편')]
        [TestCase("단편", '편', false, '단')]
        [TestCase("Северный поток", 'С', true, 'е')]
        [TestCase("Северный поток", 'е', false, 'С')]
        [TestCase("", 'a', false, null)]
        public void TestExpectChar(string text, char expectedChr, bool expected, char? peek)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.AreEqual(expected, reader.ExpectChar(expectedChr));
            Assert.True(peek.EqualsSpans(reader.PeekCharSpan()));
        }

        [Test]
        [Parallelizable]
        [TestCase("string", 's', 't')]
        [TestCase("漢字", '漢', '字')]
        [TestCase("かんじ", 'か', 'ん')]
        [TestCase("단편", '단', '편')]
        [TestCase("Северный поток", 'С', 'е')]
        public void TestPeekGetChar(string text, char expected1, char expected2)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected1.EqualsSpans(reader.PeekCharSpan()));
            Assert.That(expected1.EqualsSpans(reader.GetCharSpan()));
            Assert.That(expected2.EqualsSpans(reader.PeekCharSpan()));
        }

        [Test]
        [Parallelizable]
        [TestCase("string", 's', 't')]
        [TestCase("漢字", '漢', '字')]
        [TestCase("かんじ", 'か', 'ん')]
        [TestCase("단편", '단', '편')]
        [TestCase("Северный поток", 'С', 'е')]
        public void TestPeekCharOffset(string text, char expected1, char expected2)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.That(expected1.EqualsSpans(reader.PeekCharSpan()));
            Assert.That(expected2.EqualsSpans(reader.PeekCharSpan(1)));
        }

        [Test]
        [Parallelizable]
        [TestCase("  \nb", 'b')]
        [TestCase("   \r\nb2", 'b')]
        [TestCase("    \n漢字", '漢')]
        [TestCase("     \n단편", '단')]
        [TestCase("      \nか", 'か')]
        public void TestSkipBlank(string text, char postSkipChar)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            reader.SkipBlankBlock();
            Assert.That(postSkipChar.EqualsSpans(reader.GetCharSpan()));
        }

        [Test]
        [Parallelizable]
        [TestCase("", false, null)]
        [TestCase("a", true, 'a')]
        public void TestTryReadCharSpan(string text, bool isChar, char? expected1)
        {
            ReadOnlyMemory<char> mem = new ReadOnlyMemory<char>(text.ToCharArray());
            bool IsThereChar = mem.TryReadCharSpan(0, out var readChr);
            Assert.That(IsThereChar, Is.EqualTo(isChar));
            Assert.That(expected1.EqualsSpans(readChr));
        }
    }
}
