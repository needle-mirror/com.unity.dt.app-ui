using UnityEngine.UIElements;

namespace Unity.AppUI.Navigation
{
    /// <summary>
    /// Extension methods for VisualElement.
    /// </summary>
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Find the NavigationController used by the NavigationContainer that contains this visual element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static NavController FindNavController(this VisualElement element)
        {
            return element?.GetFirstAncestorOfType<NavHost>()?.navController;
        }
    }
}
