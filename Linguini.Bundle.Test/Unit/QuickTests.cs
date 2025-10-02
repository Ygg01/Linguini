using System.Collections.Generic;
using System.Linq;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Function;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [TestOf(typeof(FluentBundle))]
    public class QuickTests
    {
        private const string DynRef = @"
cat = {$number ->
  *[one] Cat
  [other] Cats
}
dog = {$number ->
  *[one] Dog
  [other] Dogs
}
attack-log = { $$attacker } attacked {$$defender}.
";


        [Test]
        [TestCase(DynRef)]
        public void TestDynamicReference(string input)
        {
            var (bundle, err) = LinguiniBuilder.Builder(true).Locale("en-US")
                .AddResource(input)
                .Build();
            Assert.That(err, Is.Null);
            var args = new Dictionary<string, IFluentType>
            {
                ["attacker"] = (FluentReference)"cat",
                ["defender"] = (FluentReference)"dog"
            };
            Assert.That(bundle.TryGetMessage("attack-log", args, out _, out var message));
            Assert.That("Cat attacked Dog.", Is.EqualTo(message));
        }

        private const string Macros = @"
-ship = Ship
    .gender = {$style ->
        *[traditional] neuter
          [chicago] feminine
    }
call-attr-no-args = { -ship.gender()  ->
    *[masculine] He
      [feminine] She
      [neuter] It
}
";

        [Test]
        [Parallelizable]
        public void TestExtensionsWork()
        {
            var (bundle, err) = LinguiniBuilder.Builder(true).Locale("en-US")
                .AddResource(Macros)
                .Build();
            Assert.That(err, Is.Null);
            var args = new Dictionary<string, IFluentType>
            {
                ["style"] = (FluentString)"chicago"
            };
            Assert.That(bundle.TryGetMessage("call-attr-no-args", args, out _, out var message));
            // Logic goes like this: given that term reference has no args. Resolver will ignore `$style` in `-ship.gender()` selector
            // and default to neuter. This will then resolve `call-attr-no-args` to `It`.
            Assert.That(message, Is.EqualTo("It"));

            // Check Frozen bundle behaves similarly
            var frozenBundle = bundle.ToFrozenBundle();
            Assert.That(frozenBundle.TryGetMessage("call-attr-no-args", args, out _, out var frozenMessage));
            Assert.That("It", Is.EqualTo(frozenMessage));
        }

        private const string DynamicSelectors = @"
-creature-fairy = fairy
-creature-elf = elf
    .StartsWith = vowel

you-see = You see { $$object.StartsWith ->
    [vowel] an { $$object }
    *[consonant] a { $$object }
}.
";

        private const string FuncRefs = @"
emails = Number of unread emails { $unreadEmails }.
emails2 = Number of unread emails { NUMBER($unreadEmails) }.
liked-count = { $num ->
    [0]     No likes yet.
    [one]   One person liked your message.
    *[other] { $num } people liked your message.
}

liked-count2 = { NUMBER($num) ->
    [0]     No likes yet.
    [one]   One person liked your message.
    *[other] { $num } people liked your message.
}
";

        [Test]
        [Parallelizable]
        public void TestFunctionReferences()
        {
            var (bundle, err) = LinguiniBuilder.Builder(true)
                .Locale("en-US")
                .AddResource(FuncRefs)
                .AddFunction("NUMBER", LinguiniFluentFunctions.Number)
                .Build();
            Assert.That(err, Is.Null.Or.Empty);
            var args = new Dictionary<string, IFluentType>
            {
                ["unreadEmails"] = (FluentNumber)3
            };
            Assert.That(bundle.TryGetMessage("emails", args, out _, out var message1));
            Assert.That(message1, Is.EqualTo("Number of unread emails 3."));
            Assert.That(bundle.TryGetMessage("emails2", args, out _, out var message2));
            Assert.That(message2, Is.EqualTo("Number of unread emails 3."));

            var likedArg0 = new Dictionary<string, IFluentType>
            {
                ["num"] = (FluentNumber)0
            };
            var likedArg1 = new Dictionary<string, IFluentType>
            {
                ["num"] = (FluentNumber)1
            };
            var likedArg2 = new Dictionary<string, IFluentType>
            {
                ["num"] = (FluentNumber)2
            };
            Assert.That(bundle.TryGetMessage("liked-count", likedArg0, out _, out var likedMessage0));
            Assert.That(bundle.TryGetMessage("liked-count", likedArg1, out _, out var likedMessage1));
            Assert.That(bundle.TryGetMessage("liked-count", likedArg2, out _, out var likedMessage2));
            Assert.That(likedMessage0, Is.EqualTo("No likes yet."));
            Assert.That(likedMessage1, Is.EqualTo("One person liked your message."));
            Assert.That(likedMessage2, Is.EqualTo("2 people liked your message."));

            Assert.That(bundle.TryGetMessage("liked-count2", likedArg0, out _, out var likedMessage2_0));
            Assert.That(bundle.TryGetMessage("liked-count2", likedArg1, out _, out var likedMessage2_1));
            Assert.That(bundle.TryGetMessage("liked-count2", likedArg2, out _, out var likedMessage2_2));
            Assert.That(likedMessage2_0, Is.EqualTo("No likes yet."));
            Assert.That(likedMessage2_1, Is.EqualTo("One person liked your message."));
            Assert.That(likedMessage2_2, Is.EqualTo("2 people liked your message."));
        }

        private static readonly Dictionary<string, IFluentType> UnreadEmail2 = new()
        {
            ["unreadEmails"] = (FluentNumber)2
        };

        private const string SelectorVarRef = @"
liked-count = { $num ->
    [0]     No likes yet.
    [one]   One person liked your message.
    *[other] { $num } people liked your message.
}";

        private const string SelectorFuncRef = @"
liked-count = { NUMBER($num) ->
    [0]     No likes yet.
    [one]   One person liked your message.
    *[other] { $num } people liked your message.
}";

        private const string TermRefs = @"
-ship = Ship
    .zero = {NUMBER(0)}
    .one = {NUMBER(1)}

liked-count = { -ship.zero() ->
    [0]     No likes yet.
    [one]   One person liked your message.
    *[other] { $num } people liked your message.
}
";

        private const string TermReferences = @"
foo = Foo { $num }
bar = { foo }
";

        private const string MissingAttributeStr = @"
foo = Foo
ref-foo = { foo.missing }
";
        
        private const string PlaceableRefs = @"
foo = Foo
bar = Bar
    .attr = { foo } Attribute
ref-bar = { bar.attr }
";

        private static IEnumerable<TestCaseData> TestDataFunc()
        {
            yield return new TestCaseData(
                    "emails = Number of unread emails { $unreadEmails }.",
                    "emails",
                    "Number of unread emails 2.",
                    "unreadEmails", (FluentNumber)2)
                .SetName("Placeable reference")
                .Returns(true);
            yield return new TestCaseData(
                    "emails2 = Number of unread emails { NUMBER($unreadEmails) }.",
                    "emails2",
                    "Number of unread emails 2.",
                    "unreadEmails", (FluentNumber)2)
                .SetName("Function reference")
                .Returns(true);
            yield return new TestCaseData(
                    SelectorVarRef,
                    "liked-count",
                    "2 people liked your message.",
                    "num", (FluentNumber)2)
                .SetName("Selector placeable var reference two")
                .Returns(true);
            yield return new TestCaseData(
                    SelectorVarRef,
                    "liked-count",
                    "One person liked your message.",
                    "num", (FluentNumber)1)
                .SetName("Selector placeable var reference one")
                .Returns(true);
            yield return new TestCaseData(
                    SelectorFuncRef,
                    "liked-count",
                    "One person liked your message.",
                    "num", (FluentNumber)1)
                .SetName("Selector placeable func reference one")
                .Returns(true);
            yield return new TestCaseData(
                    SelectorFuncRef,
                    "liked-count",
                    "No likes yet.",
                    "num", (FluentNumber)0)
                .SetName("Selector placeable func reference zero")
                .Returns(true);
            yield return new TestCaseData(
                    TermReferences,
                    "bar",
                    "Foo 3",
                    "num", (FluentNumber)3)
                .SetName("Term reference")
                .Returns(true);
            yield return new TestCaseData(
                    @"foo = { { ""Foo"" } }",
                    "foo",
                    "Foo",
                    "", (FluentString)""
                )
                .SetName("Nested placeable")
                .Returns(true);
            yield return new TestCaseData(
                    "foo = { $arg }",
                    "foo",
                    "{$arg}",
                    "", (FluentString)""
                )
                .SetName("Missing variable reference")
                .Returns(false);
            yield return new TestCaseData(
                    MissingAttributeStr,
                    "ref-foo",
                    "{foo.missing}",
                    "", (FluentString)""
                )
                .SetName("Missing term reference")
                .Returns(false);
            yield return new TestCaseData(
                    PlaceableRefs,
                    "ref-bar",
                    "Foo Attribute",
                    "num", (FluentNumber)3)
                .SetName("Term attribute reference")
                .Returns(true);
            yield return new TestCaseData(
                    "-foo = Bar\nbaz = { foo }",
                    "baz",
                    "{foo}",
                    "", (FluentString)"")
                .SetName("Entry mismatch isn't leaked")
                .Returns(false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(TestDataFunc))]
        public bool TestSimple(string resource, string message,
            string expected, string argName, object argValue)
        {
            var (bundle, err) = LinguiniBuilder.Builder()
                .Locale("en-US")
                .AddResource(resource)
                .AddFunction("NUMBER", LinguiniFluentFunctions.Number)
                .AddFunction("IDENTITY", LinguiniFluentFunctions.Identity)
                .Build();

            var a = new Dictionary<string, IFluentType>();
            a.Add(argName, (IFluentType)argValue);

            var isCorrect = bundle.TryGetMessage(message, a, out _, out var result);
            Assert.That(result, Is.EqualTo(expected));
            return isCorrect;
        }


        [Test]
        [Parallelizable]
        public void TestTermReferences()
        {
            var (bundle, err) = LinguiniBuilder.Builder(true)
                .Locale("en-US")
                .AddResource(TermRefs)
                .AddFunction("NUMBER", LinguiniFluentFunctions.Number)
                .Build();
            Assert.That(err, Is.Null.Or.Empty);
            var args = new Dictionary<string, IFluentType>();
            Assert.That(bundle.TryGetMessage("liked-count", args, out _, out var message1));
            Assert.That(message1, Is.EqualTo("No likes yet."));
        }

        [Test]
        [Parallelizable]
        public void TestDynamicSelectors()
        {
            var (bundle, err) = LinguiniBuilder.Builder(true)
                .Locale("en-US")
                .AddResource(DynamicSelectors)
                .Build();
            Assert.That(err, Is.Null);
            var args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-elf"
            };
            Assert.That(bundle.TryGetMessage("you-see", args, out _, out var message1));
            Assert.That(message1, Is.EqualTo("You see an elf."));
            args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-fairy"
            };
            Assert.That(bundle.TryGetMessage("you-see", args, out _, out var message2));
            Assert.That("You see a fairy.", Is.EqualTo(message2));

            // Check Frozen bundle behaves similarly
            var frozenBundle = bundle.ToFrozenBundle();
            args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-elf"
            };
            Assert.That(frozenBundle.TryGetMessage("you-see", args, out _, out var frozenMessage1));
            Assert.That("You see an elf.", Is.EqualTo(frozenMessage1));
            args = new Dictionary<string, IFluentType>
            {
                ["object"] = (FluentReference)"creature-fairy"
            };
            Assert.That(frozenBundle.TryGetMessage("you-see", args, out _, out var frozenMessage2));
            Assert.That(frozenMessage2, Is.EqualTo("You see a fairy."));
        }
    }
}