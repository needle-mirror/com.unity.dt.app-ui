using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// <para>A MonoBehaviour that can be used to build and host an app in a UIDocument
    /// or a <see cref="PanelRenderer"/> (Unity 6.5+).</para>
    /// <para>This class is intended to be used as a base class for a MonoBehaviour
    /// that is attached to a GameObject in a scene.</para>
    /// </summary>
    /// <typeparam name="T"> The type of the app to build. It is expected that this type is a subclass of <see cref="App"/>. </typeparam>
    public class UIToolkitAppBuilder<T> : MonoBehaviour where T : App
    {
        /// <summary>
        /// The UIDocument to host the app in.
        /// </summary>
        [Tooltip("The UIDocument to host the app in.")]
        public UIDocument uiDocument;

#if ENABLE_PANEL_RENDERER
        /// <summary>
        /// The PanelRenderer to host the app in (Unity 6.5+).
        /// When set, this takes priority over <see cref="uiDocument"/>.
        /// </summary>
        [Tooltip("The PanelRenderer to host the app in (Unity 6.5+). Takes priority over UIDocument when set.")]
        public PanelRenderer panelRenderer;
#endif

        T m_App;

#if ENABLE_PANEL_RENDERER
        int m_LastPanelVersion = -1;
#endif

        void OnEnable()
        {
#if ENABLE_PANEL_RENDERER
            if (panelRenderer)
            {
                panelRenderer.RegisterUIReloadCallback(OnPanelReloadVersioned);
                return;
            }
#endif

            if (!uiDocument)
            {
                Debug.LogWarning("No UIDocument assigned to Program component. Aborting App startup.");
                return;
            }

            var builder = AppBuilder.InstantiateWith<T, UIToolkitHost>();
            OnConfiguringApp(builder);

            var host = new UIToolkitHost(uiDocument);
            m_App = (T)builder.BuildWith(host);
            OnAppInitialized(m_App);
        }

#if ENABLE_PANEL_RENDERER
        void OnPanelReloadVersioned(PanelRenderer renderer, VisualElement root, int version)
        {
            if (version == m_LastPanelVersion)
                return;

            m_LastPanelVersion = version;

            ShutdownApp();

            var builder = AppBuilder.InstantiateWith<T, UIToolkitHost>();
            OnConfiguringApp(builder);

            var host = new UIToolkitHost(root);
            m_App = (T)builder.BuildWith(host);
            OnAppInitialized(m_App);
        }
#endif

        /// <summary>
        /// Called when the app builder is being configured.
        /// </summary>
        /// <param name="builder"> The app builder. </param>
        protected virtual void OnConfiguringApp(AppBuilder builder)
        {

        }

        /// <summary>
        /// Called when the app has been initialized.
        /// </summary>
        /// <param name="app"> The app that was initialized. </param>
        protected virtual void OnAppInitialized(T app)
        {

        }

        /// <summary>
        /// Called when the app is shutting down.
        /// </summary>
        /// <param name="app"> The app that is shutting down. </param>
        protected virtual void OnAppShuttingDown(T app)
        {

        }

        void OnDisable()
        {
#if ENABLE_PANEL_RENDERER
            if (panelRenderer)
                panelRenderer.UnregisterUIReloadCallback(OnPanelReloadVersioned);
#endif

            ShutdownApp();
        }

        void ShutdownApp()
        {
            if (m_App == null)
                return;

            OnAppShuttingDown(m_App);
            if (uiDocument)
                uiDocument.rootVisualElement?.Clear();
            m_App.Shutdown();
            m_App.Dispose();
            m_App = null;
        }
    }
}
