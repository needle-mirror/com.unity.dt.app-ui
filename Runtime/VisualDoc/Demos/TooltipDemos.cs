using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Tooltip documentation page.</summary>
    static class TooltipDemos
    {
        [VisualDocDemo("tooltip")]
        static VisualElement Basic()
        {
            var saveButton = new Button { title = "Save", leadingIcon = "download" };
            Tooltip.Build(saveButton).SetText("Save your changes to the current document");

            var settingsButton = new IconButton { icon = "gear" };
            Tooltip.Build(settingsButton)
                .SetText("Open application settings")
                .SetPlacement(PopoverPlacement.Top);

            return DemoUtils.Row(saveButton, settingsButton);
        }
    }
}
