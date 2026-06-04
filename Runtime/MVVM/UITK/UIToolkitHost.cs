using System;
using UnityEngine.UIElements;

namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// <para>A class used to host an app in a UI panel.</para>
    /// <para>This is a wrapper around a UIDocument (or a root VisualElement) that implements <see cref="IUIToolkitHost"/>.</para>
    /// </summary>
    public sealed class UIToolkitHost : IUIToolkitHost
    {
        UIDocument m_Document;

        VisualElement m_RootVisualElement;

        bool m_Disposed;

        /// <summary>
        /// Creates a new instance of <see cref="UIToolkitHost"/> that hosts an app in the given UIDocument.
        /// </summary>
        /// <param name="uiDocument"> The UIDocument to host the app in. </param>
        public UIToolkitHost(UIDocument uiDocument)
        {
            m_Document = uiDocument;
        }

#if ENABLE_PANEL_RENDERER
        /// <summary>
        /// Creates a new instance of <see cref="UIToolkitHost"/> that hosts an app
        /// using the given root VisualElement directly (e.g. from a <see cref="PanelRenderer"/>).
        /// </summary>
        /// <param name="rootVisualElement"> The root VisualElement to host the app in. </param>
        public UIToolkitHost(VisualElement rootVisualElement)
        {
            m_RootVisualElement = rootVisualElement;
        }
#endif

        VisualElement rootElement => m_RootVisualElement ?? m_Document?.rootVisualElement;

        /// <summary>
        /// <para>Called when the app is being hosted.</para>
        /// <para>A service provider is provided that can be used to resolve services through the host itself.</para>
        /// </summary>
        /// <param name="app"> The app to host. </param>
        /// <param name="serviceProvider"> The service provider to use. </param>
        public void HostApplication(IUIToolkitApp app, IServiceProvider serviceProvider)
        {
            rootElement?.Clear();
            rootElement?.Add(app.rootVisualElement);
        }

        /// <summary>
        /// Tries to find an element of the given type in the visual tree of the app.
        /// </summary>
        /// <param name="element"> The element that was found. </param>
        /// <typeparam name="T"> The type of the element to find. </typeparam>
        /// <returns> True if the element was found, false otherwise. </returns>
        public bool TryFindElement<T>(out T element)
            where T : VisualElement
        {
            element = null;
            if (m_Disposed)
                return false;

            var root = rootElement;
            if (root == null)
                return false;

            element = root.Q<T>();
            return element != null;
        }

        /// <summary>
        /// Disposes of the host.
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Document = null;
            m_RootVisualElement = null;
            m_Disposed = true;
        }
    }
}
