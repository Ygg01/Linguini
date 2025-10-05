using System.Collections.Generic;
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
attack-log1 = { $$attacker() } attacked {$$defender($def_num)}.
attack-log2 = { $$attacker(number: $atk_num) } attacked {$$defender(number: $def_num)}.
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
                ["defender"] = (FluentString)"dog",
                ["atk_num"] = (FluentNumber)1,
                ["def_num"] = (FluentNumber)2
            };
            Assert.That(bundle.TryGetMessage("attack-log2", args, out _, out var message));
            Assert.That(message, Is.EqualTo("Cat attacked Dogs."));
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
            Assert.That(frozenMessage, Is.EqualTo("It"));
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

        private const string PlaceableMacros = @"
foo = Foo {$arg}
-bar = {foo}
ref-bar = {-bar}
";

        private const string PlaceableMacros2 = @"
-foo = Foo {$arg}
-qux = {-foo(arg: 1)}
ref-qux = {-qux}
";

        private const string PlaceableMacros3 = @"
foo =
    .attr = Foo Attr
bar = { foo } Bar";

        private const string SelectorMissing = @"
select = {$none ->
    [a] A
    *[b] B
}";

        private const string PlaceableMacros4 = @"
-ship = Ship
    .gender = {$style ->
      *[traditional] neuter
        [chicago] feminine
    }
ref-attr = {-ship.gender ->
  *[masculine] He
    [feminine] She
    [neuter] It
}";

        private const string LaBomba = @"
lol0 = LOL
lol1 = {lol0} {lol0} {lol0} {lol0} {lol0} {lol0} {lol0} {lol0} {lol0} {lol0}
lol2 = {lol1} {lol1} {lol1} {lol1} {lol1} {lol1} {lol1} {lol1} {lol1} {lol1}
lol3 = {lol2} {lol2} {lol2} {lol2} {lol2} {lol2} {lol2} {lol2} {lol2} {lol2}
lol4 = {lol3} {lol3} {lol3} {lol3} {lol3} {lol3} {lol3} {lol3} {lol3} {lol3}
lol5 = {lol4} {lol4} {lol4} {lol4} {lol4} {lol4} {lol4} {lol4} {lol4} {lol4}
lol6 = {lol5} {lol5} {lol5} {lol5} {lol5} {lol5} {lol5} {lol5} {lol5} {lol5}
lol7 = {lol6} {lol6} {lol6} {lol6} {lol6} {lol6} {lol6} {lol6} {lol6} {lol6}
lol8 = {lol7} {lol7} {lol7} {lol7} {lol7} {lol7} {lol7} {lol7} {lol7} {lol7}
lol9 = {lol8} {lol8} {lol8} {lol8} {lol8} {lol8} {lol8} {lol8} {lol8} {lol8}
lolz = {lol9}
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
            yield return new TestCaseData(
                    PlaceableMacros,
                    "ref-bar",
                    "Foo {$arg}",
                    "", (FluentString)"")
                .SetName("Nesting message reference part")
                .Returns(true);
            yield return new TestCaseData(
                    PlaceableMacros,
                    "ref-bar",
                    "Foo {$arg}",
                    "arg", (FluentNumber)3)
                .SetName("Ignore args of term references")
                .Returns(true);
            yield return new TestCaseData(
                    PlaceableMacros2,
                    "ref-qux",
                    "Foo 1",
                    "arg", (FluentNumber)3)
                .SetName("Use args of term references, rather than arguments")
                .Returns(true);
            yield return new TestCaseData(
                    PlaceableMacros3,
                    "foo",
                    "{???}",
                    "", (FluentString)"")
                .SetName("Message reference that is null")
                .Returns(false);
            yield return new TestCaseData(
                    PlaceableMacros3,
                    "bar",
                    "{foo} Bar",
                    "", (FluentString)"")
                .SetName("Message reference to non-existent message")
                .Returns(false);
            yield return new TestCaseData(
                    "foo = { foo }",
                    "foo",
                    "{foo}",
                    "", (FluentString)"")
                .SetName("Cyclic self-reference")
                .Returns(false);
            yield return new TestCaseData(
                    SelectorMissing,
                    "select",
                    "B",
                    "", (FluentString)"")
                .SetName("Selector missing")
                .Returns(false);
            yield return new TestCaseData(
                    PlaceableMacros4,
                    "ref-attr",
                    "It",
                    "style", (FluentString)"")
                .SetName("Parameterized term attributes")
                .Returns(true);
            yield return new TestCaseData(
                    LaBomba,
                    "lol1", "LOL LOL LOL LOL LOL LOL LOL LOL LOL LOL",
                    "", (FluentString)"")
                .SetName("La Bomba 1")
                .Returns(true);
            yield return new TestCaseData(
                    LaBomba,
                    "lolz", "{???}", "", (FluentString)"")
                .SetName("La Bomba Final")
                .Returns(false);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(TestDataFunc))]
        public bool TestSimple(string resource, string message,
            string expected, string argName, object argValue)
        {
            var (bundle, _) = LinguiniBuilder.Builder()
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


        private const string DynamicSelectors = @"
-creature-fairy = fairy
-creature-elf = elf
    .StartsWith = vowel

you-see = You see { $$object.StartsWith ->
    [vowel] an { $$object }
    *[consonant] a { $$object }
}.
";


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
        }

        private const string TransformFunc = @"
foo = Faa
    .bar = Bar {foo} Baz
bar = Bar {""Baz""}
";

        [Test]
        [Parallelizable]
        public void TestTransformFunc()
        {
            var (bundle, err) = LinguiniBuilder.Builder(true)
                .Locale("en-US")
                .AddResource(TransformFunc)
                .AddFunction("NUMBER", LinguiniFluentFunctions.Number)
                .SetTransformFunc(s => s.Replace('a', 'A'))
                .Build();
            Assert.That(err, Is.Null.Or.Empty);
            var args = new Dictionary<string, IFluentType>();
            Assert.That(bundle.TryGetMessage("foo", args, out _, out var actual));
            Assert.That(actual, Is.EqualTo("FAA"));
        }
    }
}