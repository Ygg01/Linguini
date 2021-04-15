using System;
using NUnit.Framework;
using PluralRules.Types;

namespace PluralRule.Test.Types
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
        public void TestOperandsFromStr(double n, long I, int v, int w, long f, long t, string input)
        {
            var x = input.TryParse(out var operands);
            Assert.True(x, $"Parsing operand failed for {input}");
            Assert.AreEqual(n, operands!.N);
            Assert.AreEqual(I, operands!.I);
            Assert.AreEqual(v, operands!.V);
            Assert.AreEqual(w, operands!.W);
            Assert.AreEqual(f, operands!.F);
            Assert.AreEqual(t, operands!.T);
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
        public void TestOperandsFromInt(double n, long I, int v, int w, long f, long t, long input)
        {
            if (input >= SByte.MinValue && input <= SByte.MaxValue)
            {
                sbyte byteInput = Convert.ToSByte(input);
                var x = byteInput.TryParse(out var operands);
                CheckInput(n, I, v, w, f, t, x, operands);
            }

            if (input >= Int16.MinValue && input <= Int16.MaxValue)
            {
                short shortInput = Convert.ToInt16(input);
                var x = shortInput.TryParse(out var operands);
                CheckInput(n, I, v, w, f, t, x, operands);
            }

            if (input >= Int32.MinValue && input <= Int32.MaxValue)
            {
                int intInput = Convert.ToInt32(input);
                var x = intInput.TryParse(out var operands);
                CheckInput(n, I, v, w, f, t, x, operands);
            }

            {
                var r = input.TryParse(out var operands);
                CheckInput(n, I, v, w, f, t, r, operands);
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
                var x = byteInput.TryParse(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            if (input <= UInt16.MaxValue)
            {
                ushort shortInput = Convert.ToUInt16(input);
                var x = shortInput.TryParse(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            if (input <= UInt32.MaxValue)
            {
                uint intInput = Convert.ToUInt32(input);
                var x = intInput.TryParse(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            {
                var r = input.TryParse(out var operands);
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
                float floatInput = Convert.ToSingle(input);
                var x = floatInput.TryParse(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }

            {
                var x = input.TryParse(out var operands);
                CheckInput(n, i, v, w, f, t, x, operands);
            }
        }

        [Test]
        public void TestIncorrectOperand()
        {
            Assert.IsFalse("foo".TryParse(out _));
        }
        
        private static void CheckInput(double n, long I, int v, int w, long f, long t, bool x,
            PluralOperands? operands)
        {
            Assert.True(x);
            Assert.AreEqual(n, operands!.N);
            Assert.AreEqual(I, operands!.I);
            Assert.AreEqual(v, operands!.V);
            Assert.AreEqual(w, operands!.W);
            Assert.AreEqual(f, operands!.F);
            Assert.AreEqual(t, operands!.T);
        }
        
        private static void CheckInput(double n, ulong I, int v, int w, long f, long t, bool x,
            PluralOperands? operands)
        {
            Assert.True(x);
            Assert.AreEqual(n, operands!.N);
            Assert.AreEqual(I, operands!.I);
            Assert.AreEqual(v, operands!.V);
            Assert.AreEqual(w, operands!.W);
            Assert.AreEqual(f, operands!.F);
            Assert.AreEqual(t, operands!.T);
        }
    }
}