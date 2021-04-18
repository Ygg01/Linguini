
using System;
using System.Collections.Generic;

namespace PluralRule.CldrParser.Ast
{
    public class Rule
    {
        public Condition Condition;
        public Samples? Samples;
    }

    public class Samples
    {
        public List<SampleRange> IntegerSamples;
        public List<SampleRange> DecimalSample;

        public Samples(List<SampleRange> integerSamples, List<SampleRange> decimalSample)
        {
            IntegerSamples = integerSamples;
            DecimalSample = decimalSample;
        }
    }

    public class SampleRange
    {
        public DecimalValue Lower;
        public DecimalValue? Upper;

        public SampleRange(DecimalValue lower, DecimalValue? upper)
        {
            Lower = lower;
            Upper = upper;
        }
    }

    public class DecimalValue : IRangeListItem
    {
    }

    public record Condition(List<AndCondition> Conditions)
    {
        public bool IsAny()
        {
            return Conditions.Count == 0;
        }
    }

    public record AndCondition(List<Relation> Relations)
    {
    }

    public struct Relation
    {
        private Expression Expression;
        public Operator Operator;
        public List<IRangeListItem> RangeListItems;
    }

    public class Expression
    {
        public Operand Operand;
        public DecimalValue? Modulus;
    }

    public enum Operand
    {
        /// <summary>
        /// Absolute value of input
        /// </summary>
        N,
        /// <summary>
        /// Integer value of input
        /// </summary>
        I,
        /// <summary>
        /// Number of visible fractions digits with trailing zeros
        /// </summary>
        V,
        /// <summary>
        /// Number of visible fraction digits without trailing zeros
        /// </summary>
        W,
        /// <summary>
        /// Visible fraction digits with trailing zeros
        /// </summary>
        F,
        /// <summary>
        /// Visible fraction digits without trailing zeros
        /// </summary>
        T
    }

    public interface IRangeListItem
    {
    }

    public class RangeElem : IRangeListItem
    {
        public DecimalValue LowerVal = new();
        public DecimalValue UpperVal = new();
    }

    public enum Operator: byte
    {
        In,
        NotIn,
        Within,
        NotWithin,
        Is,
        IsNot,
        Equal,
        NotEqual,
    }
}