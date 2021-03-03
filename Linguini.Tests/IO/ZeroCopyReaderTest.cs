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
            Assert.AreEqual(expected, reader.ReadByteIf(expectedChr));
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
            bool isThereChar = mem.TryReadCharSpan(0, out var readChr);
            Assert.That(isThereChar, Is.EqualTo(isChar));
            Assert.That(expected1.EqualsSpans(readChr));
        }
        
        [Test]
        [Parallelizable]
        [TestCase("string", 0, 1, "s")]
        [TestCase("string", 0, 2, "st")]
        [TestCase("string", 0, 3, "str")]
        [TestCase("string", 0, 4, "stri")]
        [TestCase("string", 0, 5, "strin")]
        [TestCase("string", 0, 6, "string")]
        [TestCase("Северный поток", 0, 7, "Северны")]
        [TestCase("かんじ", 0, 1, "か")]
        [TestCase("かんじ", 0, 2, "かん")]
        [TestCase("かんじ", 0, 3, "かんじ")]
        [TestCase("かんじ", 1, 2, "ん")]
        [TestCase("かんじ", 1, 3, "んじ")]
        [TestCase("かんじ", 2, 3, "じ")]
        public void TestTryReadSliceOk(string text, int start, int end, string expected)
        {
            ZeroCopyReader reader = new ZeroCopyReader(text);
            Assert.AreEqual(expected, reader.ReadSliceToStr(start, end));
            Assert.AreEqual(expected, reader.ReadSlice(start, end).ToArray());
        }
    }
}
