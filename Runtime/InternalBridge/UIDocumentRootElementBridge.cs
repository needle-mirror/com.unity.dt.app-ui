using System;
using UnityEngine.UIElements;

namespace Unity.AppUI.Bridge
{
    static class UIDocumentRootElementBridge
    {
#if APPUI_USE_INTERNAL_API_BRIDGE
        static readonly Type k_UIDocumentRootElementType = typeof(UIDocumentRootElement);
#else
        static readonly Type k_UIDocumentRootElementType =
            typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.UIDocumentRootElement");
#endif

        internal static Type UIDocumentRootElementType => k_UIDocumentRootElementType;
    }
}
