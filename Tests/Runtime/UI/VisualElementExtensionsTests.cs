using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using VisualElementExtensions = Unity.AppUI.UI.VisualElementExtensions;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(VisualElementExtensions))]
    class VisualElementExtensionsTests
    {
        UIDocument m_TestUI;
        bool m_SetupDone;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!m_SetupDone)
            {
                // Load new scene
                var scene = SceneManager.CreateScene("VisualElementExtensionsTestScene-" + Random.Range(1, 100000));
                while (!SceneManager.SetActiveScene(scene))
                {
                    yield return null;
                }
                yield return Utils.WaitForLocalizationPreloaded();
                m_TestUI = Utils.ConstructTestUI();
            }
            UnityEngine.Device.Screen.SetResolution(120, 60, FullScreenMode.FullScreenWindow);
            m_TestUI.rootVisualElement.Clear();
            m_SetupDone = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
            m_SetupDone = false;
#pragma warning disable CS0618
            SceneManager.UnloadScene(SceneManager.GetActiveScene());
#pragma warning restore CS0618
        }

        [Test]
        public void VisualElementExtensions_GetChildren_ShouldReturnChildren()
        {
            var v = new VisualElement();
            var c1 = new VisualElement();
            var c2 = new VisualElement();
            var c3 = new VisualElement();
            v.Add(c1);
            v.Add(c2);
            v.Add(c3);

            var children = v.GetChildren<VisualElement>(true).ToList();
            Assert.AreEqual(3, children.Count);
            Assert.AreEqual(c1, children[0]);
            Assert.AreEqual(c2, children[1]);
            Assert.AreEqual(c3, children[2]);
        }

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

        [Test]
        public void VisualElementExtensions_SetTooltipTemplate_ShouldSucceed()
        {
            var v = new VisualElement();
            var changed = 0;
            var tooltipTemplate = new VisualElement();

            void OnTooltipTemplateChanged()
            {
                changed++;
            }

            Assert.Throws<ArgumentNullException>(() => v.RegisterTooltipTemplateChangedCallback(null));
            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.RegisterTooltipTemplateChangedCallback(null, OnTooltipTemplateChanged));
            v.RegisterTooltipTemplateChangedCallback(OnTooltipTemplateChanged);

            v.SetTooltipTemplate(tooltipTemplate);
            Assert.AreEqual(tooltipTemplate, v.GetTooltipTemplate());
            Assert.AreEqual(1, changed);

            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.SetTooltipTemplate(null, tooltipTemplate));
            Assert.DoesNotThrow(() => VisualElementExtensions.SetTooltipTemplate(v, null));
            Assert.AreEqual(null, v.GetTooltipTemplate());
            Assert.AreEqual(2, changed);

            Assert.Throws<ArgumentNullException>(() => v.UnregisterTooltipTemplateChangedCallback(null));
            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.UnregisterTooltipTemplateChangedCallback(null, OnTooltipTemplateChanged));
            v.UnregisterTooltipTemplateChangedCallback(OnTooltipTemplateChanged);
            v.SetTooltipTemplate(tooltipTemplate);
            Assert.AreEqual(2, changed);
        }

        [Test]
        public void VisualElementExtensions_SetTooltipContent_ShouldSucceed()
        {
            var v = new VisualElement();
            var template = new Text();

            var changed = 0;

            void OnTooltipContentChanged()
            {
                changed++;
            }

            Assert.Throws<ArgumentNullException>(() => v.RegisterTooltipContentChangedCallback(null));
            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.RegisterTooltipContentChangedCallback(null, OnTooltipContentChanged));
            v.RegisterTooltipContentChangedCallback(OnTooltipContentChanged);

            void TemplateContent(VisualElement t)
            {
                ((Text)t).text = "Tooltip";
            }

            Assert.Throws<InvalidOperationException>(() => v.SetTooltipContent(TemplateContent));
            v.SetTooltipTemplate(template);
            v.SetTooltipContent(TemplateContent);
            Assert.AreEqual((VisualElementExtensions.TooltipContentCallback)TemplateContent, v.GetTooltipContent());
            Assert.AreEqual(1, changed);

            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.SetTooltipContent(null, TemplateContent));
            Assert.DoesNotThrow(() => VisualElementExtensions.SetTooltipContent(v, null));
            Assert.AreEqual(null, v.GetTooltipContent());
            Assert.AreEqual(2, changed);

            Assert.Throws<ArgumentNullException>(() => v.UnregisterTooltipContentChangedCallback(null));
            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.UnregisterTooltipContentChangedCallback(null, OnTooltipContentChanged));
            v.UnregisterTooltipContentChangedCallback(OnTooltipContentChanged);
            v.SetTooltipContent(TemplateContent);
            Assert.AreEqual(2, changed);
        }

        [Test]
        public void VisualElementExtensions_IsOnScreen_ShouldThrowWhenElementIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => VisualElementExtensions.IsOnScreen(null));
        }

        [Test]
        public void VisualElementExtensions_IsOnScreen_ShouldReturnFalseWhenNoPanel()
        {
            var element = new VisualElement();
            Assert.IsFalse(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementIsFullyVisible()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var panel = new Panel();
            panel.style.width = m_TestUI.rootVisualElement.layout.width;
            panel.style.height = m_TestUI.rootVisualElement.layout.height;

            var element = new VisualElement();
            element.style.width = m_TestUI.rootVisualElement.layout.width - 1;
            element.style.height = m_TestUI.rootVisualElement.layout.height - 1;
            element.style.left = 0;
            element.style.top = 0;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementIsPartiallyOffScreenRight()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth - (rootWidth * 0.05f); // partially off-screen on the right
            element.style.top = rootHeight * 0.25f;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Element extends beyond the right edge, partially off-screen
            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementIsPartiallyOffScreenBottom()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth * 0.25f;
            element.style.top = rootHeight - (rootHeight * 0.05f); // partially off-screen on the bottom
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Element extends beyond the bottom edge, partially off-screen
            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnFalseWhenElementIsCompletelyOffScreenLeft()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = -(rootWidth * 0.15f); // completely off-screen on the left
            element.style.top = rootHeight * 0.25f;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.IsFalse(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnFalseWhenElementIsCompletelyOffScreenTop()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth * 0.25f;
            element.style.top = -(rootHeight * 0.15f); // completely off-screen on the top
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.IsFalse(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnFalseWhenElementHasZeroArea()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = 0;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth * 0.25f;
            element.style.top = rootHeight * 0.25f;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.IsFalse(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementIsAtLeftEdge()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = 0;
            element.style.top = rootHeight * 0.25f;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementIsAtTopEdge()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth * 0.25f;
            element.style.top = 0;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementTouchesRightEdge()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth - (rootWidth * 0.1f); // touches right edge
            element.style.top = rootHeight * 0.25f;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Element touches right edge, should be on screen
            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnTrueWhenElementTouchesBottomEdge()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth * 0.25f;
            element.style.top = rootHeight - (rootHeight * 0.1f); // touches bottom edge
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Element touches bottom edge, should be on screen
            Assert.IsTrue(element.IsOnScreen());
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldReturnFalseWhenElementIsInvisible()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var element = new VisualElement();
            element.style.width = rootWidth * 0.1f;
            element.style.height = rootHeight * 0.1f;
            element.style.left = rootWidth * 0.25f;
            element.style.top = rootHeight * 0.25f;
            element.style.display = DisplayStyle.None;
            panel.Add(element);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // IsOnScreen checks visibility via bounds validation, not visibility attribute
            // but invisible elements may have invalid bounds
            var isOnScreen = element.IsOnScreen();
            // This test documents the actual behavior
            Assert.IsFalse(isOnScreen);
        }

        [UnityTest]
        public IEnumerator VisualElementExtensions_IsOnScreen_ShouldWorkWithNestedElements()
        {
            yield return null; // wait for one frame to ensure UI is initialized

            var rootWidth = m_TestUI.rootVisualElement.layout.width;
            var rootHeight = m_TestUI.rootVisualElement.layout.height;

            var panel = new Panel();
            panel.StretchToParentSize();

            var parent = new VisualElement();
            parent.style.width = rootWidth * 0.4f;
            parent.style.height = rootHeight * 0.4f;
            parent.style.left = rootWidth * 0.1f;
            parent.style.top = rootHeight * 0.1f;
            panel.Add(parent);

            var child = new VisualElement();
            child.style.width = rootWidth * 0.1f;
            child.style.height = rootHeight * 0.1f;
            child.style.left = rootWidth * 0.05f;
            child.style.top = rootHeight * 0.05f;
            parent.Add(child);

            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Child should be on screen
            Assert.IsTrue(child.IsOnScreen());
        }
    }
}
