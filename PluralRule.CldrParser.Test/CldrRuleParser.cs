using NUnit.Framework;
using PluralRule.CldrParser.Ast;

namespace PluralRule.CldrParser.Test
{
    [TestFixture]
    [Parallelizable]
    public class Tests
    {
        [Test]
        public void BasicParse()
        {
            var rule = new Ast.CldrParser("n % 3").ParseRule();
            Assert.IsNotNull(rule);
            Assert.AreEqual(1,rule.Condition.Conditions.Count);
            Assert.AreEqual(1,rule.Condition.Conditions[0].Relations.Count);
            Assert.AreEqual(Operand.N,rule.Condition.Conditions[0].Relations[0].Expr.Operand);
        }
    }
}