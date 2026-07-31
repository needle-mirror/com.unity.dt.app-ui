using UnityEngine.UIElements;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Toast documentation page.</summary>
    static class ToastDemos
    {
        [VisualDocDemo("toast")]
        static VisualElement Variants()
        {
            var successButton = new Button { title = "Success" };
            successButton.clicked += () =>
            {
                Toast.Build(successButton, "File saved successfully!", NotificationDuration.Short)
                    .SetStyle(NotificationStyle.Positive)
                    .SetIcon("check")
                    .Show();
            };

            var errorButton = new Button { title = "Error" };
            errorButton.clicked += () =>
            {
                Toast.Build(errorButton, "Failed to connect to server", NotificationDuration.Short)
                    .SetStyle(NotificationStyle.Negative)
                    .SetIcon("warning")
                    .Show();
            };

            var actionButton = new Button { title = "With action" };
            actionButton.clicked += () =>
            {
                Toast.Build(actionButton, "Item deleted", NotificationDuration.Long)
                    .SetStyle(NotificationStyle.Default)
                    .SetIcon("trash")
                    .AddAction(1, "Undo", toast => toast.Dismiss())
                    .Show();
            };

            return DemoUtils.Row(successButton, errorButton, actionButton);
        }
    }
}
