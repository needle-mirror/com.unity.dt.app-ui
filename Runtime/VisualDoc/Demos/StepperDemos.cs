using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Stepper documentation page.</summary>
    static class StepperDemos
    {
        [VisualDocDemo("stepper")]
        static VisualElement Sizes()
        {
            return DemoUtils.Row(
                new Stepper { size = Size.S },
                new Stepper { size = Size.M },
                new Stepper { size = Size.L });
        }
    }
}
