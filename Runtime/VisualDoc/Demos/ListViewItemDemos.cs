using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ListViewItem documentation page.</summary>
    static class ListViewItemDemos
    {
        [VisualDocDemo("listviewitem")]
        static VisualElement Basic()
        {
            return DemoUtils.Column(
                new ListViewItem { title = "Project Assets", subtitle = "Contains 42 files", size = Size.M },
                new ListViewItem { title = "Documentation", subtitle = "Last updated: Today", size = Size.M },
                new ListViewItem { title = "Loading item", subtitle = "Fetching data...", isLoading = true });
        }
    }
}
