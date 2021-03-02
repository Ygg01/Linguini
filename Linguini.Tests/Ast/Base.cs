using Linguini.Ast;
using NUnit.Framework;

namespace Linguini.Tests.Ast
{
    [Parallelizable]
    [TestFixture]
    [TestOf(typeof(Base))]
    public class Base
    {
        private static Message Message = new();
        private static Comment Comment = new();
        private static Term Term = new();
        private static Junk Junk = new();

        [Test]
        public void TestTryConvert()
        {
            var msgToMsg = Message.TryConvert<Message>(out _);
            Assert.True(msgToMsg, "Can't convert Message to Message");
            var msgToComment = Message.TryConvert<Comment>(out _);
            Assert.False(msgToComment, "Conversion Message to Comment is illegal");
            var msgToTerm = Message.TryConvert<Comment>(out _);
            Assert.False(msgToTerm, "Conversion Message to Term is illegal");
            var msgToJunk = Message.TryConvert<Comment>(out _);
            Assert.False(msgToJunk, "Conversion Message to Junk is illegal");
        }
    }
}
