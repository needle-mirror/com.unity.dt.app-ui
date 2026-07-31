using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ColorPicker documentation page.</summary>
    static class ColorPickerDemos
    {
        [VisualDocDemo("colorpicker")]
        static VisualElement Basic()
        {
            var picker = new ColorPicker
            {
                showAlpha = true,
                showToolbar = true,
                showHex = true,
                value = Color.blue
            };
            return DemoUtils.Row(picker);
        }
    }
}
