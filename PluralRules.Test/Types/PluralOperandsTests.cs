using System;
using System.Globalization;
using Linguini.Shared.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace PluralRules.Test.Types
{
    public class PluralOperandsTests
    {
        [Test]
        [Parallelizable]
        [TestCase(0, 0, 0, 0, 0, 0, "0")]
        [TestCase(2, 2, 0, 0, 0, 0, "2")]
        [TestCase(57, 57, 0, 0, 0, 0, "57")]
        [TestCase(987, 987, 0, 0, 0, 0, "987")]
        [TestCase(1234567, 1234567, 0, 0, 0, 0, "1234567")]
        [TestCase(10, 10, 0, 0, 0, 0, "-10")]
        [TestCase(1000000, 1000000, 0, 0, 0, 0, "-1000000")]
        [TestCase(0.23, 0, 2, 2, 23, 23, "-0.23")]
        [TestCase(0.230, 0, 3, 2, 230, 23, "0.230")]
        [TestCase(23.00, 23, 2, 0, 00, 0, "23.00")]
        [TestCase(0.0203000, 0, 7, 4, 203000, 203, "0.0203000")]
        [TestCase(123.45, 123, 2, 2, 45, 45, "123.45")]
        [TestCase(1234.567, 1234, 3, 3, 567, 567, "-1234.567")]
        [TestCase(1234.567, 1234, 4, 3, 5670, 567, "-1234.5670")]
        public void TestOperandsFromStr(double n, long i, int v, int w, long f, long t, string input)
        {
            var x = input.TryPluralOperands(out var operands);
            Assert.That(x, $"Parsing operand failed for {input}");
            if (operands != null)
            {
                Assert.That(n, Is.EqualTo(operands.N));
                Assert.That(i, Is.EqualTo(operands.I));
                Assert.That(v, Is.EqualTo(operands.V));
                Assert.That(w, Is.EqualTo(operands.W));
                Assert.That(f, Is.EqualTo(operands.F));
                Assert.That(t, Is.EqualTo(operands.T));
            }
        }
        
        [Test]
        [Parallelizable]
        [SetCulture("de-DE")]
        [TestCase(0, 0, 0, 0, 0, 0, "0")]
        [TestCase(2, 2, 0, 0, 0, 0, "2")]
        [TestCase(57, 57, 0, 0, 0, 0, "57")]
        [TestCase(987, 987, 0, 0, 0, 0, "987")]
        [TestCase(1234567, 1234567, 0, 0, 0, 0, "1234567")]
        [TestCase(10, 10, 0, 0, 0, 0, "-10")]
        [TestCase(1000000, 1000000, 0, 0, 0, 0, "-1000000")]
        [TestCase(0.23, 0, 2, 2, 23, 23, "-0.23")]
        [TestCase(0.230, 0, 3, 2, 230, 23, "0.230")]
        [TestCase(23.00, 23, 2, 0, 00, 0, "23.00")]
        [TestCase(0.0203000, 0, 7, 4, 203000, 203, "0.0203000")]
        [TestCase(123.45, 123, 2, 2, 45, 45, "123.45")]
        [TestCase(1234.567, 1234, 3, 3, 567, 567, "-1234.567")]
        [TestCase(1234.567, 1234, 4, 3, 5670, 567, "-1234.5670")]
        public void TestOperandsFromStrInDifferentCulture(double n, long I, int v, int w, long f, long t, string input)
        {
            var x = input.TryPluralOperands(out var operands);
            Assert.That(x, $"Parsing operand failed for {input}");
            if (operands != null)
            {
                Assert.That(n, Is.EqualTo(operands.N));
                Assert.That(I, Is.EqualTo(operands.I));
                Assert.That(v, Is.EqualTo(operands.V));
                Assert.That(w, Is.EqualTo(operands.W));
                Assert.That(f, Is.EqualTo(operands.F));
                Assert.That(t, Is.EqualTo(operands.T));
            }
        }

        [Test]
        [Parallelizable]
        [TestCase(0, 0, 0, 0, 0, 0, 0)]
        [TestCase(2, 2, 0, 0, 0, 0, 2)]
        [TestCase(57, 57, 0, 0, 0, 0, 57)]
        [TestCase(987, 987, 0, 0, 0, 0, 987)]
        [TestCase(1234567, 1234567, 0, 0, 0, 0, 1234567)]
        [TestCase(10, 10, 0, 0, 0, 0, -10)]
        [TestCase(100000, 100000, 0, 0, 0, 0, -100000)]
        public void TestOperandsFromInt(double n, long i, int v, int w, long f, long t, long input)
        {
            if (input >= SByte.MinValue && input <= SByte.MaxValue)
            {
                sbyte byteInput = Convert.ToSByte(input);
                var x = byteInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            if (input >= Int16.MinValue && input <= Int16.MaxValue)
            {
                short shortInput = Convert.ToInt16(input);
                var x = shortInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            if (input >= Int32.MinValue && input <= Int32.MaxValue)
            {
                int intInput = Convert.ToInt32(input);
                var x = intInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            {
                var r = input.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, r, operands);
            }
        }

        [Test]
        [Parallelizable]
        [TestCase(0, 0u, 0, 0, 0, 0, 0u)]
        [TestCase(2, 2u, 0, 0, 0, 0, 2u)]
        [TestCase(57, 57u, 0, 0, 0, 0, 57u)]
        [TestCase(987, 987u, 0, 0, 0, 0, 987u)]
        [TestCase(1234567, 1234567u, 0, 0, 0, 0, 1234567u)]
        [TestCase(10, 10u, 0, 0, 0, 0, 10u)]
        [TestCase(100000, 100000u, 0, 0, 0, 0, 100000u)]
        [TestCase(10000000000000000000, 10000000000000000000, 0, 0, 0, 0, 10000000000000000000u)]
        public void TestOperandsFromUInt(double n, ulong i, int v, int w, long f, long t, ulong input)
        {
            if (input <= Byte.MaxValue)
            {
                byte byteInput = Convert.ToByte(input);
                var x = byteInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            if (input <= UInt16.MaxValue)
            {
                ushort shortInput = Convert.ToUInt16(input);
                var x = shortInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            if (input <= UInt32.MaxValue)
            {
                uint intInput = Convert.ToUInt32(input);
                var x = intInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            {
                var r = input.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, r, operands);
            }
        }

        [Test]
        [Parallelizable]
        [TestCase(0.23, 0, 2, 2, 23, 23, 0.23)]
        [TestCase(0.230, 0, 2, 2, 23, 23, 0.230)]
        [TestCase(0.0203000, 0, 4, 4, 203, 203, 0.0203000)]
        [TestCase(123.45, 123, 2, 2, 45, 45, 123.45)]
        [TestCase(1234.567, 1234, 3, 3, 567, 567, -1234.567)]
        public void TestOperandsFromFloatingPoint(double n, long i, int v, int w, long f, long t, double input)
        {
            if (input >= float.MinValue && input <= float.MaxValue)
            {
                float floatInput = Convert.ToSingle(input, CultureInfo.InvariantCulture);
                var x = floatInput.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            {
                var x = input.TryPluralOperands(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }
        }
        
        [Test]
        [Parallelizable]
        [TestCase("0.0", 0.0, 0u, 1, 0, 0, 0, 0)]
        [TestCase("-1.0", 1.0, 1u, 1, 0, 0, 0, 0)]
        [TestCase("-20.1", 20.1, 20u, 1, 1, 1, 1, 0)]
        [TestCase("-20.10", 20.1, 20u, 2, 1, 10, 1, 0)]
        [TestCase("-2e2", 200.0, 200u, 0, 0, 0, 0, 2)]
        [TestCase("-2c2", 200.0, 200u, 0, 0, 0, 0, 2)]
        [TestCase("-2c-2", 0.02, 0u, 0, 0, 0, 0, -2)]
        [TestCase("1", 1, 1u, 0, 0, 0, 0, 0)]
        [TestCase("1.0", 1, 1u, 1, 0, 0, 0, 0)]
        [TestCase("1.00", 1, 1u, 2, 0, 0, 0, 0)]
        [TestCase("1.3", 1.3, 1u, 1, 1, 3, 3, 0)]
        [TestCase("1.30", 1.3, 1u, 2, 1, 30, 3, 0)]
        [TestCase("1.03", 1.03, 1u, 2, 2, 3, 3, 0)]
        [TestCase("1.230", 1.23, 1u, 3, 2, 230, 23, 0)]
        [TestCase("1200000", 1200000, 1200000u, 0, 0, 0, 0, 0)]
        [TestCase("1.2e6", 1200000, 1200000u, 0, 0, 0, 0, 6)]
        [TestCase("123c6", 123000000, 123000000u, 0, 0, 0, 0, 6)]
        [TestCase("123c5", 12300000, 12300000u, 0, 0, 0, 0, 5)]
        [TestCase("1200.50", 1200.5, 1200u, 2, 1, 50, 5, 0)]
        [TestCase("1.20050e3", 1200.5, 1200u, 2, 1, 50, 5, 3)]
        public void TestCase(string numberStr, double abs, ulong intDigit, int visibleFracDigit, int withoutZFracDigit, long fractionWithZ, long trailingZRemoved, long exp)
        {
            var number = FluentNumber.FromString(numberStr);

            number.TryPluralOperands(out var actualPluralOperands);
            var expectedPluralOperand = new PluralOperands(abs, intDigit, visibleFracDigit, withoutZFracDigit, fractionWithZ, trailingZRemoved, exp);
            
            
            Assert.That(actualPluralOperands, Is.EqualTo(expectedPluralOperand));
        }

        [Test]
        public void TestIncorrectOperand()
        {
            Assert.That("foo".TryPluralOperands(out _), Is.False);
        }
        
        private static void CheckInput(double n, long i, int v, int w, long f, long t, bool x,
            PluralOperands? operands)
        {
            Assert.That(x);
            if (operands != null)
            {
                Assert.That(n, Is.EqualTo(operands.N));
                Assert.That(i, Is.EqualTo(operands.I));
                Assert.That(v, Is.EqualTo(operands.V));
                Assert.That(w, Is.EqualTo(operands.W));
                Assert.That(f, Is.EqualTo(operands.F));
                Assert.That(t, Is.EqualTo(operands.T));
            }
        }
        
        private static void CheckInput(double n, ulong i, int v, int w, long f, long t, bool x,
            PluralOperands? operands)
        {
            Assert.That(x);
            if (operands != null)
            {
                Assert.That(n, Is.EqualTo(operands.N));
                Assert.That(i, Is.EqualTo(operands.I));
                Assert.That(v, Is.EqualTo(operands.V));
                Assert.That(w, Is.EqualTo(operands.W));
                Assert.That(f, Is.EqualTo(operands.F));
                Assert.That(t, Is.EqualTo(operands.T));
            }
        }
    }
}