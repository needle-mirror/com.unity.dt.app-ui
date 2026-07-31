using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Tray documentation page.</summary>
    static class TrayDemos
    {
        [VisualDocDemo("tray")]
        static VisualElement Basic()
        {
            var actionButton = new Button { title = "More Actions" };

            actionButton.clicked += () =>
            {
                var actionSheet = DemoUtils.Column(
                    new Text { text = "Choose an action", size = TextSize.S },
                    new Button { title = "Share", leadingIcon = "link" },
                    new Button { title = "Delete", leadingIcon = "trash", variant = ButtonVariant.Destructive });

                Tray.Build(actionButton, actionSheet)
                    .SetPosition(TrayPosition.Bottom)
                    .SetHandleVisible(true)
                    .Show();
            };

            return DemoUtils.Row(actionButton);
        }
    }
}
