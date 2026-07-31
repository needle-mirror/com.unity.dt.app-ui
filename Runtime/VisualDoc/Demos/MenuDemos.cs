using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Menu documentation page.</summary>
    static class MenuDemos
    {
        [VisualDocDemo("menu")]
        static VisualElement Basic()
        {
            // Menu renders standalone (its ScrollView container needs no popover host to display),
            // so it can be added directly to the page, mirroring the component's own
            // "Creating a Menu programmatically" XmlDoc example.
            var menu = new Menu();
            menu.Add(new MenuItem { label = "New File", icon = "plus" });
            menu.Add(new MenuItem { label = "Open", icon = "folder" });
            menu.Add(new MenuItem { label = "Save", icon = "download" });
            menu.Add(new MenuDivider());
            menu.Add(new MenuItem { label = "Show Grid", selectable = true, value = true });
            menu.Add(new MenuItem { label = "Exit", icon = "x" });

            return DemoUtils.Row(menu);
        }
    }
}
