using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the LinearProgress documentation page.</summary>
    static class LinearProgressDemos
    {
        [VisualDocDemo("linearprogress")]
        static VisualElement Variants()
        {
            var indeterminate = new LinearProgress { variant = Progress.Variant.Indeterminate };
            indeterminate.AddToClassList("demo-field");

            var determinate = new LinearProgress
            {
                variant = Progress.Variant.Determinate,
                value = 0.4f,
                bufferValue = 0.6f,
            };
            determinate.AddToClassList("demo-field");

            return DemoUtils.Column(indeterminate, determinate);
        }
    }
}
