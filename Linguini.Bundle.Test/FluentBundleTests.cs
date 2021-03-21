using NUnit.Framework;

namespace Linguini.Bundle.Test
{
    public class Tests
    {

        [Test]
        [Parallelizable]
        [TestCase("-term1 = x1\nmsg1 = x2", "term1", "x1", "msg1", "x2")]
        [TestCase("-term1-term = x1\nmsg= 단편 ", "term1-term", "x1", "msg", "단편")]
        public void BuilderTest(string resource, string term, string termValue, string msg, string msgValue)
        {
            var bundle = LinguiniBundler.New()
                .Locale("en-US")
                .AddResource(resource)
                .SetUseIsolating(false)
                .UncheckedBuild();

            bundle.TryGetMessage(msg, out var astMessage);
            bundle.TryGetTerm(term, out var astTerm);
            Assert.IsNotNull(astMessage);
            Assert.IsNotNull(astMessage.Value);
            Assert.IsNotNull(astTerm);
            Assert.AreEqual(msg, new string(astMessage.Id.Name.Span));
            Assert.AreEqual(msgValue, new string(astMessage.Value.ToString()));
            Assert.AreEqual(term, new string(astTerm.Id.Name.Span));
            Assert.AreEqual(termValue, new string(astTerm.Value.ToString()));
        }
    }
}
