using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the MenuDivider documentation page.</summary>
    static class MenuDividerDemos
    {
        [VisualDocDemo("menu-divider")]
        static VisualElement Basic()
        {
            // MenuDivider is only meaningful inside a Menu, and Menu renders standalone
            // (no popover host required), so it can be shown inline here.
            var menu = new Menu();
            menu.Add(new MenuItem { label = "Cut", icon = "copy" });
            menu.Add(new MenuItem { label = "Copy", icon = "copy" });
            menu.Add(new MenuItem { label = "Paste", icon = "copy" });
            menu.Add(new MenuDivider());
            menu.Add(new MenuItem { label = "Delete", icon = "trash" });

            return DemoUtils.Row(menu);
        }
    }
}
