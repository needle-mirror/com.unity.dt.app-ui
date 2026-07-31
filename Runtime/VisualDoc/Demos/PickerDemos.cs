using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Picker documentation page.</summary>
    static class PickerDemos
    {
        [VisualDocDemo("picker")]
        static VisualElement Basic()
        {
            var picker = new Dropdown
            {
                sourceItems = new List<string> { "Small", "Medium", "Large" },
                defaultMessage = "Select size...",
            };
            picker.selectedIndex = 1;
            picker.AddToClassList("demo-field");

            return DemoUtils.Row(picker);
        }
    }
}
