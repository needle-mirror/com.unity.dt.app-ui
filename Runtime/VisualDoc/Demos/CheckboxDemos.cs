using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Checkbox documentation page.</summary>
    static class CheckboxDemos
    {
        [VisualDocDemo("checkbox")]
        static VisualElement States()
        {
            return DemoUtils.Row(
                new Checkbox { label = "Unchecked", value = CheckboxState.Unchecked },
                new Checkbox { label = "Checked", value = CheckboxState.Checked },
                new Checkbox { label = "Intermediate", value = CheckboxState.Intermediate });
        }
    }
}
