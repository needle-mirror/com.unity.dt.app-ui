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
                new Button { title = "Accent", variant = ButtonVariant.Accent },
                new Button { title = "Destructive", variant = ButtonVariant.Destructive },
                new Button { title = "Quiet", quiet = true },
                new Button { title = "With icon", leadingIcon = "info" });
        }
    }
}
