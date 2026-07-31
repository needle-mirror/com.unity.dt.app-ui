using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the SVSquare documentation page.</summary>
    static class SVSquareDemos
    {
        [VisualDocDemo("svsquare")]
        static VisualElement Basic()
        {
            var square = new SVSquare { referenceHue = 0.33f, saturation = 0.8f, brightness = 0.9f };
            square.AddToClassList("demo-block");
            return DemoUtils.Row(square);
        }
    }
}
