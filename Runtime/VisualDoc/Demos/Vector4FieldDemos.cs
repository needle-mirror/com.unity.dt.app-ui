using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Vector4Field = Unity.AppUI.UI.Vector4Field;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Vector4Field documentation page.</summary>
    static class Vector4FieldDemos
    {
        [VisualDocDemo("vector4field")]
        static VisualElement Basic()
        {
            var field = new Vector4Field { value = new Vector4(1, 2, 3, 4), formatString = "F2" };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
