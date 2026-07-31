using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the CircularProgress documentation page.</summary>
    static class CircularProgressDemos
    {
        [VisualDocDemo("circularprogress")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new CircularProgress(),
                new CircularProgress { variant = Progress.Variant.Determinate, value = 0.25f },
                new CircularProgress { variant = Progress.Variant.Determinate, value = 0.75f });
        }
    }
}
