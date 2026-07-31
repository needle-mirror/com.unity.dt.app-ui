using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Toolbar documentation page.</summary>
    static class ToolbarDemos
    {
        [VisualDocDemo("toolbar")]
        static VisualElement Basic()
        {
            var toolbar = new Toolbar { dockMode = ToolbarDockMode.Floating, direction = Direction.Horizontal };
            toolbar.Add(new ActionButton { icon = "house", quiet = true });
            toolbar.Add(new ActionButton { icon = "magnifying-glass", quiet = true });
            toolbar.Add(new Divider { direction = Direction.Vertical });
            toolbar.Add(new ActionButton { icon = "gear", quiet = true });

            return DemoUtils.Row(toolbar);
        }
    }
}
