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
                new Stepper { size = Size.Sm },
                new Stepper { size = Size.Md },
                new Stepper { size = Size.Lg });
        }
    }
}
