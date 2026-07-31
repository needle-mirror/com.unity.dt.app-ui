using UnityEngine.UIElements;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the DatePicker documentation page.</summary>
    static class DatePickerDemos
    {
        [VisualDocDemo("datepicker")]
        static VisualElement Basic()
        {
            var picker = new DatePicker { value = new Date(2024, 1, 1) };
            picker.AddToClassList("demo-block");

            return DemoUtils.Row(picker);
        }
    }
}
