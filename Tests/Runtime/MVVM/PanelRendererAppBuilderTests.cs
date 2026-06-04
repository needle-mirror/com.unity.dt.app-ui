#if ENABLE_PANEL_RENDERER
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.MVVM;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.MVVM
{
    [TestFixture]
    [TestOf(typeof(UIToolkitAppBuilder<>))]
    public class UIToolkitAppBuilderPanelRendererTests
    {
        PanelRenderer m_PanelRenderer;

        [SetUp]
        public void SetUp()
        {
            TearDown();
            m_PanelRenderer = Utils.ConstructTestUIPanelRenderer();
        }

        [TearDown]
        public void TearDown()
        {
            if (App.current != null)
            {
                App.current.Shutdown();
                App.current.Dispose();
            }

            if (m_PanelRenderer && m_PanelRenderer.gameObject)
                Object.Destroy(m_PanelRenderer.gameObject);
        }

        [UnityTest]
        public IEnumerator CanSetUpUITKAppWithPanelRenderer()
        {
            yield return null;

            m_PanelRenderer.gameObject.SetActive(false);
            var builder = m_PanelRenderer.gameObject.AddComponent<TestableUITKAppWithPanelRenderer>();
            builder.panelRenderer = m_PanelRenderer;

            Assert.IsNotNull(builder);
            m_PanelRenderer.gameObject.SetActive(true);

            yield return null;

            builder.enabled = false;

            yield return null;
        }

        [UnityTest]
        public IEnumerator UIToolkitHost_AcceptsRawVisualElement()
        {
            yield return null;

            var root = new VisualElement();
            var host = new UIToolkitHost(root);

            Assert.IsNotNull(host);
            Assert.IsTrue(host.TryFindElement<VisualElement>(out _));

            host.Dispose();

            Assert.IsFalse(host.TryFindElement<VisualElement>(out _));
        }
    }

    class TestableUITKAppWithPanelRenderer : UIToolkitAppBuilder<TestableUITKAppWithPanelRenderer.TestApp>
    {
        internal class TestApp : App
        {
            public TestApp(MainPage mainPage)
            {
                var panel = new AppUI.UI.Panel();
                panel.Add(mainPage);
                this.rootVisualElement = panel;
            }
        }

        internal class MainPage : VisualElement
        {
            public MainPage() { }
        }

        protected override void OnConfiguringApp(AppBuilder builder)
        {
            base.OnConfiguringApp(builder);
            builder.services.AddSingleton<MainPage>();
        }
    }
}
#endif
