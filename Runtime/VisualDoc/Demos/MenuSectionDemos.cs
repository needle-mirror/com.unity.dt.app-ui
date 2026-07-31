using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Menu Section documentation page.</summary>
    static class MenuSectionDemos
    {
        [VisualDocDemo("menu-section")]
        static VisualElement Basic()
        {
            var editSection = new MenuSection { title = "Edit" };
            editSection.Add(new MenuItem { label = "Undo", shortcut = "Ctrl+Z" });
            editSection.Add(new MenuItem { label = "Redo", shortcut = "Ctrl+Y" });

            var viewSection = new MenuSection { title = "View" };
            viewSection.Add(new MenuItem { label = "Zoom In", shortcut = "Ctrl++" });
            viewSection.Add(new MenuItem { label = "Zoom Out", shortcut = "Ctrl+-" });

            var menu = new Menu();
            menu.AddToClassList("demo-block");
            menu.Add(editSection);
            menu.Add(new MenuDivider());
            menu.Add(viewSection);

            return DemoUtils.Row(menu);
        }
    }
}
