using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Modal documentation page.</summary>
    static class ModalDemos
    {
        [VisualDocDemo("modal")]
        static VisualElement Basic()
        {
            var trigger = new Button { title = "Open Modal" };
            trigger.clicked += () =>
            {
                var content = new VisualElement();
                content.Add(new Text("Are you sure you want to delete this item?"));

                var buttonContainer = new VisualElement();
                var cancelButton = new Button { title = "Cancel", quiet = true };
                var confirmButton = new Button { title = "Delete", variant = ButtonVariant.Destructive };
                buttonContainer.Add(cancelButton);
                buttonContainer.Add(confirmButton);
                content.Add(buttonContainer);

                var modal = Modal.Build(trigger, content).SetOutsideClickDismiss(true);
                cancelButton.clicked += modal.Dismiss;
                confirmButton.clicked += modal.Dismiss;
                modal.Show();
            };

            return DemoUtils.Row(trigger);
        }
    }
}
