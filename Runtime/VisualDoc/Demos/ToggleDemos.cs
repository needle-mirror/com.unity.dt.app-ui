using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Toggle documentation page.</summary>
    static class ToggleDemos
    {
        [VisualDocDemo("toggle")]
        static VisualElement States()
        {
            return DemoUtils.Row(
                new Toggle { label = "Off" },
                new Toggle { label = "On", value = true },
                new Toggle { label = "Invalid", invalid = true });
        }
    }
}
