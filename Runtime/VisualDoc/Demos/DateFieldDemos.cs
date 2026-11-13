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
            var small = new DateField { size = Size.Sm };
            small.AddToClassList("demo-field");

            var medium = new DateField { size = Size.Md };
            medium.AddToClassList("demo-field");

            var large = new DateField { size = Size.Lg };
            large.AddToClassList("demo-field");

            return DemoUtils.Row(small, medium, large);
        }
    }
}
