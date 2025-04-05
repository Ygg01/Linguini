using System;
using System.Collections.Generic;

namespace PluralRules.Generator.Cldr
{
    
    public struct CldrRule(List<string> langIds, List<RuleMap> ruleList)
    {
        public readonly List<string> LangIds = langIds;
        public readonly List<RuleMap> Rules = ruleList;
    }
    
    public struct RuleMap(string category, Rule rule)
    {
        public readonly string Category = category;
        public Rule Rule = rule;
    }
    public struct Rule(Condition condition, Samples? samples)
    {
        public readonly Condition Condition = condition;
        public Samples? Samples = samples;
    }

    public struct Samples(List<SampleRange> integerSamples, List<SampleRange> decimalSamples)
    {
        public readonly List<SampleRange> IntegerSamples = integerSamples;
        public readonly List<SampleRange> DecimalSamples = decimalSamples;
    }

    public readonly struct SampleRange(DecimalValue lower, DecimalValue? upper)
    {
        public readonly DecimalValue Lower = lower;
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly DecimalValue? Upper = upper;

        public override string ToString()
        {
            return Upper != null ? $"{Lower}~{Upper}" : $"{Lower}";
        }
    }

    public readonly struct DecimalValue(string value) : IRangeListItem, IEquatable<DecimalValue>
    {
        public string Value { get; } = value;

        public bool Equals(DecimalValue other)
        {
            return Value == other.Value;
        }
        

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }

    public struct Condition(List<AndCondition> Conditions)
    {
        public List<AndCondition> Conditions = Conditions;

        public readonly bool IsAny()
        {
            return Conditions.Count == 0;
        }
    }

    public struct AndCondition(List<Relation> relations)
    {
        public readonly List<Relation> Relations = relations;
    }

    public struct Relation(Expr expr, Op op, List<IRangeListItem> rangeList)
    {
        public readonly Expr Expr = expr;
        public readonly Op Op = op;
        public readonly List<IRangeListItem> RangeListItems = rangeList;
    }

    public struct Expr(Operand operand, DecimalValue? modulus = null)
    {
        public Operand Operand = operand;
        public DecimalValue? Modulus = modulus;
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
        T,
        
        /// <summary>
        /// Exponent used in some rules
        /// </summary>
        E,
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
                case 'e':
                    return Operand.E;
                default:
                    return null;
            }
        }

        public static string ToUpperChar(this Operand operand)
        {
            switch (operand)
            {
                case Operand.N:
                    return "N";
                case Operand.I:
                    return "I";
                case Operand.F:
                    return "F";
                case Operand.T:
                    return "T";
                case Operand.V:
                    return "V";
                case Operand.W:
                    return "W";
                case Operand.E:
                    return "E";
                default:
                    throw new ArgumentException($"Unknown operand {operand}"); 
            }
        }
    }


    public interface IRangeListItem
    {
    }

    public struct RangeElem : IRangeListItem
    {
        public DecimalValue LowerVal;
        public DecimalValue UpperVal;


        public RangeElem(DecimalValue lowerVal, DecimalValue upperVal)
        {
            LowerVal = lowerVal;
            UpperVal = upperVal;
        }

        public override string ToString()
        {
            return $"{LowerVal}..{UpperVal}";
        }
    }

    public enum Op : byte
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

    public static class OpHelper
    {
        public static bool IsNegated(this Op op)
        {
            switch (op)
            {
                case Op.NotEqual: case Op.NotWithin: case Op.IsNot: case Op.NotIn:
                    return true;
                default:
                    return false;
            }
        }
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
        public static Op GetOperator(this RelationType? rt, bool negated)
        {
            if (rt == null)
            {
                throw new ArgumentException("Relation should not be null");
            }

            switch (rt)
            {
                case RelationType.Is:
                    return negated ? Op.IsNot : Op.Is;
                case RelationType.Within:
                    return negated ? Op.NotWithin : Op.Within;
                case RelationType.In:
                    return negated ? Op.NotIn : Op.In;
                case RelationType.Equal:
                    return negated ? Op.NotEqual : Op.Equal;
                default:
                    throw new ArgumentException("Unknown Operator");
            }
        }
    }
}
