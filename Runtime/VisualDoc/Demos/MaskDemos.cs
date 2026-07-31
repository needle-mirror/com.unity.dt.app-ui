using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Mask documentation page.</summary>
    static class MaskDemos
    {
        [VisualDocDemo("mask")]
        static VisualElement Basic()
        {
            var mask = new Mask
            {
                innerMaskColor = Color.clear,
                outerMaskColor = new Color(0, 0, 0, 0.7f),
                maskRect = new Rect(20f, 20f, 100f, 60f),
                radius = 12f,
                blur = 8f,
            };
            mask.AddToClassList("demo-block");
            return DemoUtils.Row(mask);
        }
    }
}
