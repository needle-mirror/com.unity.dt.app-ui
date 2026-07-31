using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Popover documentation page.</summary>
    static class PopoverDemos
    {
        [VisualDocDemo("popover")]
        static VisualElement Basic()
        {
            var button = new Button { title = "Show Menu" };
            button.clicked += () =>
            {
                var menuContent = new VisualElement();
                menuContent.Add(new MenuItem { label = "Edit", icon = "pencil" });
                menuContent.Add(new MenuItem { label = "Delete", icon = "trash" });
                menuContent.Add(new MenuItem { label = "Share", icon = "link" });

                var popover = Popover.Build(button, menuContent)
                    .SetPlacement(PopoverPlacement.Bottom)
                    .SetOffset(8);
                popover.Show();
            };

            return DemoUtils.Row(button);
        }
    }
}
