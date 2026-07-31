using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the GridView documentation page.</summary>
    static class GridViewDemos
    {
        [VisualDocDemo("gridview")]
        static VisualElement Basic()
        {
            var items = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple", "Orange" };

            var gridView = new GridView(
                items,
                () => new Chip(),
                (element, index) => ((Chip)element).label = items[index])
            {
                itemHeight = 40,
                columnCount = 2
            };
            gridView.AddToClassList("demo-block");

            return DemoUtils.Row(gridView);
        }
    }
}
