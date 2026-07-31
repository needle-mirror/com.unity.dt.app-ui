using UnityEngine.UIElements;
using Unity.AppUI.UI;
using DoubleField = Unity.AppUI.UI.DoubleField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the DoubleField documentation page.</summary>
    static class DoubleFieldDemos
    {
        [VisualDocDemo("doublefield")]
        static VisualElement Basic()
        {
            var field = new DoubleField { value = 3.14, unit = "m" };
            field.AddToClassList("demo-field");

            return DemoUtils.Row(field);
        }
    }
}
