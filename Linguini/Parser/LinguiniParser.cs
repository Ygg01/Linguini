#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Linguini.Ast;
using Linguini.IO;
using Attribute = Linguini.Ast.Attribute;

#pragma warning disable 8600

namespace Linguini.Parser
{
    public class LinguiniParser
    {
        private readonly ZeroCopyReader _reader;

        public LinguiniParser(string input)
        {
            _reader = new ZeroCopyReader(input);
        }

        public LinguiniParser(TextReader input)
        {
            // TODO add buffering
            _reader = new ZeroCopyReader(input.ReadToEnd());
        }

        public Resource Parse()
        {
            var body = new List<IEntry>();
            var errors = new List<ParseError>();
            _reader.SkipBlankBlock();

            Comment? lastComment = null;
            var lastBlankCount = 0;

            while (_reader.IsNotEof)
            {
                var entryStart = _reader.Position;
                (IEntry entry, ParseError? error) = GetEntry(entryStart);

                if (lastComment != null)
                {
                    if (entry.TryConvert<IEntry, Message>(out var message)
                        && lastBlankCount < 2)
                    {
                        message.Comment = lastComment;
                    }
                    else if (entry.TryConvert<IEntry, Term>(out var term)
                             && lastBlankCount < 2)
                    {
                        term.Comment = lastComment;
                    }
                    else
                    {
                        body.Add(lastComment);
                    }

                    lastComment = null;
                }

                if (error != null)
                {
                    _reader.SkipToNextEntry();
                    error.Slice = new Range(entryStart, _reader.Position);
                    errors.Add(error);
                    Junk junk = (Junk) entry;
                    junk.Content = _reader.ReadSlice(entryStart, _reader.Position);
                    body.Add(entry);
                }
                else if (entry.TryConvert<IEntry, Comment>(out var comment)
                         && comment.CommentLevel == CommentLevel.Comment)
                {
                    lastComment = comment;
                }
                else
                {
                    body.Add(entry);
                }

                lastBlankCount = _reader.SkipBlankBlock();
            }

            if (lastComment != null)
            {
                body.Add(lastComment);
            }


            return new Resource(body, errors);
        }

        private (IEntry, ParseError?) GetEntry(int entryStart)
        {
            var charSpan = _reader.PeekCharSpan();

            if ('#'.EqualsSpans(charSpan))
            {
                IEntry entry = new Junk();
                if (TryGetComment(out var comment, out var error))
                {
                    entry = comment;
                }

                return (entry, error);
            }

            if ('-'.EqualsSpans(charSpan))
            {
                return GetTerm(entryStart);
            }

            return GetMessage(entryStart);
        }

