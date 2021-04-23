using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace PluralRule.CldrParser.Ast
{
    public class Rule
    {
        public Condition Condition;
        public Samples? Samples;

        public Rule(Condition condition, Samples? samples)
        {
            Condition = condition;
            Samples = samples;
        }

        public Rule(Condition condition)
        {
            Condition = condition;
            Samples = null;
        }
    }

    public class Samples
    {
        public List<SampleRange> IntegerSamples;
        public List<SampleRange> DecimalSample;

        public Samples()
        {
            IntegerSamples = new List<SampleRange>();
            DecimalSample = new List<SampleRange>();
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
        public string Value { get; }

        public DecimalValue(string value)
        {
            Value = value;
        }
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

    public class Relation
    {
        public Expr Expr;
        public Operator Op;
        public List<IRangeListItem> RangeListItems;

        public Relation(Expr expr, Operator op,  List<IRangeListItem> rangeList)
        {
            Expr = expr;
            Op = op;
            RangeListItems = rangeList;
        }
    }

    public class Expr
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

    public static class OperandExtension
    {
        public static Operand? FromChar(char c)
        {
            switch (c)
            {
                case 'n':
                    return Operand.N;
                case 'i':
                    return Operand.I;
                case 'f':
                    return Operand.F;
                case 't':
                    return Operand.T;
                case 'v':
                    return Operand.V;
                case 'w':
                    return Operand.W;
                default:
                    return null;
            }
        }
    }


    public interface IRangeListItem
    {
    }

    public class RangeElem : IRangeListItem
    {
        public DecimalValue LowerVal;
        public DecimalValue UpperVal;


        public RangeElem(DecimalValue lowerVal, DecimalValue upperVal)
        {
            LowerVal = lowerVal;
            UpperVal = upperVal;
        }
    }

    public enum Operator : byte
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

    public enum RelationType : byte
    {
        Is,
        In,
        Within,
        Equal
    }

    public static class RelationTypeExtensions
    {
        public static Operator GetOperator(this RelationType rt, bool negated)
        {
            switch (rt)
            {
                case RelationType.Is:
                    return negated ? Operator.IsNot : Operator.Is;
                case RelationType.Within:
                    return negated ? Operator.NotWithin : Operator.Within;
                case RelationType.In:
                    return negated ? Operator.NotIn : Operator.In;
                case RelationType.Equal:
                    return negated ? Operator.NotEqual : Operator.Equal;
                default:
                    throw new ArgumentException("Unknown Operator");
            }
        }
    }
}