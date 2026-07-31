using UnityEngine.UIElements;
using Unity.AppUI.UI;
using SliderInt = Unity.AppUI.UI.SliderInt;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the SliderInt documentation page.</summary>
    static class SliderIntDemos
    {
        [VisualDocDemo("sliderint")]
        static VisualElement Basic()
        {
            var slider = new SliderInt { lowValue = 0, highValue = 100, value = 50 };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }

        [VisualDocDemo("sliderint", order = 1)]
        static VisualElement Marks()
        {
            var slider = new SliderInt
            {
                lowValue = -50,
                highValue = 50,
                step = 5,
                value = 0,
                showMarks = true
            };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
