using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ColorWheel documentation page.</summary>
    static class ColorWheelDemos
    {
        [VisualDocDemo("colorwheel")]
        static VisualElement Basic()
        {
            var wheel = new ColorWheel
            {
                value = 0.33f,
                saturation = 0.8f,
                brightness = 0.9f,
                opacity = 0.9f
            };
            wheel.AddToClassList("demo-block");
            return DemoUtils.Row(wheel);
        }
    }
}
