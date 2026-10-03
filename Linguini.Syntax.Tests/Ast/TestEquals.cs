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
        [Test]
        public void TestAstComment()
        {
            var comment1 = new AstComment(CommentLevel.Comment, "comment");
            var same = new AstComment(CommentLevel.Comment, "comment");
            var diffLvl = new AstComment(CommentLevel.GroupComment, "comment!");
            var diffTxt = new AstComment(CommentLevel.Comment, "comment !!!!");

            Assert.That(comment1 != null, "Not equals null");
            Assert.That(comment1 == comment1, "Equals comment");
            Assert.That(comment1 == same, "Equals same comment");
            
            Assert.That(!comment1.Equals((object?)null), "Not equals object? null");
            Assert.That(!comment1.Equals(null), "Not equals null");
            Assert.That(comment1.Equals((object?)comment1), "Same object? ref");
            Assert.That(comment1.Equals(comment1), "Same ref");
            Assert.That(comment1.Equals(same), "Same fields");
            Assert.That(comment1.Equals((object?)same), "Same object? fields");

            Assert.That(!comment1.Equals(diffLvl), "Different level");
            Assert.That(!comment1.Equals(diffTxt), "Different level");
            Assert.That(!comment1.Equals(3), "Not same hash values");

            Assert.That(comment1.GetHashCode() == same.GetHashCode(), "Same value hash");
            Assert.That(comment1.GetHashCode() != diffLvl.GetHashCode(), "Different level hash");
        }
    }
}