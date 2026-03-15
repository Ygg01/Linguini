using System.Collections.Generic;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Shared.Types.Bundle;
using Linguini.Syntax.Ast;
using NUnit.Framework;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS8604 // Possible null reference argument.

namespace Linguini.Bundle.Test.Unit
{
    public class ConformanceTests
    {
        private const string Res1 = @"
term = term
    .attr = 3";

        public static IEnumerable<IReadBundle> AllBundles()
        {
            // Nonconcurrent bundle
            yield return LinguiniBuilder.Builder().Locale("en-US").AddResource(Res1).UncheckedBuild();
            // Concurrent bundle
            yield return LinguiniBuilder.Builder().Locale("en-US").AddResource(Res1).UseConcurrent().UncheckedBuild();
            // Frozen bundle
            yield return LinguiniBuilder.Builder().Locale("en-US").AddResource(Res1).UncheckedBuild().ToFrozenBundle();
            // Nonconcurrent experimental bundle
            yield return LinguiniBuilder.Builder(true).Locale("en-US").AddResource(Res1).UncheckedBuild();
            // Concurrent experimental bundle
            yield return LinguiniBuilder.Builder(true).Locale("en-US").AddResource(Res1).UseConcurrent()
                .UncheckedBuild();
            // Frozen experimental bundle
            yield return LinguiniBuilder.Builder(true).Locale("en-US").AddResource(Res1).UncheckedBuild()
                .ToFrozenBundle();
        }

