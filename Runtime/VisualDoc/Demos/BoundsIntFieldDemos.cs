using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using BoundsIntField = Unity.AppUI.UI.BoundsIntField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the BoundsIntField documentation page.</summary>
    static class BoundsIntFieldDemos
    {
        [VisualDocDemo("boundsintfield")]
        static VisualElement Basic()
        {
            var field = new BoundsIntField
            {
                value = new BoundsInt(Vector3Int.zero, new Vector3Int(1, 1, 1))
            };
            field.AddToClassList("demo-field");
            return DemoUtils.Row(field);
        }
    }
}
