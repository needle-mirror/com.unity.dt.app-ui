using System.Reflection;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Extension methods for FocusController.
    /// </summary>
    public static class FocusControllerExtensions
    {
        static readonly MethodInfo k_FocusNextInDirection = typeof(FocusController).GetMethod("FocusNextInDirection",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Focus the next element in the given direction.
        /// </summary>
        /// <param name="controller">The focus controller.</param>
        /// <param name="direction">The direction to focus.</param>
        /// <returns>The focused element.</returns>
        public static Focusable FocusNextInDirectionEx(this FocusController controller, FocusChangeDirection direction)
        {
            if (controller == null)
                return null;

#if UNITY_2023_2_OR_NEWER
            var r = FocusNextInDirectionEx(controller, controller.focusedElement, direction);
#else 
            var r = k_FocusNextInDirection!.Invoke(controller, new object[] { direction });
#endif
            return r as Focusable;
        }

        /// <summary>
        /// Focus the next element in the given direction.
        /// </summary>
        /// <param name="controller">The focus controller.</param>
        /// <param name="currentlyFocusedElement">The currently focused element.</param>
        /// <param name="direction">The direction to focus.</param>
        /// <returns>The focused element.</returns>
        public static Focusable FocusNextInDirectionEx(this FocusController controller,
            Focusable currentlyFocusedElement, FocusChangeDirection direction)
        {
            if (controller == null)
                return null;

#if UNITY_2023_2_OR_NEWER
            var r = k_FocusNextInDirection!.Invoke(controller, new object[] { currentlyFocusedElement, direction });
#else 
            var r = controller.FocusNextInDirectionEx(direction);
#endif
            return r as Focusable;
        }
    }
}
