using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the DateField documentation page.</summary>
    static class DateFieldDemos
    {
        [VisualDocDemo("datefield")]
        static VisualElement Sizes()
        {
            var small = new DateField { size = Size.S };
            small.AddToClassList("demo-field");

            var medium = new DateField { size = Size.M };
            medium.AddToClassList("demo-field");

            var large = new DateField { size = Size.L };
            large.AddToClassList("demo-field");

            return DemoUtils.Row(small, medium, large);
        }
    }
}
