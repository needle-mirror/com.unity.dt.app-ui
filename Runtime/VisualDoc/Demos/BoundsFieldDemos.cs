using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using BoundsField = Unity.AppUI.UI.BoundsField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the BoundsField documentation page.</summary>
    static class BoundsFieldDemos
    {
        [VisualDocDemo("boundsfield")]
        static VisualElement Basic()
        {
            var boundsField = new BoundsField { value = new Bounds(Vector3.zero, new Vector3(1, 1, 1)) };
            boundsField.AddToClassList("demo-field");

            return DemoUtils.Row(boundsField);
        }
    }
}
