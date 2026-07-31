using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ColorSlider documentation page.</summary>
    static class ColorSliderDemos
    {
        [VisualDocDemo("colorslider")]
        static VisualElement Basic()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.red, 0),
                    new GradientColorKey(Color.blue, 1)
                },
                new[]
                {
                    new GradientAlphaKey(1, 0),
                    new GradientAlphaKey(1, 1)
                });

            var slider = new ColorSlider { colorRange = gradient, value = 0.5f };
            slider.AddToClassList("demo-field");
            return DemoUtils.Row(slider);
        }
    }
}
