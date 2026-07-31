using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Link documentation page.</summary>
    static class LinkDemos
    {
        [VisualDocDemo("link")]
        static VisualElement Sizes()
        {
            return DemoUtils.Row(
                new Link("Small Link", "https://example.com") { size = TextSize.S },
                new Link("Medium Link", "https://example.com") { size = TextSize.M },
                new Link("Large Link", "https://example.com") { size = TextSize.L });
        }
    }
}
