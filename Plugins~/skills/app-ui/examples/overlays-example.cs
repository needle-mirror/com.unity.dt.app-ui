// Overlays and Popups Example

using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class OverlaysExample : MonoBehaviour
{
    private Panel m_Panel;

    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        m_Panel = uiDocument.rootVisualElement.Q<Panel>();
    }

    // ========================================================================
    // POPOVER - Displayed next to a target element
    // ========================================================================

    public void ShowPopover(VisualElement target)
    {
        // Create popover content
        var content = new VisualElement();
        content.style.padding = new StyleLength(16);
        content.Add(new Text { text = "Popover content here" });

        var closeBtn = new Button { title = "Close" };

        // Build and show popover
        var popover = Popover.Build(target, content)
            .SetPlacement(PopoverPlacement.Bottom)      // Position relative to target
            .SetShouldFlip(true)                        // Flip if not enough space
            .SetOffset(8)                               // Distance from target
            .SetCrossOffset(0)                          // Offset perpendicular to placement
            .SetArrowVisible(true)                      // Show arrow pointing to target
            .SetContainerPadding(16)                    // Padding from screen edges
            .SetOutsideClickDismiss(true)               // Dismiss on outside click
            .SetKeyboardDismiss(true);                  // Dismiss on Escape key

        closeBtn.clickable.clicked += () => popover.Dismiss();
        content.Add(closeBtn);

        popover.Show();
    }

    // ========================================================================
    // MODAL - Center dialog that blocks interaction
    // ========================================================================

    public void ShowModal()
    {
        // Create modal content
        var dialog = new Dialog
        {
            title = "Confirm Action",
            description = "Are you sure you want to proceed?"
        };

        // Add action buttons to footer
        var cancelBtn = new Button { title = "Cancel" };
        var confirmBtn = new Button { title = "Confirm", primary = true };

        dialog.footer.Add(cancelBtn);
        dialog.footer.Add(confirmBtn);

        // Build modal
        var modal = Modal.Build(m_Panel, dialog)
            .SetFullScreenMode(ModalFullScreenMode.None)    // None, Mobile, Always
            .SetOutsideClickDismiss(false)                  // Don't dismiss on outside click
            .SetKeyboardDismiss(true);                      // Allow Escape to dismiss

        cancelBtn.clickable.clicked += () => modal.Dismiss();
        confirmBtn.clickable.clicked += () =>
        {
            Debug.Log("Confirmed!");
            modal.Dismiss();
        };

        modal.Show();
    }

    // ========================================================================
    // ALERT DIALOG - Styled dialog with semantic meaning
    // ========================================================================

    public void ShowAlertDialog()
    {
        var alertDialog = new AlertDialog
        {
            title = "Warning",
            description = "This action cannot be undone.",
            variant = AlertSemantic.Warning  // Info, Warning, Error, Success
        };

        var okBtn = new Button { title = "OK", primary = true };
        alertDialog.footer.Add(okBtn);

        var modal = Modal.Build(m_Panel, alertDialog);

        okBtn.clickable.clicked += () => modal.Dismiss();

        modal.Show();
    }

    // ========================================================================
    // MENU - Context menu with items and submenus
    // ========================================================================

    public void ShowMenu(VisualElement anchor)
    {
        MenuBuilder.Build(anchor)
            .AddAction(1, "Cut", "cut", evt => Debug.Log("Cut"))
            .AddAction(2, "Copy", "copy", evt => Debug.Log("Copy"))
            .AddAction(3, "Paste", "paste", evt => Debug.Log("Paste"))
            .AddSeparator()
            .PushSubMenu(4, "More Options", "more")
                .AddAction(5, "Option A", null, evt => Debug.Log("Option A"))
                .AddAction(6, "Option B", null, evt => Debug.Log("Option B"))
                .PushSubMenu(7, "Even More", null)
                    .AddAction(8, "Deep Option", null, evt => Debug.Log("Deep"))
                .Pop()
            .Pop()
            .AddSeparator()
            .AddAction(9, "Delete", "delete", evt => Debug.Log("Delete"))
            .Show();
    }

    // ========================================================================
    // TRAY - Bottom sheet for mobile devices
    // ========================================================================

    public void ShowTray()
    {
        var content = new VisualElement();
        content.style.padding = new StyleLength(16);

        var heading = new Heading { text = "Select Option" };
        content.Add(heading);

        var option1 = new Button { title = "Option 1" };
        var option2 = new Button { title = "Option 2" };
        var option3 = new Button { title = "Option 3" };
        var cancelBtn = new Button { title = "Cancel" };

        content.Add(option1);
        content.Add(option2);
        content.Add(option3);
        content.Add(cancelBtn);

        var tray = Tray.Build(m_Panel, content)
            .SetOutsideClickDismiss(true)
            .SetKeyboardDismiss(true);

        option1.clickable.clicked += () => { Debug.Log("Option 1"); tray.Dismiss(); };
        option2.clickable.clicked += () => { Debug.Log("Option 2"); tray.Dismiss(); };
        option3.clickable.clicked += () => { Debug.Log("Option 3"); tray.Dismiss(); };
        cancelBtn.clickable.clicked += () => tray.Dismiss();

        tray.Show();
    }

    // ========================================================================
    // TOAST - Notification message
    // ========================================================================

    public void ShowToast()
    {
        // Simple toast
        Toast.Build(m_Panel, "Operation completed successfully!", NotificationDuration.Short)
            .SetStyle(NotificationStyle.Positive)
            .Show();
    }

    public void ShowToastWithAction()
    {
        // Toast with action button
        var toast = Toast.Build(m_Panel, "Item deleted", NotificationDuration.Long)
            .SetStyle(NotificationStyle.Default);

        toast.AddAction("undo", "Undo", toastRef =>
        {
            Debug.Log("Undo clicked!");
            toastRef.Dismiss();
        });

        toast.Show();
    }

    public void ShowToastVariants()
    {
        // Different toast styles
        Toast.Build(m_Panel, "Info message", NotificationDuration.Short)
            .SetStyle(NotificationStyle.Default)
            .Show();

        // Use coroutine or delay for sequential toasts
        // Toast.Build(m_Panel, "Success!", NotificationDuration.Short)
        //     .SetStyle(NotificationStyle.Positive)
        //     .Show();

        // Toast.Build(m_Panel, "Warning!", NotificationDuration.Short)
        //     .SetStyle(NotificationStyle.Warning)
        //     .Show();

        // Toast.Build(m_Panel, "Error!", NotificationDuration.Short)
        //     .SetStyle(NotificationStyle.Negative)
        //     .Show();
    }

    // ========================================================================
    // TOOLTIP - Hover information
    // ========================================================================

    public void SetupTooltip(VisualElement target)
    {
        // Tooltips are typically set via USS or the tooltip property
        target.tooltip = "This is a helpful tooltip";

        // Or programmatically with the Tooltip component
        var tooltipManipulator = new TooltipManipulator();
        target.AddManipulator(tooltipManipulator);
    }

    // ========================================================================
    // CUSTOM POPUP CONTENT WITH DISMISS
    // ========================================================================

    public void ShowCustomPopup()
    {
        var content = new CustomPopupContent();

        var modal = Modal.Build(m_Panel, content);

        // Content can request dismiss via IDismissInvocator
        content.dismissRequested += type => modal.Dismiss();

        modal.Show();
    }
}

// Custom content that can dismiss its parent popup
public class CustomPopupContent : VisualElement, IDismissInvocator
{
    public event System.Action<DismissType> dismissRequested;

    public CustomPopupContent()
    {
        style.padding = new StyleLength(16);

        var heading = new Heading { text = "Custom Content" };
        Add(heading);

        var text = new Text { text = "This popup can dismiss itself." };
        Add(text);

        var closeBtn = new Button { title = "Close" };
        closeBtn.clickable.clicked += () => dismissRequested?.Invoke(DismissType.Manual);
        Add(closeBtn);
    }
}
