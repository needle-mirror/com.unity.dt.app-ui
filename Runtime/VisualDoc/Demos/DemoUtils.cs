using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>
    /// Shared helpers for the live examples displayed on component documentation pages.
    /// </summary>
    static class DemoUtils
    {
        /// <summary>
        /// A horizontal row of demo elements (uses the "example-row" USS class).
        /// </summary>
        public static VisualElement Row(params VisualElement[] children)
        {
            return Container("example-row", children);
        }

        /// <summary>
        /// A vertical stack of demo elements (uses the "example-column" USS class).
        /// </summary>
        public static VisualElement Column(params VisualElement[] children)
        {
            return Container("example-column", children);
        }

        static VisualElement Container(string className, VisualElement[] children)
        {
            var container = new VisualElement();
            container.AddToClassList(className);
            foreach (var child in children)
                container.Add(child);
            return container;
        }
    }
}
