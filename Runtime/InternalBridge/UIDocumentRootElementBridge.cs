using System;
using UnityEngine.UIElements;

namespace Unity.AppUI.Bridge
{
    static class UIDocumentRootElementBridge
    {
#if APPUI_USE_INTERNAL_API_BRIDGE
        static readonly Type k_UIDocumentRootElementType = typeof(UIDocumentRootElement);
#else // REFLECTION
        static readonly Type k_UIDocumentRootElementType =
#if UI_DOCUMENT_ROOT_ELEMENT_TYPE_EXISTS
            typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.UIDocumentRootElement");
#else
            null;
#endif
#endif // APPUI_USE_INTERNAL_API_BRIDGE

        internal static Type UIDocumentRootElementType => k_UIDocumentRootElementType;
    }
}
