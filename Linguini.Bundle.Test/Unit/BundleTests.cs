using System;
using System.Collections.Generic;
using System.Globalization;
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

        private static string _res2 = @"
term = term
    .attr = 6";

        private static string _wrong = @"
    term = 1";

        [Test]
        public void TestDefaultBundleOptions()
        {
            var defaultBundleOpt = new FluentBundleOption();
            var bundle = new FluentBundle("en", defaultBundleOpt, out _);
            Assert.AreEqual(new CultureInfo("en"), bundle.Culture);
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
            var bundle = new FluentBundle("en", defaultBundleOpt, out _);
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
                .AddResource(_res1)
                .SetUseIsolating(false);

            var bundle = bundler.UncheckedBuild();
            Assert.IsTrue(bundle.TryGetAttrMsg("term", null, out _, out var termMsg));
            Assert.AreEqual("term", termMsg);
            Assert.IsTrue(bundle.TryGetAttrMsg("term.attr", null, out _, out var msg));
            Assert.AreEqual("3", msg);

            bundle.AddResourceOverriding(_res2);
            Assert.IsTrue(bundle.TryGetAttrMsg("term", null, out _, out termMsg));
            Assert.AreEqual("term", termMsg);
            Assert.IsTrue(bundle.TryGetAttrMsg("term.attr", null, out _, out msg));
            Assert.AreEqual("6", msg);
        }

        [Test]
        [Parallelizable]
        public void TestExceptions()
        {
            var bundler = LinguiniBuilder.Builder()
                .Locales("en-US", "sr-RS")
                .AddResources(_res1, _res2)
                .SetUseIsolating(false)
                .SetFormatterFunc(_formatter)
                .SetTransformFunc(_transform)
                .AddFunction("id", _idFunc)
                .AddFunction("zero", _zeroFunc);

            Assert.Throws(typeof(LinguiniException), () => bundler.UncheckedBuild());
        }
    }
}
