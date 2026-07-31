using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Chip documentation page.</summary>
    static class ChipDemos
    {
        [VisualDocDemo("chip")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new Chip { variant = Chip.Variant.Filled, label = "Filled" },
                new Chip { variant = Chip.Variant.Outlined, label = "Outlined" },
                new Chip { variant = Chip.Variant.Filled, label = "Deletable", deletable = true },
                new Chip { variant = Chip.Variant.Outlined, label = "Custom icon", deletable = true, deleteIcon = "trash" });
        }
    }
}
