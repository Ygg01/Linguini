using System.Collections.Generic;
using Linguini.Syntax.Ast;
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
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, AstLocation.FromRowAndFilename(5, "text.ftl"), false);
            yield return new TestCaseData(same, AstLocation.FromRowAndFilename(3, "testing.ftl"), false);
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
    }
}