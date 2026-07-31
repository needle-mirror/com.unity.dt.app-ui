using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the RangeSliderFloat documentation page.</summary>
    static class RangeSliderFloatDemos
    {
        [VisualDocDemo("rangesliderfloat")]
        static VisualElement Basic()
        {
            var slider = new RangeSliderFloat
            {
                lowValue = 0,
                highValue = 100,
                value = new Vector2(25f, 75f),
                showMarks = true,
                track = TrackDisplayType.On,
                displayValueLabel = ValueDisplayMode.Auto
            };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
