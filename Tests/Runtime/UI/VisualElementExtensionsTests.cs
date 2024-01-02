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
        public void VisualElementExtensions_GetContext_ShouldReturnContextFromApplication()
        {
            var v = new Panel();
            
            LangContext langContext = default;
            ThemeContext themeContext = default;
            ScaleContext scaleContext = default;
            
            Assert.DoesNotThrow(() =>
            {
                langContext = v.GetContext<LangContext>();
                themeContext = v.GetContext<ThemeContext>();
                scaleContext = v.GetContext<ScaleContext>();
            });
            
            Assert.AreEqual(v.lang, langContext.lang);
            Assert.AreEqual(v.theme, themeContext.theme);
            Assert.AreEqual(v.scale, scaleContext.scale);
        }

        [Test]
        [TestCase("fr", "dark")]
        [TestCase("de", "light")]
        [TestCase("en", "light")]
        [TestCase("en", "dark")]
        public void VisualElementExtensions_GetContext_ShouldComputeContextWithOverrides(string lang, string theme)
        {
            var v = new Panel();
            var overrideElement = new VisualElement();
            overrideElement.ProvideContext(new LangContext(lang));
            overrideElement.ProvideContext(new ThemeContext(theme));
           
            Assert.AreEqual(lang, overrideElement.GetSelfContext<LangContext>().lang);
            Assert.AreEqual(theme, overrideElement.GetSelfContext<ThemeContext>().theme);
            v.Add(overrideElement);

            LangContext langContext = default;
            ThemeContext themeContext = default;
            ScaleContext scaleContext = default;
            
            Assert.DoesNotThrow(() =>
            {
                langContext = overrideElement.GetContext<LangContext>();
                themeContext = overrideElement.GetContext<ThemeContext>();
                scaleContext = overrideElement.GetContext<ScaleContext>();
            });
            Assert.AreEqual(overrideElement.GetSelfContext<LangContext>().lang, langContext.lang);
            Assert.AreEqual(overrideElement.GetSelfContext<ThemeContext>().theme, themeContext.theme);
            Assert.AreEqual(v.scale, scaleContext.scale);
        }
    }
}
