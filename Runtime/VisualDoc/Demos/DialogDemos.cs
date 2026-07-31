using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Dialog documentation page.</summary>
    static class DialogDemos
    {
        [VisualDocDemo("dialog")]
        static VisualElement Basic()
        {
            var trigger = new Button { title = "Open dialog" };
            trigger.clicked += () =>
            {
                var dialog = new Dialog
                {
                    title = "Welcome",
                    description = "Welcome to our application! We hope you enjoy using it.",
                    dismissable = true
                };

                Modal.Build(trigger, dialog).Show();
            };

            return DemoUtils.Row(trigger);
        }
    }
}
