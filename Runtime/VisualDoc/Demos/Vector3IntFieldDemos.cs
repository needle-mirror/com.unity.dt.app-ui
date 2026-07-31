using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Vector3IntField = Unity.AppUI.UI.Vector3IntField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Vector3IntField documentation page.</summary>
    static class Vector3IntFieldDemos
    {
        [VisualDocDemo("vector3intfield")]
        static VisualElement Basic()
        {
            var field = new Vector3IntField { value = new Vector3Int(100, 200, 300) };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
