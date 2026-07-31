using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the RangeSliderInt documentation page.</summary>
    static class RangeSliderIntDemos
    {
        [VisualDocDemo("rangesliderint")]
        static VisualElement Basic()
        {
            var slider = new RangeSliderInt
            {
                lowValue = 0,
                highValue = 100,
                value = new Vector2Int(25, 75),
                showMarks = true,
                track = TrackDisplayType.On,
                displayValueLabel = ValueDisplayMode.Auto
            };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
