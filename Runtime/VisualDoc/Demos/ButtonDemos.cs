using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Button documentation page.</summary>
    static class ButtonDemos
    {
        [VisualDocDemo("button")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new Button { title = "Default" },
                new Button { title = "Destructive", variant = ButtonVariant.Destructive },
                new Button { title = "Outline", variant = ButtonVariant.Outline },
                new Button { title = "Secondary", variant = ButtonVariant.Secondary },
                new Button { title = "Ghost", variant = ButtonVariant.Ghost },
                new Button { title = "Link", variant = ButtonVariant.Link },
                new Button { title = "With icon", leadingIcon = "info" });
        }
    }
}
