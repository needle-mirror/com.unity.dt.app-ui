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
                new Link("Small Link", "https://example.com") { size = LinkSize.Sm },
                new Link("Medium Link", "https://example.com") { size = LinkSize.Base },
                new Link("Large Link", "https://example.com") { size = LinkSize.Lg });
        }
    }
}
