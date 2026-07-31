using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Divider documentation page.</summary>
    static class DividerDemos
    {
        [VisualDocDemo("divider")]
        static VisualElement Orientations()
        {
            return DemoUtils.Row(
                new Text { text = "Left content" },
                new Divider { direction = Direction.Vertical, size = Size.M },
                new Text { text = "Right content" });
        }
    }
}
