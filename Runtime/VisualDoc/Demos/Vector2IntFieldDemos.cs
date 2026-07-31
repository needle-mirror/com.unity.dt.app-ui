using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Vector2IntField = Unity.AppUI.UI.Vector2IntField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Vector2IntField documentation page.</summary>
    static class Vector2IntFieldDemos
    {
        [VisualDocDemo("vector2intfield")]
        static VisualElement Basic()
        {
            var field = new Vector2IntField { value = new Vector2Int(1920, 1080) };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
