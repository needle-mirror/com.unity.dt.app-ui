using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the SwipeView documentation page.</summary>
    static class SwipeViewDemos
    {
        [VisualDocDemo("swipeview")]
        static VisualElement Basic()
        {
            var swipeView = new SwipeView { direction = Direction.Horizontal };
            swipeView.AddToClassList("demo-block");

            var item1 = new SwipeViewItem();
            item1.Add(new Chip { label = "Item 1" });
            var item2 = new SwipeViewItem();
            item2.Add(new Chip { label = "Item 2" });
            var item3 = new SwipeViewItem();
            item3.Add(new Chip { label = "Item 3" });

            swipeView.Add(item1);
            swipeView.Add(item2);
            swipeView.Add(item3);

            return swipeView;
        }
    }
}
