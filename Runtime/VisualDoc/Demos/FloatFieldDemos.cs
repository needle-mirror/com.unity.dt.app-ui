using UnityEngine.UIElements;
using Unity.AppUI.UI;
using FloatField = Unity.AppUI.UI.FloatField;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the FloatField documentation page.</summary>
    static class FloatFieldDemos
    {
        [VisualDocDemo("floatfield")]
        static VisualElement Basic()
        {
            var percentage = new FloatField
            {
                value = 50f,
                lowValue = 0f,
                highValue = 100f,
                formatString = "F1",
                unit = "%"
            };
            percentage.AddToClassList("demo-field");

            var temperature = new FloatField { value = 22f, unit = "C" };
            temperature.AddToClassList("demo-field");

            return DemoUtils.Row(percentage, temperature);
        }
    }
}
