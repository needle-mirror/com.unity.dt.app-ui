using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the MasonryGridView documentation page.</summary>
    static class MasonryGridViewDemos
    {
        [VisualDocDemo("masonrygridview")]
        static VisualElement Basic()
        {
            var items = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6" };

            var grid = new MasonryGridView
            {
                columnCount = 3,
                pack = true,
                itemsSource = items,
                makeItem = () => new Chip(),
                bindItem = (element, i) => ((Chip)element).label = items[i],
            };
            grid.AddToClassList("demo-block");

            return DemoUtils.Row(grid);
        }
    }
}
