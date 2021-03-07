using System;
using Linguini.Ast;
using Linguini.Parser;
using NUnit.Framework;

namespace Linguini.Tests.Parser
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
            Assert.That(parsed.Body.Count, Is.EqualTo(1));
            Assert.True(parsed.Body[0].TryConvert<IEntry, Comment>(out var comment));
            Assert.AreEqual(expectedCommentLevel, comment!.CommentLevel);
            Assert.AreEqual(expectedContent, comment.ContentStr());
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

        #region TermTest

        [Test]
        [Parallelizable]
        [TestCase("a = b", "a", "b")]
        [TestCase("a = \"b\"", "a", "\"b\"")]
        [TestCase("# comment\na = \"b\"", "a", "\"b\"")]
        [TestCase("a = b\n c", "a", "b\nc")]
        [TestCase("a = test\n  test", "a", "test\ntest")]
        [TestCase("a = test\r\n  test", "a", "test\ntest")]
        [TestCase("a = \n  test", "a", "test")]
        public void TestMessageParse(string input, string expName, string expValue)
        {
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.AreEqual(0, parsed.Errors.Count, "Failed, with errors");
            Assert.AreEqual(1, parsed.Body.Count);
            if (parsed.Body[0].TryConvert(out Message message)
                && message.Value != null)
            {
                Assert.AreEqual(expName, new string(message.Id.Name.ToArray()));
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
        [TestCase("## comment\na = b", false, "a", "comment")]
        public void TestMessageComment(string input, bool inTerm, string expMsg, string expComment)
        {
            var expBodySize = inTerm ? 1 : 2;
            Resource parsed = new LinguiniParser(input).Parse();
            Assert.AreEqual(0, parsed.Errors.Count);
            Assert.AreEqual(expBodySize, parsed.Body.Count);
            if (inTerm)
            {
                parsed.Body[0].TryConvert(out Message? msg);

                Assert.AreEqual(expMsg, new string(msg.Id.Name.ToArray()));
                Assert.AreEqual(expComment, msg.Comment.ContentStr());
            }
            else
            {
                parsed.Body[0].TryConvert(out Comment comment);
                parsed.Body[1].TryConvert(out Message msg);

                Assert.AreEqual(expComment, comment.ContentStr());
                Assert.AreEqual(expMsg, new string(msg.Id.Name.ToArray()));
            }
        }

        #endregion
    }
}
