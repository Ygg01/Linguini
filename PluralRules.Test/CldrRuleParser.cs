#nullable enable
using System;
using NUnit.Framework;
using PluralRules.Generator;
using PluralRules.Generator.Types;

namespace PluralRules.Test
{
    [TestFixture]
    [Parallelizable]
    public class Tests
    {
        [Test]
        [Parallelizable]
        [TestCase("n is 123", Operand.N, null, Op.Is, new[] {"123"})]
        [TestCase("n is not 456", Operand.N, null, Op.IsNot, new[] {"456"})]
        [TestCase("i% 10 not in 11..15", Operand.I, "10", Op.NotIn, new[] {"11..15"})]
        [TestCase("i % 10 in 131..15", Operand.I, "10", Op.In, new[] {"131..15"})]
        [TestCase("f% 3 = 1", Operand.F, "3", Op.Equal, new[] {"1"})]
        [TestCase("t != 5, 6", Operand.T, null, Op.NotEqual, new[] {"5", "6"})]
        [TestCase("w within 5, 6", Operand.W, null, Op.Within, new[] {"5", "6"})]
        [TestCase("v not within 3, 6..9", Operand.V, null, Op.NotWithin, new[] {"3", "6..9"})]
        [TestCase("n not != 3, 6..9", Operand.N, null, Op.Equal, new[] {"3", "6..9"})]
        [TestCase("n = 1", Operand.N, null, Op.Equal, new[] {"1"})]
        [TestCase("n % 10 in 131..15", Operand.N, "10", Op.In, new[] {"131..15"})]
        [TestCase("e != 0..6", Operand.E, null, Op.NotEqual, new[] {"0..6"})]
        public void BasicParseRule(string input, Operand expOperand, string? modulus, Op expOp,
            string[] expRangeList)
        {
            var rule = new CldrParser(input).ParseRule();
            Assert.IsNotNull(rule);
            var relation = rule.Condition.Conditions[0].Relations[0];
            Assert.AreEqual(expOperand, relation.Expr.Operand);

            if (modulus != null)
            {
                Assert.AreEqual(new DecimalValue(modulus), relation.Expr.Modulus);
            }
            else
            {
                Assert.IsNull(relation.Expr.Modulus);
            }

            Assert.AreEqual(expOp, relation.Op);
            for (var i = 0; i < expRangeList.Length; i++)
            {
                Assert.AreEqual(expRangeList[i], relation.RangeListItems[i].ToString());
            }
        }

        [Test]
        public void ParseEmpty()
        {
            var rule = new CldrParser("").ParseRule();
            Assert.IsNotNull(rule);
            Assert.IsEmpty(rule.Condition.Conditions);
            Assert.IsNull(rule.Samples);
        }

        [Test]
        [Parallelizable]
        [TestCase("n is 12 @integer 0, 5, 7~20", new[] {"0", "5", "7~20"}, new string[] { })]
        [TestCase("n is 12 @integer 0, 5, 7~20 @decimal 1, 3~6,...", new[] {"0", "5", "7~20"},
            new string[] {"1", "3~6"})]
        [TestCase("@integer 0, 11~25, 100, 1000,  …", new string[] {"0", "11~25", "100", "1000"}, new string[] { })]
        public void ParseSamples(string input, string[] expIntRangeList, string[] expDecRangeList)
        {
            var rule = new CldrParser(input).ParseRule();
            Assert.IsNotNull(rule.Samples);
            for (var i = 0; i < expIntRangeList.Length; i++)
            {
                Assert.AreEqual(expIntRangeList[i], rule.Samples?.IntegerSamples[i].ToString());
            }

            for (var i = 0; i < expDecRangeList.Length; i++)
            {
                Assert.AreEqual(expDecRangeList[i], rule.Samples?.DecimalSamples[i].ToString());
            }
        }

        [Test]
        [Parallelizable]
        [TestCase("v= 0 @integer 1c1, 1c2, 1c3, 1c4, 1c6", new[] {10, 100, 1000, 10000, 1000000}, null)]
        [TestCase("v= 0 @decimal 2c2, 2c3, 2c4, 2c6", null, new[] {200.0, 2000.0, 20000.0, 2000000.0})]
        [TestCase(" @integer 1c1, 1c2, 1c3, 1c4, 1c6 @decimal 3c2, 3c3, 3c4",
            new[] {10, 100, 1000, 10000, 1000000}, new[] {300.0, 3000.0, 30000.0})]
        public void TestParseExponent(string input, int[]? integerRanges, double[]? decimalRanges)
        {
            var rule = new CldrParser(input).ParseRule();
            Assert.IsNotNull(rule);
            Assert.IsNotNull(rule.Samples);
            if (integerRanges != null)
            {
                for (var index = 0; index < integerRanges.Length; index++)
                {
                    var expected = integerRanges[index];
                    var ruleStr = rule.Samples?.IntegerSamples[index].Lower.Value;
                    var dec = Double.Parse(ruleStr!);
                    Assert.AreEqual(expected, Convert.ToInt32(dec));
                }
            }

            if (decimalRanges != null)
            {
                for (var index = 0; index < decimalRanges.Length; index++)
                {
                    var expected = decimalRanges[index];
                    var ruleStr = rule.Samples?.DecimalSamples[index].Lower.Value;
                    var actual = Double.Parse(ruleStr!);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [Test]
        public void ParseCondition()
        {
            var rule = new CldrParser("i = 0 or n = 1 and f = 0 ").ParseRule();
            Assert.IsNotNull(rule.Condition);
            Assert.AreEqual(2, rule.Condition.Conditions.Count);
            Assert.AreEqual(1, rule.Condition.Conditions[0].Relations.Count);
            Assert.AreEqual(2, rule.Condition.Conditions[1].Relations.Count);
        }
    }
}
