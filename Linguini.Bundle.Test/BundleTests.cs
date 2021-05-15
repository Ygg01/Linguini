using System;
using System.Collections.Generic;
using System.Globalization;
using Linguini.Bundle.Types;
using Linguini.Shared.Types.Bundle;
using NUnit.Framework;

namespace Linguini.Bundle.Test
{
    [TestFixture]
    [Parallelizable]
    public class BundleTests
    {
        private ExternalFunction _zeroFunc = (_, _) => FluentNone.None;
        private ExternalFunction _idFunc = (args, _) => args[0];
        private Func<IFluentType, string> _formatter = _ => "";
        private Func<string, string> _transform = str => str.ToUpper(CultureInfo.InvariantCulture);

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
    }
}
