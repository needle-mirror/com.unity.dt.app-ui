using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the AlertDialog documentation page.</summary>
    static class AlertDialogDemos
    {
        [VisualDocDemo("alertdialog")]
        static VisualElement Basic()
        {
            var trigger = new Button { title = "Show Alert Dialog" };
            trigger.clicked += () =>
            {
                var alertDialog = new AlertDialog
                {
                    variant = AlertSemantic.Warning,
                    title = "Unsaved Changes",
                    description = "You have unsaved changes. Do you want to save them before leaving?"
                };
                alertDialog.SetPrimaryAction(1, "Save", null);
                alertDialog.SetSecondaryAction(2, "Don't Save", null);
                alertDialog.SetCancelAction(3, "Cancel");

                Modal.Build(trigger, alertDialog)
                    .SetKeyboardDismiss(true)
                    .SetOutsideClickDismiss(false)
                    .Show();
            };

            return DemoUtils.Row(trigger);
        }
    }
}
