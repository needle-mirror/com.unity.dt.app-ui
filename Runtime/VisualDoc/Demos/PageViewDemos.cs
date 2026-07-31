using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Page View documentation page.</summary>
    static class PageViewDemos
    {
        [VisualDocDemo("pageview")]
        static VisualElement Basic()
        {
            var pageView = new PageView();
            pageView.AddToClassList("demo-block");
            pageView.Add(BuildPage("Page 1"));
            pageView.Add(BuildPage("Page 2"));
            pageView.Add(BuildPage("Page 3"));

            return DemoUtils.Row(pageView);
        }

        static SwipeViewItem BuildPage(string label)
        {
            var item = new SwipeViewItem();
            item.Add(new Text(label));
            return item;
        }
    }
}
