using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Linguini.Bundle.Builder;
using Linguini.Bundle.Errors;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test.Unit
{
    [TestFixture]
    [TestOf(typeof(FluentBundle))]
    [Parallelizable]
    public class BundleTests
    {
        private readonly ExternalFunction _zeroFunc = (_, _) => FluentNone.None;
        private readonly ExternalFunction _idFunc = (args, _) => args[0];
        private readonly Func<IFluentType, string> _formatter = _ => "";
        private readonly Func<string, string> _transform = str => str.ToUpper(CultureInfo.InvariantCulture);

        private static string _res1 = @"
term = term
    .attr = 3";

        private static string _wrong = @"
    term = 1";
        
        private static string _multi = @"
term1 = val1
term2 = val2
    .attr = 6";
        
        private static string _replace1 = @"
term1 = val1
term2 = val2";
        
        private static string _replace2 = @"
term1 = xxx
new1  = new
    .attr = 6";

        [Test]
        public void TestDefaultBundleOptions()
        {
            var defaultBundleOpt = new FluentBundleOption();
            var bundle = FluentBundle.MakeUnchecked(defaultBundleOpt);
            Assert.AreEqual(CultureInfo.CurrentCulture, bundle.Culture);
            Assert.IsNull(bundle.FormatterFunc);
            Assert.IsNull(bundle.TransformFunc);
            Assert.IsTrue(bundle.UseIsolating);
            Assert.AreEqual(100, bundle.MaxPlaceable);
        }

        [Test]
        public void TestNonDefaultBundleOptions()
        {
            var defaultBundleOpt = new FluentBundleOption()
            {
                Locales = {"en"},
                MaxPlaceable = 123,
                UseIsolating = false,
                TransformFunc = _transform,
                FormatterFunc = _formatter,
                Functions = new Dictionary<string, ExternalFunction>()
                {
                    ["zero"] = _zeroFunc,
                    ["id"] = _idFunc,
                }
            };
            var bundle = FluentBundle.MakeUnchecked(defaultBundleOpt);
            Assert.AreEqual(new CultureInfo("en"), bundle.Culture);
            Assert.AreEqual(defaultBundleOpt.FormatterFunc, bundle.FormatterFunc);
            Assert.AreEqual(defaultBundleOpt.TransformFunc, bundle.TransformFunc);
            Assert.AreEqual(defaultBundleOpt.UseIsolating, bundle.UseIsolating);
            Assert.AreEqual(defaultBundleOpt.MaxPlaceable, bundle.MaxPlaceable);
            Assert.IsTrue(bundle.TryGetFunction("zero", out var zero));
            Assert.AreEqual(_zeroFunc, zero?.Function);
            Assert.IsTrue(bundle.TryGetFunction("id", out var id));
            Assert.AreEqual(_idFunc, id?.Function);
        }

        [Test]
        [Parallelizable]
        public void TestReplaceMessage()
        {
            var bundler = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en"))
                .AddResource(_replace1)
                .SetUseIsolating(false);

            var bundle = bundler.UncheckedBuild();
            Assert.IsTrue(bundle.TryGetAttrMsg("term1", null, out _, out var termMsg));
            Assert.AreEqual("val1", termMsg);
            Assert.IsTrue(bundle.TryGetAttrMsg("term2", null, out _, out var termMsg2));
            Assert.AreEqual("val2", termMsg2);

            bundle.AddResourceOverriding(_replace2);
            Assert.IsTrue(bundle.TryGetAttrMsg("term2", null, out _, out _));
            Assert.IsTrue(bundle.TryGetAttrMsg("term1", null, out _, out termMsg));
            Assert.AreEqual("xxx", termMsg);
            Assert.IsTrue(bundle.TryGetAttrMsg("new1.attr", null, out _, out var newMsg));
            Assert.AreEqual("6", newMsg);
        }

        [Test]
        [Parallelizable]
        public void TestExceptions()
        {
            var bundler = LinguiniBuilder.Builder()
                .Locales("en-US", "sr-RS")
                .AddResources(_wrong, _res1)
                .SetUseIsolating(false)
                .SetFormatterFunc(_formatter)
                .SetTransformFunc(_transform)
                .AddFunction("id", _idFunc)
                .AddFunction("zero", _zeroFunc);

            Assert.Throws(typeof(LinguiniException), () => bundler.UncheckedBuild());
        }
        [Test]
        public void TestEnumeration()
        {
            var bundler = LinguiniBuilder.Builder()
                .Locale("en-US")
                .AddResource(_multi)
                .AddFunction("id", _idFunc)
                .AddFunction("zero", _zeroFunc)
                .UncheckedBuild();
            var messages = bundler.GetMessageEnumerable().ToArray();
            var functions = bundler.GetFuncEnumerable().ToArray();
            CollectionAssert.AreEquivalent(new[] {"term1", "term2"}, messages);
            CollectionAssert.AreEquivalent(new[] {"id", "zero"}, functions);
        }
        
        [Test]
        public void TestConcurrencyBundler()
        {
            var bundler = LinguiniBuilder.Builder()
                .CultureInfo(new CultureInfo("en-US"))
                .SkipResources()
                .UseConcurrent()
                .UncheckedBuild();
            
            Parallel.For(0, 10, i => bundler.AddResource($"term-1 = {i}", out _));
            Parallel.For(0, 10, i => bundler.AddResource($"term-2= {i}", out _));
            Parallel.For(0, 10, i => bundler.TryGetAttrMsg("term-1", null, out _, out _));
            Parallel.For(0, 10, i => bundler.AddResourceOverriding($"term-2= {i+1}"));
            Assert.True(bundler.HasMessage("term-1"));
        }


        [Test]
        public void TestConcurrencyOption()
        {
            var bundleOpt = new FluentBundleOption()
            {
                Locales = {"en-US"},
                UseConcurrent = true,
            };
            var optBundle = FluentBundle.MakeUnchecked(bundleOpt);
            Parallel.For(0, 10, i => optBundle.AddResource($"term-1 = {i}", out _));
            Parallel.For(0, 10, i => optBundle.AddResource($"term-2= {i}", out _));
            Parallel.For(0, 10, i => optBundle.TryGetAttrMsg("term-1", null, out _, out _));
            Parallel.For(0, 10, i => optBundle.AddResourceOverriding($"term-2= {i+1}"));
            Assert.True(optBundle.HasMessage("term-1"));
        }
    }
}
