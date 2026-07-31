using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Vector3Field = Unity.AppUI.UI.Vector3Field;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Vector3Field documentation page.</summary>
    static class Vector3FieldDemos
    {
        [VisualDocDemo("vector3field")]
        static VisualElement Basic()
        {
            var field = new Vector3Field { value = new Vector3(1, 2, 3) };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
