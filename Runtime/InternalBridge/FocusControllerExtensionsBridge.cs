using UnityEngine.UIElements;
#if FOCUSABLE_AS_VISUALELEMENT
using Focusable = UnityEngine.UIElements.VisualElement;
#endif

namespace Unity.AppUI.Bridge
{
    static class FocusControllerExtensionsBridge
    {
#if APPUI_USE_INTERNAL_API_BRIDGE

        internal static Focusable FocusNextInDirection(this FocusController controller, Focusable currentFocusable, FocusChangeDirection direction)
        {
            return controller.FocusNextInDirection(currentFocusable, direction);
        }

#else

        static readonly System.Reflection.MethodInfo k_FocusNextInDirection = typeof(FocusController)
            .GetMethod("FocusNextInDirection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        internal static Focusable FocusNextInDirection(this FocusController controller, Focusable currentFocusable, FocusChangeDirection direction)
        {
            return k_FocusNextInDirection.Invoke(controller, new object[] { currentFocusable, direction }) as Focusable;
        }

#endif
    }
}
