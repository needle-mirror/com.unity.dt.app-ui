using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using RectIntField = Unity.AppUI.UI.RectIntField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the RectIntField documentation page.</summary>
    static class RectIntFieldDemos
    {
        [VisualDocDemo("rectintfield")]
        static VisualElement Basic()
        {
            var field = new RectIntField { value = new RectInt(0, 0, 100, 100) };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
