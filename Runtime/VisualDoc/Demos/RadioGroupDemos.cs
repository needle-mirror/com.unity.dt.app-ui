using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the RadioGroup documentation page.</summary>
    static class RadioGroupDemos
    {
        [VisualDocDemo("radiogroup")]
        static VisualElement Basic()
        {
            var group = new RadioGroup();
            group.Add(new Radio { key = "small", label = "Small" });
            group.Add(new Radio { key = "medium", label = "Medium" });
            group.Add(new Radio { key = "large", label = "Large" });
            group.value = "medium";

            return DemoUtils.Row(group);
        }
    }
}
