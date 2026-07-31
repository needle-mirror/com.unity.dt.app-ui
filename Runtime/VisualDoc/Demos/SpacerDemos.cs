using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Spacer documentation page.</summary>
    static class SpacerDemos
    {
        [VisualDocDemo("spacer")]
        static VisualElement Basic()
        {
            return DemoUtils.Row(
                new Button { title = "Left" },
                new Spacer { spacing = SpacerSpacing.M },
                new Button { title = "Center" },
                new Spacer { spacing = SpacerSpacing.M },
                new Button { title = "Right" });
        }

        [VisualDocDemo("spacer", order = 1)]
        static VisualElement Expand()
        {
            return DemoUtils.Row(
                new Button { title = "Left-aligned" },
                new Spacer { spacing = SpacerSpacing.Expand },
                new Button { title = "Right-aligned" });
        }
    }
}
