using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Menu Item documentation page.</summary>
    static class MenuItemDemos
    {
        [VisualDocDemo("menu-item")]
        static VisualElement Variants()
        {
            var subMenu = new Menu();
            subMenu.Add(new MenuItem { label = "Zoom In", shortcut = "Ctrl++" });
            subMenu.Add(new MenuItem { label = "Zoom Out", shortcut = "Ctrl+-" });

            var disabledItem = new MenuItem { label = "Disabled Item" };
            disabledItem.SetEnabled(false);

            var menu = new Menu();
            menu.AddToClassList("demo-block");
            menu.Add(new MenuItem { label = "New File", icon = "plus", shortcut = "Ctrl+N" });
            menu.Add(new MenuItem { label = "Show Grid", selectable = true, value = true });
            menu.Add(new MenuItem { label = "View", icon = "gear", subMenu = subMenu });
            menu.Add(disabledItem);

            return DemoUtils.Row(menu);
        }
    }
}
