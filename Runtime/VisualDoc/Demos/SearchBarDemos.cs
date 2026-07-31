using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the SearchBar documentation page.</summary>
    static class SearchBarDemos
    {
        [VisualDocDemo("searchbar")]
        static VisualElement Sizes()
        {
            var small = new SearchBar { size = Size.S };
            var medium = new SearchBar { size = Size.M };
            var large = new SearchBar { size = Size.L };

            small.AddToClassList("demo-field");
            medium.AddToClassList("demo-field");
            large.AddToClassList("demo-field");

            return DemoUtils.Row(small, medium, large);
        }
    }
}
