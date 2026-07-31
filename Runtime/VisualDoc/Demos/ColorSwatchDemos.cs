using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ColorSwatch documentation page.</summary>
    static class ColorSwatchDemos
    {
        [VisualDocDemo("colorswatch")]
        static VisualElement Variants()
        {
            var red = new ColorSwatch { size = Size.M, color = Color.red };
            var greenRound = new ColorSwatch { size = Size.M, color = Color.green, round = true };

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
            var gradientSwatch = new ColorSwatch { size = Size.M, value = gradient };

            return DemoUtils.Row(red, greenRound, gradientSwatch);
        }
    }
}
