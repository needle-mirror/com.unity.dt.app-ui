using UnityEngine.UIElements;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the DateRangePicker documentation page.</summary>
    static class DateRangePickerDemos
    {
        [VisualDocDemo("daterangepicker")]
        static VisualElement Basic()
        {
            var picker = new DateRangePicker
            {
                value = new DateRange(new Date(2024, 1, 1), new Date(2024, 1, 15))
            };
            picker.AddToClassList("demo-block");

            return DemoUtils.Row(picker);
        }
    }
}
