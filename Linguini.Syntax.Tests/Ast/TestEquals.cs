using System.Collections.Generic;
using Linguini.Syntax.Ast;
using Linguini.Syntax.IO;
using NUnit.Framework;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable once EqualExpressionComparison

namespace Linguini.Syntax.Tests.Ast
{
    [TestFixture]
    [Parallelizable]
    public class TestEquals
    {
        private static IEnumerable<TestCaseData> AstCommentExample()
        {
            var same = new AstComment(CommentLevel.Comment, "comment");
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new AstComment(CommentLevel.GroupComment, "comment"), false);
            yield return new TestCaseData(same, new AstComment(CommentLevel.Comment, "commentzzzz"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstCommentExample))]
        public void TestAstComment(AstComment comment, object? other, bool expected)
        {
            if (other is AstComment otherComment)
            {
                Assert.That(comment == otherComment, Is.EqualTo(expected));
                Assert.That(comment != otherComment, Is.EqualTo(!expected));
                Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                Assert.That(comment.GetId() == otherComment.GetId());
            }

            if (other is null)
            {
                Assert.That(comment.Equals((AstComment?)other), Is.EqualTo(expected));
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AstLocData()
        {
            var same = AstLocation.FromRowAndFilename(3, "text.ftl");
            var fromReader = AstLocation.FromReader(new ZeroCopyReader("aaaa"));
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, AstLocation.FromRowAndFilename(5, "text.ftl"), false);
            yield return new TestCaseData(same, AstLocation.FromRowAndFilename(3, "testing.ftl"), false);
            yield return new TestCaseData(same, fromReader, false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstLocData))]
        public void TestLocation(AstLocation comment, object? other, bool expected)
        {
            if (other is AstLocation otherComment)
            {
                Assert.That(comment == otherComment, Is.EqualTo(expected));
                Assert.That(comment != otherComment, Is.EqualTo(!expected));
                Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
            }

            if (other is null)
            {
                Assert.That(comment.Equals((AstLocation?)other), Is.EqualTo(expected));
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AstTextData()
        {
            var same = new TextLiteral("ex");
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new TextLiteral("heh"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstTextData))]
        public void TestTextLiteral(TextLiteral comment, object? other, bool expected)
        {
            if (other is TextLiteral otherComment)
            {
                Assert.That(comment == otherComment, Is.EqualTo(expected));
                Assert.That(comment != otherComment, Is.EqualTo(!expected));
                Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
            }

            if (other is null)
            {
                Assert.That(comment.Equals((TextLiteral?)other), Is.EqualTo(expected));
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AstNumberData()
        {
            var same = new NumberLiteral(1);
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new NumberLiteral(3.2), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstNumberData))]
        public void TestNumberData(NumberLiteral comment, object? other, bool expected)
        {
            if (other is NumberLiteral otherComment)
            {
                Assert.That(comment == otherComment, Is.EqualTo(expected));
                Assert.That(comment != otherComment, Is.EqualTo(!expected));
                Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
            }

            if (other is null)
            {
                Assert.That(comment.Equals((NumberLiteral?)other), Is.EqualTo(expected));
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }


        private static IEnumerable<TestCaseData> IdentifierTestData()
        {
            Identifier same = "aaa";
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new Identifier("bbb"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(IdentifierTestData))]
        public void TestIdentifier(Identifier comment, object? other, bool expected)
        {
            if (other is Identifier otherComment)
            {
                Assert.That(comment == otherComment, Is.EqualTo(expected));
                Assert.That(comment != otherComment, Is.EqualTo(!expected));
                Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
            }

            if (other is null)
            {
                Assert.That(comment.Equals((Identifier?)other), Is.EqualTo(expected));
                Assert.That(Identifier.Comparer.Equals(comment, null), Is.False);
                Assert.That(Identifier.Comparer.Equals(null, comment), Is.False);
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AttributeTestData()
        {
            var same = new Attribute("zzz", Pattern.Builder().AddText("top"));
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same,
                new Attribute("xyz", Pattern.From("top")), false);
            yield return new TestCaseData(same,
                new Attribute("zzz", Pattern.Builder().AddText("xxxx")), false);
            yield return new TestCaseData(same, new
                    Attribute("x1", Pattern.Builder().AddDynamicReference("dyn-rfe")), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AttributeTestData))]
        public void TestAttribute(Attribute comment, object? other, bool expected)
        {
            if (other is Attribute otherComment)
            {
                Assert.That(comment == otherComment, Is.EqualTo(expected));
                Assert.That(comment != otherComment, Is.EqualTo(!expected));
                Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
            }

            if (other is null)
            {
                Assert.That(comment.Equals((Attribute?)other), Is.EqualTo(expected));
                Assert.That(Attribute.Comparer.Equals(comment, null), Is.False);
                Assert.That(Attribute.Comparer.Equals(null, comment), Is.False);
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }
    }
}