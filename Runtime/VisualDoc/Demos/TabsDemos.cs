using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Tabs documentation page.</summary>
    static class TabsDemos
    {
        [VisualDocDemo("tabs")]
        static VisualElement Basic()
        {
            var tabs = new Tabs();
            tabs.Add(new TabItem { label = "Tab 1", icon = "info" });
            tabs.Add(new TabItem { label = "Tab 2", icon = "info" });
            tabs.Add(new TabItem { label = "Tab 3", icon = "info" });
            return DemoUtils.Row(tabs);
        }

        [VisualDocDemo("tabs", order = 1)]
        static VisualElement Emphasized()
        {
            var tabs = new Tabs { emphasized = true, justified = true };
            tabs.Add(new TabItem { label = "Tab 1" });
            tabs.Add(new TabItem { label = "Tab 2" });
            tabs.Add(new TabItem { label = "Tab 3" });
            return DemoUtils.Row(tabs);
        }
    }
}
