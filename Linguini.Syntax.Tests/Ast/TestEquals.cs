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
            var same = new AstComment(CommentLevel.Comment, "thisObj");
            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, new AstComment(CommentLevel.GroupComment, "thisObj"), false);
            yield return new TestCaseData(same, new AstComment(CommentLevel.Comment, "thisObjzzzz"), false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AstCommentExample))]
        public void TestAstComment(AstComment thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case AstComment otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    Assert.That(thisObj.GetId() == otherObj.GetId());
                    break;
                case null:
                    Assert.That(thisObj.Equals((AstComment?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestLocation(AstLocation thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case AstLocation otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((AstLocation?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestTextLiteral(TextLiteral thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case TextLiteral otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((TextLiteral?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestNumberData(NumberLiteral thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case NumberLiteral otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((NumberLiteral?)other), Is.EqualTo(expected));
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestIdentifier(Identifier thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case Identifier otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((Identifier?)other), Is.EqualTo(expected));
                    Assert.That(Identifier.Comparer.Equals(thisObj, null), Is.False);
                    Assert.That(Identifier.Comparer.Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestAttribute(Attribute thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case Attribute otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((Attribute?)other), Is.EqualTo(expected));
                    Assert.That(Attribute.Comparer.Equals(thisObj, null), Is.False);
                    Assert.That(Attribute.Comparer.Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestPattern(Pattern thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case Pattern otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((Pattern?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestNamedArgs(NamedArgument thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case NamedArgument otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((NamedArgument?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestCallArguments(CallArguments thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case CallArguments otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((CallArguments?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestTermRef(TermReference thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case TermReference otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((TermReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestTermRef(MessageReference thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case MessageReference otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((MessageReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestVarRef(VariableReference thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case VariableReference otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((VariableReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
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
        public void TestDynRef(DynamicReference thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case DynamicReference otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((DynamicReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> FuncRefTestData()
        {
            var same = new FunctionReference("id", CallArguments.Builder().AddPositionalArg("aaa").Build());


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same,
                new FunctionReference("id!!", CallArguments.Builder().AddPositionalArg("aaa").Build()), false);
            yield return new TestCaseData(same,
                new FunctionReference("id", CallArguments.Builder().AddPositionalArg(3.0f).Build()), false);
            yield return new TestCaseData(same,
                new FunctionReference("id", CallArguments.Builder().AddPositionalArg("aaa").Build()), true);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(FuncRefTestData))]
        public void TestFunRef(FunctionReference thisObj, object? other, bool expected)
        {
            switch (other)
            {
                case FunctionReference otherObj:
                    Assert.That(thisObj == otherObj, Is.EqualTo(expected));
                    Assert.That(thisObj != otherObj, Is.EqualTo(!expected));
                    Assert.That(thisObj.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(thisObj.Equals((FunctionReference?)other), Is.EqualTo(expected));
                    Assert.That(Equals(thisObj, null), Is.False);
                    Assert.That(Equals(null, thisObj), Is.False);
                    break;
            }

            Assert.That(thisObj.Equals(other), Is.EqualTo(expected));
            Assert.That(thisObj.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }
        
        private static IEnumerable<TestCaseData> VariantTestData()
        {
            var patternBuilder = Pattern.Builder()
                .AddTermReference("id", "aaa");
            var patternBuilder2 = Pattern.Builder()
                .AddTermReference("id", "aaa")
                .AddText("AAAB");
            var same = new Variant("id", patternBuilder.Build());


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, new Variant("id!!", patternBuilder.Build()), false);
            yield return new TestCaseData(same, patternBuilder2.Build(), false);
            yield return new TestCaseData(same, new Variant("id", patternBuilder.Build()), true);
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(VariantTestData))]
        public void TestVariant(Variant variant, object? other, bool expected)
        {
            switch (other)
            {
                case Variant otherObj:
                    Assert.That(variant == otherObj, Is.EqualTo(expected));
                    Assert.That(variant != otherObj, Is.EqualTo(!expected));
                    Assert.That(variant.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(variant.Equals((Variant?)other), Is.EqualTo(expected));
                    Assert.That(Equals(variant, null), Is.False);
                    Assert.That(Equals(null, variant), Is.False);
                    break;
            }

            Assert.That(variant.Equals(other), Is.EqualTo(expected));
            Assert.That(variant.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }
        
        private static IEnumerable<TestCaseData> SelectTestData()
        {
            var selectBuild = SelectExpression.Builder(InlineExpressionBuilder.CreateNumber(2.0f).Build())
                .AddVariant("0.0", Pattern.Builder().AddTermReference("id"))
                .SetDefault();

            var same = selectBuild.Build();
            var same2 = selectBuild.Build();
            
            var diff = SelectExpression.Builder(InlineExpressionBuilder.CreateVariableReferences("aa").Build())
                .AddVariant("0.0", Pattern.Builder().AddTermReference("id"))
                .AddVariant(0.0f, Pattern.Builder().AddFunctionReference("COUNT"))
                .SetDefault()
                .Build();


            yield return new TestCaseData(same, 3, false);
            yield return new TestCaseData(same, null, false);
            yield return new TestCaseData(same, same, true);
            yield return new TestCaseData(same, CallArguments.Empty, false);
            yield return new TestCaseData(same, diff, false);
            yield return new TestCaseData(same, same2, true);
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(SelectTestData))]
        public void TestSelectExpr(SelectExpression variant, object? other, bool expected)
        {
            switch (other)
            {
                case SelectExpression otherObj:
                    Assert.That(variant == otherObj, Is.EqualTo(expected));
                    Assert.That(variant != otherObj, Is.EqualTo(!expected));
                    Assert.That(variant.Equals(otherObj), Is.EqualTo(expected));
                    break;
                case null:
                    Assert.That(variant.Equals((SelectExpression?)other), Is.EqualTo(expected));
                    Assert.That(Equals(variant, null), Is.False);
                    Assert.That(Equals(null, variant), Is.False);
                    break;
            }

            Assert.That(variant.Equals(other), Is.EqualTo(expected));
            Assert.That(variant.GetHashCode() == (other?.GetHashCode() ?? 0), Is.EqualTo(expected));
        }
    }
}