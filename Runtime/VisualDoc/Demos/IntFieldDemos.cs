using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the IntField documentation page.</summary>
    static class IntFieldDemos
    {
        [VisualDocDemo("intfield")]
        static VisualElement Basic()
        {
            var basic = new IntField { value = 42 };
            basic.AddToClassList("demo-field");

            var withUnit = new IntField { value = 20, unit = "C" };
            withUnit.AddToClassList("demo-field");

            var withRange = new IntField { value = 5, lowValue = 0, highValue = 10 };
            withRange.AddToClassList("demo-field");

            return DemoUtils.Row(basic, withUnit, withRange);
        }
    }
}
