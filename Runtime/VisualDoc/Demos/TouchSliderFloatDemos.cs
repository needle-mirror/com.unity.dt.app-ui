using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the TouchSliderFloat documentation page.</summary>
    static class TouchSliderFloatDemos
    {
        [VisualDocDemo("touchsliderfloat")]
        static VisualElement Basic()
        {
            var slider = new TouchSliderFloat
            {
                label = "TEMPERATURE",
                lowValue = -100,
                highValue = 100,
                value = 0,
                formatString = "#0.0°C"
            };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
