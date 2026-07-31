using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ColorField documentation page.</summary>
    static class ColorFieldDemos
    {
        [VisualDocDemo("colorfield")]
        static VisualElement Variants()
        {
            var basic = new ColorField { value = Color.blue };
            basic.AddToClassList("demo-field");

            var swatchOnly = new ColorField { value = Color.red, swatchOnly = true };
            swatchOnly.AddToClassList("demo-field");

            var noAlpha = new ColorField { value = Color.green, showAlpha = false };
            noAlpha.AddToClassList("demo-field");

            return DemoUtils.Row(basic, swatchOnly, noAlpha);
        }
    }
}
