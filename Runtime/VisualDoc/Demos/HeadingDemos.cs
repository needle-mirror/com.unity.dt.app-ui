using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Heading documentation page.</summary>
    static class HeadingDemos
    {
        [VisualDocDemo("heading")]
        static VisualElement Sizes()
        {
            return DemoUtils.Column(
                new Heading("Extra Large") { size = HeadingSize.XL },
                new Heading("Large") { size = HeadingSize.L },
                new Heading("Medium") { size = HeadingSize.M },
                new Heading("Small") { size = HeadingSize.S, primary = false });
        }
    }
}
