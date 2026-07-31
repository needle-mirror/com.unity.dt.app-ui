using UnityEngine.UIElements;
using Unity.AppUI.UI;
using LongField = Unity.AppUI.UI.LongField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the LongField documentation page.</summary>
    static class LongFieldDemos
    {
        [VisualDocDemo("longfield")]
        static VisualElement Basic()
        {
            var basic = new LongField { value = 1234567890L };
            basic.AddToClassList("demo-field");

            var withRange = new LongField { value = 50L, lowValue = 0L, highValue = 100L };
            withRange.AddToClassList("demo-field");

            return DemoUtils.Row(basic, withRange);
        }
    }
}
