using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using RectField = Unity.AppUI.UI.RectField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the RectField documentation page.</summary>
    static class RectFieldDemos
    {
        [VisualDocDemo("rectfield")]
        static VisualElement Basic()
        {
            var field = new RectField { value = new Rect(0, 0, 100, 100) };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
