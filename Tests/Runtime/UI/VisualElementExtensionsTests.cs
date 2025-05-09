using System;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using VisualElementExtensions = Unity.AppUI.UI.VisualElementExtensions;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(VisualElementExtensions))]
    class VisualElementExtensionsTests
    {
        [Test]
        public void VisualElementExtensions_GetContext_ShouldThrowWithInvalidArg()
        {
            VisualElement v = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                v.GetContext();
            });
        }

        [Test]
        public void VisualElementExtensions_GetContext_ShouldReturnDefaultWithoutContextProvider()
        {
            var v = new VisualElement();
            Assert.DoesNotThrow(() =>
            {
                var ctx = v.GetContext();
                UnityEngine.Assertions.Assert.AreEqual(default(ApplicationContext), ctx);
            });
        }

        [Test]
        public void VisualElementExtensions_GetContext_ShouldReturnContextFromApplication()
        {
            var v = new Panel();
            ApplicationContext ctx = default;
            Assert.DoesNotThrow(() =>
            {
                ctx = v.GetContext();
            });
            Assert.AreEqual(v.lang, ctx.lang);
            Assert.AreEqual(v.theme, ctx.theme);
            Assert.AreEqual(v.scale, ctx.scale);
            Assert.AreEqual(v, ctx.panel);
        }

        [Test]
        [TestCase("fr", "dark")]
        [TestCase("de", "light")]
        [TestCase("en", "light")]
        [TestCase("en", "dark")]
        public void VisualElementExtensions_GetContext_ShouldComputeContextWithOverrides(string lang, string theme)
        {
            var v = new Panel();
            var overrideElement = new ContextProvider
            {
                lang = lang,
                theme = theme
            };
            Assert.AreEqual(lang, overrideElement.lang);
            Assert.AreEqual(theme, overrideElement.theme);
            v.Add(overrideElement);

            ApplicationContext ctx = default;
            Assert.DoesNotThrow(() =>
            {
                ctx = overrideElement.GetContext();
            });
            Assert.AreEqual(overrideElement.lang, ctx.lang);
            Assert.AreEqual(overrideElement.theme, ctx.theme);
            Assert.AreEqual(v.scale, ctx.scale);
            Assert.AreEqual(v, ctx.panel);
        }
    }
}
