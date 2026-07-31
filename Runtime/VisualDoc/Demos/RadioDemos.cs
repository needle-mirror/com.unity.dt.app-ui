using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Radio documentation page.</summary>
    static class RadioDemos
    {
        [VisualDocDemo("radio")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new Radio { label = "Unselected", key = "unselected" },
                new Radio { label = "Selected", key = "selected", value = true },
                new Radio { label = "Emphasized", key = "emphasized", value = true, emphasized = true },
                new Radio { label = "Small", key = "small", size = Size.S },
                new Radio { label = "Large", key = "large", size = Size.L });
        }
    }
}
