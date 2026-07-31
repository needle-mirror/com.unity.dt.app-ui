using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the DateRangeField documentation page.</summary>
    static class DateRangeFieldDemos
    {
        [VisualDocDemo("daterangefield")]
        static VisualElement Basic()
        {
            var field = new DateRangeField { size = Size.M, formatString = "MM/dd/yyyy" };
            field.AddToClassList("demo-field");

            return DemoUtils.Row(field);
        }
    }
}
