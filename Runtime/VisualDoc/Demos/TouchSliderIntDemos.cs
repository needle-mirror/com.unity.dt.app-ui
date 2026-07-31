using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the TouchSliderInt documentation page.</summary>
    static class TouchSliderIntDemos
    {
        [VisualDocDemo("touchsliderint")]
        static VisualElement Basic()
        {
            var slider = new TouchSliderInt
            {
                label = "%",
                lowValue = 0,
                highValue = 100,
                value = 50,
                step = 5,
                shiftStep = 10
            };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