        #region HasMethods

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void HasMessage(IReadBundle bundle)
        {
            Assert.That(bundle.HasMessage("term"), Is.True);
            Assert.That(bundle.HasMessage("nonExistent"), Is.False);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void HasAttrMessage(IReadBundle bundle)
        {
            Assert.That(bundle.HasAttrMessage("term.attr"), Is.True);
            Assert.That(bundle.HasAttrMessage("term.notExistent"), Is.False);
        }

        #endregion


        #region GetMethods

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void GetMessage(IReadBundle bundle)
        {
            // Message
            var message = bundle.GetMessage("term");
            Assert.That(message, Is.EqualTo("term"));

            // Missing message
            Assert.Throws<LinguiniException>(() => bundle.GetMessage("missing"));
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void GetMessageAndAttr(IReadBundle bundle)
        {
            // Message with attribute
            var messageAtttr = bundle.GetMessage("term", "attr");
            Assert.That(messageAtttr, Is.EqualTo("3"));

            // Missing message
            Assert.Throws<LinguiniException>(() => bundle.GetMessage("missing"));
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void GetAttrMessage(IReadBundle bundle)
        {
            // Messsage with term.attribute
            var attrMessage = bundle.GetAttrMessage("term.attr");
            Assert.That(attrMessage, Is.EqualTo("3"));

            // Check negative case
            Assert.Throws<LinguiniException>(() => bundle.GetAttrMessage("missing.attr"));
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void GetPatternUnchecked(IReadBundle bundle)
        {
            IList<FluentError>? errors = null;
            var pattern = new PatternBuilder().AddMessage("term").Build();
            var formatted = bundle.GetPatternUnchecked(pattern, null);
            Assert.That(errors, Is.Null);
            Assert.That(formatted, Is.EqualTo("term"));

            // Check negative case
            var nonExistent = new PatternBuilder().AddMessage("nonExistent").Build();
            Assert.Throws<LinguiniException>(() => bundle.GetPatternUnchecked(nonExistent, null));
        }

        #endregion


        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void FormatPattern(IReadBundle bundle)
        {
            var astMessage = AstMessageBuilder.Builder("term").SetPattern(new PatternBuilder("term")).Build();
            // Pattern formatting
            var pattern = bundle.FormatPattern(astMessage.Value, null, out var err);
            Assert.That(err, Is.Null);
            Assert.That(pattern, Is.EqualTo("term"));

            // Check negative case
            var nonExistentPattern = new PatternBuilder().AddMessage("nonExistent").Build();
            var nonExistentMessage = bundle.FormatPattern(nonExistentPattern, null, out err);
            Assert.That(nonExistentMessage, Is.EqualTo("{nonExistent}"));
            Assert.That(err, Is.Not.Null);
        }


        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void FormatPatternErrRef(IReadBundle bundle)
        {
            IList<FluentError>? errors = null;
            var astMessage = AstMessageBuilder.Builder("term").SetPattern(new PatternBuilder("term")).Build();
            var formatted = bundle.FormatPatternErrRef(astMessage.Value, null, ref errors);
            Assert.That(errors, Is.Null);
            Assert.That(formatted, Is.EqualTo("term"));

            // Check negative case
            var pattern = new PatternBuilder().AddMessage("nonExistent").Build();
            var message = bundle.FormatPatternErrRef(pattern, null, ref errors);
            Assert.That(errors, Is.Not.Null);
            Assert.That(message, Is.EqualTo("{nonExistent}"));
        }

        #region TryGetMethods

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void TryGetPattern(IReadBundle bundle)
        {
            var pattern = new PatternBuilder("term").Build();
            var result = bundle.TryGetPattern(pattern, null, out IList<FluentError>? errors1, out var formattedMessage);
            Assert.That(result, Is.True);
            Assert.That(formattedMessage, Is.EqualTo("term"));
            Assert.That(errors1, Is.Null);

            // Check negative case
            var nonExistent = new PatternBuilder().AddMessage("nonExistent").Build();
            var wrong = bundle.TryGetPattern(nonExistent, null, out var formattedMessage2,
                out IList<FluentError>? errors2);
            Assert.That(wrong, Is.False);
            Assert.That(formattedMessage2, Is.Null);
            Assert.That(errors2, Is.Not.Empty);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void TryGetMessage(IReadBundle bundle)
        {
            var res1 = bundle.TryGetMessage("term", null, out var errors1, out var message);
            Assert.That(res1, Is.True);
            Assert.That(errors1, Is.Null);
            Assert.That(message, Is.EqualTo("term"));

            // Check negative case
            var res2 = bundle.TryGetMessage("nonExistent", null, out var errors2, out var missingMessage);
            Assert.That(res2, Is.False);
            Assert.That(errors2, Is.Not.Empty);
            Assert.That(missingMessage, Is.Null);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void TryGetMessageErrRef(IReadBundle bundle)
        {
            IList<FluentError>? errors = null;
            var res1 = bundle.TryGetMessageErrRef("term", "attr", null, ref errors, out var message);
            Assert.That(res1, Is.True);
            Assert.That(errors, Is.Null);
            Assert.That(message, Is.EqualTo("3"));

            // Check negative case
            var res2 = bundle.TryGetMessageErrRef("term", "xyz", null, ref errors, out var missingMessage);
            Assert.That(res2, Is.False);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(missingMessage, Is.Null);
        }

        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void TryGetAttrMessageErrRef(IReadBundle bundle)
        {
            IList<FluentError>? errors = null;
            var res1 = bundle.TryGetAttrMessageErrRef("term.attr", null, ref errors, out var message);
            Assert.That(res1, Is.True);
            Assert.That(errors, Is.Null);
            Assert.That(message, Is.EqualTo("3"));

            // Check negative case
            var res2 = bundle.TryGetAttrMessageErrRef("termyz", null, ref errors, out var missingMessage);
            Assert.That(res2, Is.False);
            Assert.That(errors, Is.Not.Empty);
            Assert.That(missingMessage, Is.Null);
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void TryGetMessage2(IReadBundle bundle)
        {
            var res1 = bundle.TryGetMessage("term", "attr", null, out var errors1, out var message1);
            Assert.That(res1, Is.True);
            Assert.That(errors1, Is.Null);
            Assert.That(message1, Is.EqualTo("3"));
            
            var res2 = bundle.TryGetMessage("term", "xyz", null, out var errors2, out var message2);
            Assert.That(res2, Is.False);
            Assert.That(errors2, Is.Not.Empty);
            Assert.That(message2, Is.Null);
        }
        
        [Test]
        [Parallelizable]
        [TestCaseSource(nameof(AllBundles))]
        public void TryGetAttrMessage(IReadBundle bundle)
        {
            var res1 = bundle.TryGetAttrMessage("term.attr", null, out var errors1, out var message1);
            Assert.That(res1, Is.True);
            Assert.That(errors1, Is.Null);
            Assert.That(message1, Is.EqualTo("3"));
            
            var res2 = bundle.TryGetAttrMessage("term.xyz", null, out var errors2, out var message2);
            Assert.That(res2, Is.False);
            Assert.That(errors2, Is.Not.Empty);
            Assert.That(message2, Is.Null);
        }

        #endregion
    }
}