        private (IEntry, ParseError?) GetTerm(int entryStart)
        {
            IEntry entry = new Junk();
            if (!TryExpectChar('-', out var error))
            {
                return (entry, error);
            }

            if (!TryGetIdentifier(out var id, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();
            if (!TryExpectChar('=', out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();

            if (!TryGetPattern(out var value, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankBlock();

            var attribute = GetAttributes();

            if (value != null)
            {
                entry = new Term(id, value, attribute, null);
                return (entry, error);
            }
            else
            {
                error = ParseError.ExpectedTermField(id, entryStart, _reader.Position);
                return (entry, error);
            }
        }

        private (IEntry, ParseError?) GetMessage(int entryStart)
        {
            IEntry entry = new Junk();
            if (!TryGetIdentifier(out var id, out var error))
            {
                return (entry, error);
            }

            _reader.SkipBlankInline();
            if (!TryExpectChar('=', out error))
            {
                return (entry, error);
            }


            if (!TryGetPattern(out var pattern, out error))
            {
                return (entry, error);
            }

            _reader.SkipBlankBlock();
            var attrs = GetAttributes();

            if (pattern == null && attrs.Count < 1)
            {
                return (entry, ParseError.ExpectedMessageField(id.Name, entryStart, _reader.Position));
            }

            return (new Message(id, pattern, attrs, null), null);
        }


        #region CommentSyntax

        private bool TryGetComment([NotNullWhen(true)] out Comment? comment, out ParseError? error)
        {
            var level = CommentLevel.None;
            var content = new List<ReadOnlyMemory<char>>();

            while (_reader.IsNotEof)
            {
                var lineLevel = GetCommentLevel();
                if (lineLevel == CommentLevel.None)
                {
                    _reader.Position -= 1;
                    break;
                }

                if (level != CommentLevel.None && lineLevel != level)
                {
                    _reader.Position -= (int) lineLevel;
                    break;
                }

                level = lineLevel;

                if (_reader.IsEof)
                {
                    break;
                }

                if ('\n'.EqualsSpans(_reader.PeekCharSpan())
                    || ('\r'.EqualsSpans(_reader.PeekCharSpan()) && '\n'.EqualsSpans(_reader.PeekCharSpan(1))))
                {
                    content.Add(_reader.GetCommentLine());
                }
                else
                {
                    if (!TryExpectChar(' ', out var e))
                    {
                        if (content.Count == 0)
                        {
                            error = e;
                            comment = default!;
                            return false;
                        }

                        _reader.Position -= (int) lineLevel;
                        break;
                    }

                    content.Add(_reader.GetCommentLine());
                }

                _reader.SkipEol();
            }

            comment = new Comment(level, content);
            error = null;
            return true;
        }


        private CommentLevel GetCommentLevel()
        {
            if (_reader.ReadCharIf('#'))
            {
                if (_reader.ReadCharIf('#'))
                {
                    if (_reader.ReadCharIf('#'))
                    {
                        return CommentLevel.ResourceComment;
                    }

                    return CommentLevel.GroupComment;
                }

                return CommentLevel.Comment;
            }

            return CommentLevel.None;
        }

        #endregion

        #region CommonSyntax

        private bool TryExpectChar(char c, out ParseError? error)
        {
            if (_reader.ReadCharIf(c))
            {
                error = null;
                return true;
            }

            error = ParseError.ExpectedToken(c, _reader.Position);
            return false;
        }

        private bool TryGetIdentifier([NotNullWhen(true)] out Identifier? id, out ParseError? error)
        {
            if (_reader.PeekCharSpan().IsAsciiAlphabetic())
            {
                _reader.Position += 1;
                error = null;
                id = GetUncheckedIdentifier();
                return true;
            }

            error = ParseError.ExpectedCharRange("a-zA-Z", _reader.Position);
            id = default!;
            return false;
        }

        private Identifier GetUncheckedIdentifier()
        {
            // First character is already checked
            var ptr = _reader.Position;
            while (_reader.PeekCharSpanAt(ptr).IsIdentifier())
            {
                ptr += 1;
            }

            Identifier id = new(_reader.ReadSlice(_reader.Position - 1, ptr));
            _reader.Position = ptr;
            return id;
        }

        private bool TryGetPattern(out Pattern? pattern, out ParseError? error)
        {
            var elements = new List<IPatternElementPlaceholder>();
            TextElementPosition textElementRole;
            int? lastNonBlank = null;
            int? commonIndent = null;

            _reader.SkipBlankInline();

            if (_reader.SkipEol())
            {
                _reader.SkipBlankBlock();
                textElementRole = TextElementPosition.LineStart;
            }
            else
            {
                textElementRole = TextElementPosition.InitialLineStart;
            }

            while (_reader.IsNotEof)
            {
                if (_reader.ReadCharIf('{'))
                {
                    if (textElementRole == TextElementPosition.LineStart)
                    {
                        commonIndent = 0;
                    }

                    if (!TryGetPlaceable(out var exp, out error))
                    {
                        pattern = null;
                        return false;
                    }

                    lastNonBlank = elements.Count;
                    elements.Add(new Placeable(exp));
                    textElementRole = TextElementPosition.Continuation;
                }
                else
                {
                    var sliceStart = _reader.Position;
                    var indent = 0;
                    if (textElementRole == TextElementPosition.LineStart)
                    {
                        indent = _reader.SkipBlankInline();
                        if (_reader.TryPeekCharSpan(out var b))
                        {
                            if (indent == 0)
                            {
                                if (!'\r'.EqualsSpans(b) && !'\n'.EqualsSpans(b))
                                {
                                    break;
                                }
                            }
                            else if (b.IsOneOf('.', '}', '[', '*'))
                            {
                                _reader.Position = sliceStart;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!TryGetTextSlice(out var text, out error))
                    {
                        pattern = null;
                        return false;
                    }

                    if (text.Start != text.End)
                    {
                        if (textElementRole == TextElementPosition.LineStart
                            && text.ElementType == TextElementType.NonBlank)
                        {
                            if (commonIndent != null)
                            {
                                if (indent < commonIndent)
                                {
                                    commonIndent = indent;
                                }
                            }
                            else
                            {
                                commonIndent = indent;
                            }
                        }

                        if (textElementRole != TextElementPosition.LineStart
                            || text.ElementType == TextElementType.NonBlank
                            || text.TerminationReason == TextElementTermination.LF)
                        {
                            if (text.ElementType == TextElementType.NonBlank)
                            {
                                lastNonBlank = elements.Count;
                            }

                            elements.Add(new TextElementPlaceholder(
                                sliceStart,
                                text.End,
                                indent,
                                textElementRole
                            ));
                        }
                    }

                    textElementRole = text.TerminationReason switch
                    {
                        TextElementTermination.LF => TextElementPosition.LineStart,
                        TextElementTermination.CRLF => TextElementPosition.LineStart,
                        TextElementTermination.PlaceableStart => TextElementPosition.Continuation,
                        TextElementTermination.EndOfFile => TextElementPosition.Continuation,
                        _ => textElementRole
                    };
                }
            }

            if (lastNonBlank != null)
            {
                List<IPatternElement> patterns = new(lastNonBlank.Value + 1);

                for (int i = 0; i < lastNonBlank + 1; i++)
                {
                    IPatternElementPlaceholder? elem = null;
                    if (i < elements.Count)
                    {
                        elem = elements[i];
                        if (elem.TryConvert(out Placeable placeable))
                        {
                            patterns.Add(placeable);
                        }
                        else if (elem.TryConvert(out TextElementPlaceholder textLiteral))
                        {
                            int start = textLiteral.Start;
                            int indent = textLiteral.Indent;
                            var end = textLiteral.End;
                            if (textLiteral.Role == TextElementPosition.LineStart)
                            {
                                if (commonIndent == null)
                                {
                                    start = start + indent;
                                }
                                else
                                {
                                    start = start + Math.Min(indent, commonIndent.Value);
                                }
                            }

                            var value = _reader.ReadSlice(start, end);
                            if (lastNonBlank == i)
                            {
                                value = value.TrimEnd();
                            }

                            patterns.Add(new TextLiteral(value));
                        }
                    }
                }

                pattern = new Pattern(patterns);
                error = null;
                return true;
            }

            // We successfully found nothing
            pattern = null;
            error = null;
            return true;
        }

        private bool TryGetTextSlice([NotNullWhen(true)] out TextSlice? textElement, out ParseError? error)
        {
            var startPos = _reader.Position;
            var textElementType = TextElementType.Blank;

            while (_reader.TryPeekCharSpan(out var span))
            {
                if (' '.EqualsSpans(span))
                {
                    _reader.Position += 1;
                }
                else if ('\n'.EqualsSpans(span))
                {
                    _reader.Position += 1;
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.LF
                    );
                    error = null;
                    return true;
                }
                else if ('\r'.EqualsSpans(span)
                         && '\n'.EqualsSpans(_reader.PeekCharSpan(1)))
                {
                    _reader.Position += 1;
                    // This takes one less element because it converts CRLF endings 
                    // to LF endings
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position - 1,
                        textElementType,
                        TextElementTermination.CRLF
                    );
                    error = null;
                    return true;
                }
                else if ('{'.EqualsSpans(span))
                {
                    textElement = new TextSlice(
                        startPos,
                        _reader.Position,
                        textElementType,
                        TextElementTermination.PlaceableStart
                    );
                    error = null;
                    return true;
                }
                else if ('}'.EqualsSpans(span))
                {
                    textElement = null;
                    error = ParseError.UnbalancedClosingBrace(_reader.Position);
                    return false;
                }
                else
                {
                    textElementType = TextElementType.NonBlank;
                    _reader.Position += 1;
                }
            }

            textElement = new TextSlice(
                startPos,
                _reader.Position,
                textElementType,
                TextElementTermination.EndOfFile
            );
            error = null;
            return true;
        }

        #endregion

        #region AttributeSyntax

        private List<Attribute> GetAttributes()
        {
            var attributes = new List<Attribute>();

            while (true)
            {
                var lineStart = _reader.Position;
                _reader.SkipBlankInline();
                if (!_reader.ReadCharIf('.'))
                {
                    _reader.Position = lineStart;
                    break;
                }

                if (TryGetAttribute(out var attr, out _))
                {
                    attributes.Add(attr.Value);
                }
                else
                {
                    _reader.Position = lineStart;
                    break;
                }
            }

            return attributes;
        }

        private bool TryGetAttribute([NotNullWhen(true)] out Attribute? attr, out ParseError? error)
        {
            if (!TryGetIdentifier(out var id, out var err))
            {
                error = err;
                attr = default!;
                return false;
            }

            _reader.SkipBlankInline();

            if (!TryExpectChar('=', out err))
            {
                error = err;
                attr = default!;
                return false;
            }

            if (TryGetPattern(out var pattern, out err) && pattern != null)
            {
                error = null;
                attr = new Attribute(id, pattern);
                return true;
            }

            error = ParseError.MissingValue(_reader.Position);
            attr = default!;
            return false;
        }

        #endregion

        #region ExpressionSyntax

        private bool TryGetPlaceable([NotNullWhen(true)] out IExpression? expr, out ParseError? error)
        {
            _reader.SkipBlank();
            if (!TryGetExpression(out expr, out error))
            {
                return false;
            }

            _reader.SkipBlankInline();
            if (!TryExpectChar('}', out error))
            {
                expr = null;
                return false;
            }

            if (expr.TryConvert(out TermReference termReference) && termReference.Attribute != null)
            {
                expr = null;
                error = ParseError.TermAttributeAsPlaceable(_reader.Position);
                return false;
            }

            return true;
        }

        private bool TryGetExpression([NotNullWhen(true)] out IExpression? retVal, out ParseError? error)
        {
            if (!TryGetInlineExpression(false, out var inlineExpression, out error))
            {
                retVal = inlineExpression;
                return false;
            }

            _reader.SkipBlank();

            if (!'-'.EqualsSpans(_reader.PeekCharSpan())
                || !'>'.EqualsSpans(_reader.PeekCharSpan(1)))
            {
                if (inlineExpression.TryConvert(out TermReference termReference)
                    && termReference.Attribute != null)
                {
                    error = ParseError.TermAttributeAsPlaceable(_reader.Position);
                    retVal = null;
                    return false;
                }

                retVal = inlineExpression;
                return true;
            }

            if (inlineExpression.TryConvert(out MessageReference msgRef))
            {
                if (msgRef.Attribute == null)
                {
                    error = ParseError.MessageReferenceAsSelector(_reader.Position);
                    retVal = null;
                    return false;
                }

                error = ParseError.MessageAttributeAsSelector(_reader.Position);
                retVal = null;
                return false;
            }

            if (!inlineExpression.TryConvert(out TermReference termRef))
            {
                if (!inlineExpression.TryConvert<IInlineExpression, TextLiteral>(out _) &&
                    !inlineExpression.TryConvert<IInlineExpression, NumberLiteral>(out _) &&
                    !inlineExpression.TryConvert<IInlineExpression, VariableReference>(out _) &&
                    !inlineExpression.TryConvert<IInlineExpression, FunctionReference>(out _))
                {
                    retVal = null;
                    error = ParseError.ExpectedSimpleExpressionAsSelector(_reader.Position);
                    return false;
                }
            }
            else
            {
                if (termRef.Attribute == null)
                {
                    retVal = null;
                    error = ParseError.TermReferenceAsSelector(_reader.Position);
                    return false;
                }
            }

            // We found `->`
            _reader.Position += 2;

            _reader.SkipBlankInline();
            if (!_reader.SkipEol())
            {
                error = ParseError.ExpectedCharRange(@"\n | \r\n", _reader.Position);
                retVal = null;
                return false;
            }

            _reader.SkipBlank();

            if (!TryGetVariants(out List<Variant> variants, out error))
            {
                retVal = null;
                return false;
            }

            retVal = new SelectExpression(inlineExpression, variants);
            error = null;
            return true;
        }

        private bool TryGetVariants(out List<Variant> variants, out ParseError? error)
        {
            variants = new List<Variant>();
            var hasDefault = false;

            while (true)
            {
                var isDefault = _reader.ReadCharIf('*');
                if (isDefault)
                {
                    if (hasDefault)
                    {
                        error = ParseError.MultipleDefaultVariants(_reader.Position);
                        return true;
                    }

                    hasDefault = true;
                }

                if (!_reader.ReadCharIf('['))
                {
                    break;
                }

                if (!TryGetVariantKey(out var variant, out error))
                {
                    return false;
                }

                if (!TryGetPattern(out var value, out error))
                {
                    return false;
                }


                if (value != null)
                {
                    variant.Value = value;
                    variant.IsDefault = hasDefault;
                    variants.Add(variant);
                    _reader.SkipBlank();
                }
                else
                {
                    error = ParseError.MissingValue(_reader.Position);
                    return false;
                }
            }

            if (hasDefault)
            {
                error = null;
                return true;
            }

            error = ParseError.MissingDefaultVariant(_reader.Position);
            return false;
        }

        private bool TryGetVariantKey([NotNullWhen(true)] out Variant? variantKey, out ParseError? error)
        {
            _reader.SkipBlank();
            VariantType variantType;


            if (_reader.PeekCharSpan().IsNumberStart())
            {
                variantType = VariantType.NumberLiteral;
                if (!TryGetNumberLiteral(out var num, out error))
                {
                    variantKey = null;
                    return false;
                }

                variantKey = new Variant(variantType, num);
            }
            else
            {
                variantType = VariantType.Identifier;
                if (!TryGetIdentifier(out var id, out error))
                {
                    variantKey = null;
                    return false;
                }

                variantKey = new Variant(variantType, id.Name);
            }

            _reader.SkipBlank();
            if (!TryExpectChar(']', out error))
            {
                return false;
            }


            return true;
        }

        private bool TryGetInlineExpression(bool onlyLiteral, [NotNullWhen(true)] out IInlineExpression? expr,
            out ParseError? error)
        {
            var peekChr = _reader.PeekCharSpan();
            if ('"'.EqualsSpans(peekChr))
            {
                _reader.Position += 1;
                var start = _reader.Position;
                while (true)
                {
                    var b = _reader.PeekCharSpan();
                    if ('\\'.EqualsSpans(b))
                    {
                        var c = _reader.PeekCharSpan(1);
                        if (c.IsOneOf('\\', '{', '"'))
                        {
                            _reader.Position += 2;
                        }
                        else if ('u'.EqualsSpans(c))
                        {
                            _reader.Position += 2;
                            if (!TrySkipUnicodeSequence(4, out error))
                            {
                                expr = null;
                                return false;
                            }
                        }
                        else if ('U'.EqualsSpans(c))
                        {
                            _reader.Position += 2;
                            if (!TrySkipUnicodeSequence(6, out error))
                            {
                                expr = null;
                                return false;
                            }
                        }
                        else
                        {
                            var seq = c == null ? ' ' : c[0];
                            error = ParseError.UnknownEscapeSequence(seq, _reader.Position);
                            expr = null;
                            return false;
                        }
                    }
                    else if ('"'.EqualsSpans(b))
                    {
                        break;
                    }
                    else if ('\n'.EqualsSpans(b))
                    {
                        error = ParseError.UnterminatedStringLiteral(_reader.Position);
                        expr = null;
                        return false;
                    }
                    else
                    {
                        _reader.Position += 1;
                    }
                }

                if (!TryExpectChar('"', out error))
                {
                    expr = null;
                    return false;
                }

                var slice = _reader.ReadSlice(start, _reader.Position - 1);
                expr = new TextLiteral(slice);
                return true;
            }

            if (peekChr.IsAsciiDigit())
            {
                if (!TryGetNumberLiteral(out var num, out error))
                {
                    expr = null;
                    return false;
                }

                expr = new NumberLiteral(num);
                return true;
            }

            if ('-'.EqualsSpans(peekChr) && !onlyLiteral)
            {
                _reader.Position += 1;
                if (_reader.PeekCharSpan().IsAsciiAlphabetic())
                {
                    _reader.Position += 1;
                    var id = GetUncheckedIdentifier();
                    if (!TryGetAttributeAccessor(out var attribute, out error))
                    {
                        expr = null;
                        return false;
                    }

                    if (!TryCallArguments(out var args, out error))
                    {
                        expr = null;
                        return false;
                    }

                    expr = new TermReference(id, attribute, args);
                    return true;
                }
                else
                {
                    _reader.Position -= 1;
                    if (TryGetNumberLiteral(out var num, out error))
                    {
                        expr = new NumberLiteral(num);
                        return true;
                    }

                    expr = null;
                    return false;
                }
            }
            else if ('$'.EqualsSpans(peekChr) && !onlyLiteral)
            {
                _reader.Position += 1;
                if (!TryGetIdentifier(out var id, out error))
                {
                    expr = null;
                    return false;
                }

                expr = new VariableReference(id);
                return true;
            }
            else if (peekChr.IsAsciiAlphabetic())
            {
                _reader.Position += 1;
                var id = GetUncheckedIdentifier();

                if (!TryCallArguments(out var args, out error))
                {
                    expr = null;
                    return false;
                }

                if (args != null)
                {
                    if (!id.Name.IsCallee())
                    {
                        error = ParseError.ForbiddenCallee(_reader.Position);
                        expr = null;
                        return false;
                    }

                    error = null;
                    expr = new FunctionReference(id, args.Value);
                    return true;
                }
                else
                {
                    if (!TryGetAttributeAccessor(out var attribute, out error))
                    {
                        expr = null;
                        return false;
                    }

                    expr = new MessageReference(id, attribute);
                    return true;
                }
            }
            else if ('{'.EqualsSpans(peekChr) && !onlyLiteral)
            {
                _reader.Position += 1;
                if (!TryGetPlaceable(out var exp, out error))
                {
                    expr = null;
                    return false;
                }

                error = null;
                expr = new Placeable(exp);
                return true;
            }

            if (onlyLiteral)
            {
                error = ParseError.ExpectedLiteral(_reader.Position);
                expr = null;
                return false;
            }

            error = ParseError.ExpectedInlineExpression(_reader.Position);
            expr = null;
            return false;
        }

        private bool TryCallArguments(out CallArguments? args, out ParseError? error)
        {
            _reader.SkipBlank();
            if (!_reader.ReadCharIf('('))
            {
                args = null;
                error = null;
                return true;
            }

            var positional = new List<IInlineExpression>();
            var nameArgs = new List<NamedArgument>();
            var argNames = new List<Identifier>();

            _reader.SkipBlank();

            while (_reader.IsNotEof)
            {
                if (IsCurrentByte(')'))
                {
                    break;
                }

                if (!TryGetInlineExpression(false, out var expr, out error))
                {
                    args = null;
                    return false;
                }

                if (expr.TryConvert(out MessageReference msgRef)
                    && msgRef.Attribute == null)
                {
                    var id = msgRef.Id;
                    _reader.SkipBlank();
                    if (IsCurrentByte(':'))
                    {
                        if (argNames.Contains(id))
                        {
                            args = null;
                            error = ParseError.DuplicatedNamedArgument(id, _reader.Position);
                            return false;
                        }

                        _reader.Position += 1;
                        _reader.SkipBlank();

                        if (!TryGetInlineExpression(true, out var val, out error))
                        {
                            args = null;
                            return false;
                        }

                        argNames.Add(id);
                        nameArgs.Add(new NamedArgument(id, val));
                    }
                    else
                    {
                        if (argNames.Count > 0)
                        {
                            args = null;
                            error = ParseError.PositionalArgumentFollowsNamed(_reader.Position);
                            return false;
                        }

                        positional.Add(expr);
                    }
                }
                else
                {
                    if (argNames.Count > 0)
                    {
                        args = null;
                        error = ParseError.PositionalArgumentFollowsNamed(_reader.Position);
                        return false;
                    }
                }

                _reader.SkipBlank();
                _reader.ReadCharIf(',');
                _reader.SkipBlank();
            }

            if (!TryExpectChar(')', out error))
            {
                args = null;
                return false;
            }

            args = new CallArguments(positional, nameArgs);
            return true;
        }

        private bool IsCurrentByte(char c)
        {
            return c.EqualsSpans(_reader.PeekCharSpan());
        }

        private bool TryGetAttributeAccessor(out Identifier? id, out ParseError? error)
        {
            if (_reader.ReadCharIf('.'))
            {
                if (!TryGetIdentifier(out id, out error))
                {
                    return false;
                }

                return true;
            }

            id = null;
            error = null;
            return true;
        }

        private bool TryGetNumberLiteral([NotNullWhen(true)] out ReadOnlyMemory<char> num, out ParseError? error)
        {
            var start = _reader.Position;
            _reader.ReadCharIf('-');
            if (!TrySkipDigits(out error))
            {
                num = null;
                return false;
            }

            if (_reader.ReadCharIf('.') && !TrySkipDigits(out error))
            {
                num = null;
                return false;
            }

            num = _reader.ReadSlice(start, _reader.Position);
            error = null;
            return true;
        }

        private bool TrySkipDigits(out ParseError? error)
        {
            var start = _reader.Position;
            while (_reader.PeekCharSpan().IsAsciiDigit())
            {
                _reader.Position += 1;
            }

            if (start == _reader.Position)
            {
                error = ParseError.ExpectedCharRange("0-9", _reader.Position);
                return false;
            }

            error = null;
            return true;
        }

        private bool TrySkipUnicodeSequence(int length, out ParseError? error)
        {
            var start = _reader.Position;
            for (int i = 0; i < length; i++)
            {
                if (_reader.PeekCharSpan().IsAsciiHexdigit())
                {
                    _reader.Position += 1;
                }
                else
                {
                    break;
                }
            }

            if (_reader.Position - start != length)
            {
                var end = _reader.IsEof ? _reader.Position : _reader.Position + 1;
                var seq = _reader.ReadSliceToStr(start, end);
                error = ParseError.InvalidUnicodeEscapeSequence(seq, _reader.Position);
                return false;
            }

            error = null;
            return true;
        }

        #endregion
    }

    public class TextSlice
    {
        public int Start { get; }
        public int End { get; }
        public TextElementType ElementType { get; }
        public TextElementTermination TerminationReason { get; }

        public TextSlice(int start, int end, TextElementType elementType, TextElementTermination terminationReason)
        {
            Start = start;
            End = end;
            ElementType = elementType;
            TerminationReason = terminationReason;
        }
    }
}
