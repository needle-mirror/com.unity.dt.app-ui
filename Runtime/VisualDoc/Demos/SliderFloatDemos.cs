using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the SliderFloat documentation page.</summary>
    static class SliderFloatDemos
    {
        [VisualDocDemo("sliderfloat")]
        static VisualElement Basic()
        {
            var slider = new SliderFloat { lowValue = 0, highValue = 100, value = 42 };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
