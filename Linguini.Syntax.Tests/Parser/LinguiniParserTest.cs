using System;
using Linguini.Syntax.Ast;
using Linguini.Syntax.Parser;
using Linguini.Syntax.Parser.Error;
using NUnit.Framework;

#pragma warning disable 8602
#pragma warning disable 8600

namespace Linguini.Syntax.Tests.Parser
{
    [Parallelizable]
    [TestFixture]
    [TestOf(typeof(LinguiniParser))]
    public class LinguiniParserTest
    {
        #region CommentTest

        [Test]
        [Parallelizable]
        [TestCase("# Comment")]
        [TestCase("## Comment", CommentLevel.GroupComment)]
        [TestCase("### Comment", CommentLevel.ResourceComment)]
        [TestCase("### Funny funny", CommentLevel.ResourceComment, "Funny funny")]
        [TestCase("###  漢字", CommentLevel.ResourceComment, " 漢字")]
        [TestCase("# Comment\r\n")]
        [TestCase("# Comment\n")]
        public void TestCommentParse(string input, CommentLevel expectedCommentLevel = CommentLevel.Comment,
            string expectedContent = "Comment")
        {
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.That(parsed.Entries.Count, Is.EqualTo(1));
            Assert.True(parsed.Entries[0].TryConvert<IEntry, AstComment>(out var comment));
            Assert.AreEqual(expectedCommentLevel, comment!.CommentLevel);
            Assert.AreEqual(expectedContent, comment.AsStr());
        }

        [Test]
        [Parallelizable]
        [TestCase("#Comment", ErrorType.ExpectedToken,
            "Expected a token starting with  \" \"", 1, 2, 0, 8)]
        [TestCase("#Comment\n", ErrorType.ExpectedToken,
            "Expected a token starting with  \" \"", 1, 2, 0, 9)]
        [TestCase("#Comment\r\n", ErrorType.ExpectedToken,
            "Expected a token starting with  \" \"", 1, 2, 0, 10)]
        public void TestErrorCommentParse(string input, ErrorType expErrType, string expMsg, int start, int end,
            int sliceStart, int sliceEnd)
        {
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.That(parsed.Errors.Count, Is.EqualTo(1));
            Assert.AreEqual(expErrType, parsed.Errors[0].Kind);
            Assert.AreEqual(expMsg, parsed.Errors[0].Message);
            Assert.AreEqual(new Range(start, end), parsed.Errors[0].Position);
            Assert.AreEqual(new Range(sliceStart, sliceEnd), parsed.Errors[0].Slice);
        }

        #endregion

        #region MessageTest

        [Test]
        [Parallelizable]
        [TestCase("a = b", "a", "b")]
        [TestCase("a = \"b\"", "a", "\"b\"")]
        [TestCase("# comment\na = \"b\"", "a", "\"b\"")]
        [TestCase("hello = wo\n rld", "hello", "wo\nrld")]
        [TestCase("a = test\n  test", "a", "test\ntest")]
        [TestCase("a = test\r\n  test", "a", "test\ntest")]
        [TestCase("hello = \n  world", "hello", "world")]
        [TestCase("a = \ttest", "a", "\ttest")]
        [TestCase("a=\n\n  bar\n  baz", "a", "bar\nbaz")]
        public void TestMessageParse(string input, string expName, string expValue)
        {
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.AreEqual(0, parsed.Errors.Count, "Failed, with errors");
            Assert.AreEqual(1, parsed.Entries.Count);
            if (parsed.Entries[0].TryConvert(out AstMessage message)
                && message.Value != null)
            {
                Assert.AreEqual(expName, message.Id.ToString());
                Assert.AreEqual(expValue, message.Value.Stringify());
            }
            else
            {
                Assert.Fail("Failed to parse");
            }
        }

        [Test]
        [Parallelizable]
        [TestCase("# comment\na = b", true, "a", "comment")]
        [TestCase("## comment\nhello = world", false, "hello", "comment")]
        [TestCase("# Msg Comment\n# with blank line.\n#\nhello = term",
            true, "hello", "Msg Comment\nwith blank line.\n")]
        public void TestMessageComment(string input, bool inMessage, string expMsg, string expComment)
        {
            var expBodySize = inMessage ? 1 : 2;
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.AreEqual(0, parsed.Errors.Count);
            Assert.AreEqual(expBodySize, parsed.Entries.Count);
            if (inMessage)
            {
                parsed.Entries[0].TryConvert(out AstMessage? msg);

                Assert.AreEqual(expMsg, new string(msg.Id.Name.ToArray()));
#pragma warning disable 8602
                Assert.AreEqual(expComment, msg.Comment.AsStr());
#pragma warning restore 8602
            }
            else
            {
                parsed.Entries[0].TryConvert(out AstComment comment);
                parsed.Entries[1].TryConvert(out AstMessage msg);

                Assert.AreEqual(expComment, comment.AsStr());
                Assert.AreEqual(expMsg, new string(msg.Id.Name.ToArray()));
            }
        }

        #endregion

        [Test]
        [TestCase("# Term\r\n# blank line.\r\n#\r\n-term = Term", true, "term", "Term\nblank line.\n")]
        public void TestTermComment(string input, bool inTerm, string expTerm, string expComment)

        {
            var expBodySize = inTerm ? 1 : 2;
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.AreEqual(0, parsed.Errors.Count);
            Assert.AreEqual(expBodySize, parsed.Entries.Count);
            if (inTerm)
            {
                parsed.Entries[0].TryConvert(out AstTerm? term);

                Assert.AreEqual(expTerm, new string(term.Id.Name.ToArray()));
                Assert.AreEqual(expComment, term.Comment.AsStr());
            }
            else
            {
                parsed.Entries[0].TryConvert(out AstComment? comment);
                parsed.Entries[1].TryConvert(out AstTerm? term);

                Assert.AreEqual(expComment, comment!.AsStr());
                Assert.AreEqual(expTerm, new string(term!.Id.Name.ToArray()));
            }
        }

        [Test]
        [Parallelizable]
        [TestCase("num = {-3.14}", "num", "-3.14")]
        [TestCase("num = {123}", "num", "123")]
        public void TestNumExpressions(string input, string identifier, string value)
        {
            var res = new LinguiniParser(input).Parse();

            Assert.AreEqual(0, res.Errors.Count);
            Assert.AreEqual(1, res.Entries.Count);
            Assert.IsInstanceOf(typeof(AstMessage), res.Entries[0]);
            if (res.Entries[0].TryConvert(out AstMessage message))
            {
                Assert.AreEqual(1, message.Value.Elements.Count);
                Assert.IsInstanceOf(typeof(Placeable), message.Value.Elements[0]);
                message.Value.Elements[0].TryConvert(out Placeable placeable);
                Assert.NotNull(placeable);
                Assert.IsInstanceOf(typeof(NumberLiteral), placeable.Expression);
                placeable.Expression.TryConvert(out NumberLiteral numberLiteral);
                Assert.NotNull(numberLiteral);
                Assert.AreEqual(identifier, message.Id.ToString());
                Assert.AreEqual(value, numberLiteral!.ToString());
            }
        }
    }
}
