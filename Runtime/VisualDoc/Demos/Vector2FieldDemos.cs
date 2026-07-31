using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Vector2Field = Unity.AppUI.UI.Vector2Field;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Vector2Field documentation page.</summary>
    static class Vector2FieldDemos
    {
        [VisualDocDemo("vector2field")]
        static VisualElement Basic()
        {
            var field = new Vector2Field { value = new Vector2(1, 2) };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
