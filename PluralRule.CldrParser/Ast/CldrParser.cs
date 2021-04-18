using System;
using System.Collections.Generic;
using Linguini.Syntax.IO;

namespace PluralRule.CldrParser.Ast
{
    public class CldrParser
    {
        private string _input;
        private int _pos;

        public CldrParser(string input)
        {
            _input = input;
            _pos = 0;
        }

        public Rule? TryParse()
        {
            var rule = new Rule();

            rule.Condition = ParseCondition();

            return rule;
        }

        private Condition ParseCondition()
        {
            SkipWhitespace();
            var andConditions = new List<AndCondition>();
            while (ParseAndCondition(andConditions))
            {
            }

            SkipWhitespace();

            return new Condition(andConditions);
        }

        private bool ParseAndCondition(List<AndCondition> conditions)
        {
            if (conditions.Count > 0)
            {
                SkipWhitespace();
                if (!TryConsume("and"))
                {
                    return false;
                }
            }

            SkipWhitespace();
            var orCondition = ParseOrCondition();
            SkipWhitespace();
            return true;
        }


        private bool TryPeekCharSpan(out ReadOnlySpan<char> span)
        {
            return _input.AsMemory().TryReadCharSpan(_pos, out span);
        }

        private bool TryConsume(string consume)
        {
            if (_pos + consume.Length > _input.Length)
            {
                return false;
            }

            var areEqual = Equals(
                _input.AsMemory(new Range(_pos, _pos + consume.Length)),
                consume.AsMemory()
            );

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