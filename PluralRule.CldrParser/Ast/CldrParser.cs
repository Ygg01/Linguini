using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Linguini.Syntax.IO;

namespace PluralRule.CldrParser.Ast
{
    public class CldrParser
    {
        private readonly string _input;
        private int _pos;

        public CldrParser(string input)
        {
            _input = input;
            _pos = 0;
        }

        public Rule? ParseRule()
        {
            var condition = ParseCondition();
            TryParseSamples(out Samples samples);

            return new Rule(condition, samples);
        }

        private void TryParseSamples(out Samples samples)
        {
            samples = new Samples();
            SkipWhitespace();
            if (TryConsume("@integer"))
            {
                SkipWhitespace();
                samples.IntegerSamples = TryParseSampleList();
            }

            if (TryConsume("@decimal"))
            {
                SkipWhitespace();
                samples.DecimalSample = TryParseSampleList();
            }
            
        }

        private List<SampleRange> TryParseSampleList()
        {
            var listSample = new List<SampleRange>();

            while (TryParseSampleRange(out var sampleRange))
            {
                SkipWhitespace();
                if (listSample.Count > 0)
                {
                    if (!TryConsume(","))
                    {
                        break;
                    }

                    SkipWhitespace();
                }
                listSample.Add(sampleRange);
            }
            return listSample;
        }

        private bool TryParseSampleRange(out SampleRange o)
        {
            throw new NotImplementedException();
        }

        private Condition ParseCondition()
        {
            var andConditions = new List<AndCondition>();
            SkipWhitespace();
            while (ParseAndCondition(out var andCondition))
            {
                SkipWhitespace();
                if (andConditions.Count > 0)
                {
                    if (!TryConsume("or"))
                    {
                        return new Condition(andConditions);
                    }

                    SkipWhitespace();
                }

                andConditions.Add(andCondition);
            }

            return new Condition(andConditions);
        }

        private bool ParseAndCondition([NotNullWhen(true)] out AndCondition? conditions)
        {
            var relations = new List<Relation>();
            SkipWhitespace();
            while (ParseRelation(out var relation))
            {
                SkipWhitespace();
                if (relations.Count > 0)
                {
                    if (!TryConsume("and"))
                    {
                        conditions = null;
                        return false;
                    }

                    SkipWhitespace();
                }

                relations.Add(relation!);
            }

            conditions = new AndCondition(relations);
            return true;
        }

        private bool ParseRelation([NotNullWhen(true)] out Relation? relation)
        {
            SkipWhitespace();
            if (!TryParseExpr(out var expr))
            {
                relation = null;
                return false;
            }

            var op = Operator.Equal;
            var list = new List<IRangeListItem>();
            RelationType type = RelationType.Equal;
            var negation = false;
            if (TryConsume("is"))
            {
                type = RelationType.Is;
            }

            if (TryConsume("not"))
            {
                negation = true;
                SkipWhitespace();
            }

            if (TryConsume("within"))
            {
                type = RelationType.Within;
            }
            else if (TryConsume("in"))
            {
                type = RelationType.In;
            }
            else if (TryConsume("="))
            {
                type = RelationType.Equal;
            }
            else if (TryConsume("!="))
            {
                negation = !negation;
                type = RelationType.Equal;
            }
            SkipWhitespace();

            if (type == RelationType.Is)
            {
                if(!TryParseValue(out var x ))
                {
                    relation = null;
                    return false;
                }
                list.Add(x);
            }
            else
            {
                if (!TryParseRangeList(out list))
                {
                    relation = null;
                    return false;
                }
            }

            relation = new Relation(expr, type.GetOperator(negation), list);
            return true;
        }

        private bool TryParseRangeList(out List<IRangeListItem> list)
        {
            list = new List<IRangeListItem>();
            while (TryParseRangeItem(out var x))
            {
                SkipWhitespace();
                if (list.Count > 1)
                {
                    if (!TryConsume(","))
                    {
                        return false;
                    }
                    SkipWhitespace();
                }
                list.Add(x);
            }

            return true;
        }

        private bool TryParseRangeItem([NotNullWhen(true)] out IRangeListItem? item)
        {
            SkipWhitespace();
            if (!TryParseValue(out var start))
            {
                item = null;
                return false;
            }
            SkipWhitespace();
            if (TryConsume(".."))
            {
                SkipWhitespace();
                if (!TryParseValue(out var end))
                {
                    item = null;
                    return false;
                }

                item = new RangeElem(start, end);
                return true;
            }

            item = start;
            return true;
        }

        private bool TryParseExpr([NotNullWhen(true)] out Expr? expr)
        {
            SkipWhitespace();
            if (TryOperand(out var operand))
            {
                _pos += 1;
                expr = new Expr {Operand = operand.Value};

                SkipWhitespace();

                var modulus = ParseModulus();
                expr.Modulus = modulus;
                return true;
            }

            expr = null;
            return false;
        }

        private DecimalValue? ParseModulus()
        {
            if (TryConsume("mod") || TryConsume("%"))
            {
                SkipWhitespace();
                if (TryParseValue(out var val))
                {
                    return val;
                }
            }

            return null;
        }

        private bool TryParseValue(out DecimalValue val)
        {
            var startPos = _pos;
            while (TryPeekCharSpan(out var span)
                   && span.IsAsciiDigit())
            {
                _pos += 1;
            }

            val = new DecimalValue(_input[new Range(startPos, _pos)]);
            return startPos != _pos;
        }


        private bool TryPeekCharSpan(out ReadOnlySpan<char> span)
        {
            return _input.AsMemory().TryReadCharSpan(_pos, out span);
        }

        private bool TryOperand([NotNullWhen(true)] out Operand? operand)
        {
            if (_pos < _input.Length)
            {
                var chr = _input[_pos];
                operand = OperandExtension.FromChar(chr);
                return operand != null;
            }

            operand = null;
            return false;
        }

        private bool TryConsume(string consume)
        {
            if (_pos + consume.Length > _input.Length)
            {
                return false;
            }

            var span = _input.AsMemory(_pos, consume.Length).Span;
            var areEqual = span.Equals(consume, StringComparison.InvariantCulture);

            if (areEqual)
            {
                _pos += consume.Length;
            }

            return areEqual;
        }

        private void SkipWhitespace()
        {
            while (TryPeekCharSpan(out var span)
                   && span.IsUnicodeWhiteSpace())
            {
                _pos += 1;
            }
        }
    }
}