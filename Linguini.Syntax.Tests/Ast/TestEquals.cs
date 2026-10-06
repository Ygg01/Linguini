using System.Collections.Generic;
using Linguini.Syntax.Ast;
using Linguini.Syntax.IO;
using NUnit.Framework;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable once EqualExpressionComparison

namespace Linguini.Syntax.Tests.Ast
{
    [TestFixture]
    [Parallelizable]
    public class TestEquals
    {
        private static IEnumerable<TestCaseData> AstCommentExample()
        {
            var same = new AstComment(CommentLevel.Comment, "comment");
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new AstComment(CommentLevel.GroupComment, "comment"), false);
            yield return new TestCaseData(same, new AstComment(CommentLevel.Comment, "commentzzzz"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstCommentExample))]
        public void TestAstComment(AstComment comment, object? other, bool expected)
        {
            switch (other)
            {
                case AstComment otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    Assert.That(comment.GetId() == otherComment.GetId());
                    break;
                case null:
                    Assert.That(comment.Equals((AstComment?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AstLocData()
        {
            var same = AstLocation.FromRowAndFilename(3, "text.ftl");
            var fromReader = AstLocation.FromReader(new ZeroCopyReader("aaaa"));
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, AstLocation.FromRowAndFilename(5, "text.ftl"), false);
            yield return new TestCaseData(same, AstLocation.FromRowAndFilename(3, "testing.ftl"), false);
            yield return new TestCaseData(same, fromReader, false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstLocData))]
        public void TestLocation(AstLocation comment, object? other, bool expected)
        {
            switch (other)
            {
                case AstLocation otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((AstLocation?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AstTextData()
        {
            var same = new TextLiteral("ex");
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new TextLiteral("heh"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstTextData))]
        public void TestTextLiteral(TextLiteral comment, object? other, bool expected)
        {
            switch (other)
            {
                case TextLiteral otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((TextLiteral?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AstNumberData()
        {
            var same = new NumberLiteral(1);
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new NumberLiteral(3.2), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstNumberData))]
        public void TestNumberData(NumberLiteral comment, object? other, bool expected)
        {
            switch (other)
            {
                case NumberLiteral otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((NumberLiteral?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }


        private static IEnumerable<TestCaseData> IdentifierTestData()
        {
            Identifier same = "aaa";
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new Identifier("bbb"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(IdentifierTestData))]
        public void TestIdentifier(Identifier comment, object? other, bool expected)
        {
            switch (other)
            {
                case Identifier otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((Identifier?)other), Is.EqualTo(expected));
                    Assert.That(Identifier.Comparer.Equals(comment, null), Is.False);
                    Assert.That(Identifier.Comparer.Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> AttributeTestData()
        {
            var same = new Attribute("zzz", Pattern.Builder().AddText("top"));
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same,
                new Attribute("xyz", Pattern.From("top")), false);
            yield return new TestCaseData(same,
                new Attribute("zzz", Pattern.Builder().AddText("xxxx")), false);
            yield return new TestCaseData(same, new
                Attribute("x1", Pattern.Builder().AddDynamicReference("dyn-rfe")), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AttributeTestData))]
        public void TestAttribute(Attribute comment, object? other, bool expected)
        {
            switch (other)
            {
                case Attribute otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((Attribute?)other), Is.EqualTo(expected));
                    Assert.That(Attribute.Comparer.Equals(comment, null), Is.False);
                    Assert.That(Attribute.Comparer.Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> PatternTestData()
        {
            var same = Pattern.Builder()
                .AddText("top")
                .AddDynamicReference("dyn-ref")
                .Build();
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, Pattern.From("top"), false);
            yield return new TestCaseData(same, Pattern.From(30), false);
            yield return new TestCaseData(same, Pattern.From(3.1), false);
            yield return new TestCaseData(same, Pattern.Builder()
                    .AddText("top")
                    .AddDynamicReference("dyn-ref").Build(),
                true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(PatternTestData))]
        public void TestPattern(Pattern comment, object? other, bool expected)
        {
            switch (other)
            {
                case Pattern otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((Pattern?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> NamedArgumentTestData()
        {
            var same = new NamedArgument("id",
                InlineExpressionBuilder.CreateDynamicReference("aa", "zz").Build()
            );
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same,
                new NamedArgument("id", InlineExpressionBuilder.CreateNumber(32.0).Build()), false);
            yield return new TestCaseData(same,
                new NamedArgument("wrong-id", InlineExpressionBuilder.CreateDynamicReference("aa", "zz").Build()),
                false);
            yield return new TestCaseData(same,
                new NamedArgument("id", InlineExpressionBuilder.CreateDynamicReference("aa", "zz").Build()),
                true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(NamedArgumentTestData))]
        public void TestPattern(NamedArgument comment, object? other, bool expected)
        {
            switch (other)
            {
                case NamedArgument otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((NamedArgument?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> CallArgumentTestData()
        {
            var builder = CallArguments.Builder()
                .AddNamedArg("dyn-ref", InlineExpressionBuilder.CreateDynamicReference("dyn", "ref"))
                .AddPositionalArg(InlineExpressionBuilder.CreateMessageReference("msg", "atr"))
                .AddNamedArg("name-tref", InlineExpressionBuilder.CreateTermReference("term").Build())
                .AddNamedArg("name-fl", 3.0f)
                .AddNamedArg("name-str", "name")
                .AddNamedArg("name-dbl", 2.0d);

            var same = builder.Build();

            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, builder.Build(), true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(CallArgumentTestData))]
        public void TestCallArguments(CallArguments comment, object? other, bool expected)
        {
            switch (other)
            {
                case CallArguments otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((CallArguments?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> TermRefTestData()
        {
            var callArgs = CallArguments.Builder()
                .AddPositionalArg(2.0f)
                .AddPositionalArg(20d)
                .AddPositionalArg("aaa");
            var same = new TermReference("id", "attr", callArgs);


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, new TermReference("id!!", "attr", callArgs), false);
            yield return new TestCaseData(same, new TermReference("id", "attr!!", callArgs), false);
            yield return new TestCaseData(same, new TermReference("id", "attr", callArgs.Build()), true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(TermRefTestData))]
        public void TestTermRef(TermReference comment, object? other, bool expected)
        {
            switch (other)
            {
                case TermReference otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((TermReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> MsgRefTestData()
        {
            var same = new MessageReference("id", "attr");


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, new MessageReference("id!!", "attr"), false);
            yield return new TestCaseData(same, new MessageReference("id", "attr!!"), false);
            yield return new TestCaseData(same, new MessageReference("id", "attr"), true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(MsgRefTestData))]
        public void TestTermRef(MessageReference comment, object? other, bool expected)
        {
            switch (other)
            {
                case MessageReference otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((MessageReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> VarRefTestData()
        {
            var same = new VariableReference("id");


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, new VariableReference("id!!"), false);
            yield return new TestCaseData(same, new VariableReference("id"), true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(VarRefTestData))]
        public void TestVarRef(VariableReference comment, object? other, bool expected)
        {
            switch (other)
            {
                case VariableReference otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((VariableReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> DynRefTestData()
        {
            var same = new DynamicReference("id", "attr", CallArguments.Builder().AddPositionalArg("aaa"));


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, new DynamicReference("id!!"), false);
            yield return new TestCaseData(same, new DynamicReference("id", "attr"), false);
            yield return new TestCaseData(same,
                new DynamicReference("id", "attr", CallArguments.Builder().AddPositionalArg("aaa").Build()), true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(DynRefTestData))]
        public void TestDynRef(DynamicReference comment, object? other, bool expected)
        {
            switch (other)
            {
                case DynamicReference otherComment:
                    Assert.That(comment == otherComment, Is.EqualTo(expected));
                    Assert.That(comment != otherComment, Is.EqualTo(!expected));
                    Assert.That(comment.Equals(otherComment), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(comment.Equals((DynamicReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(comment, null), Is.False);
                    Assert.That(Equals(null, comment), Is.False);
                    break;
            }

            Assert.That(comment.Equals(other), Is.EqualTo(expected));
            Assert.That(comment.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }
    }
}