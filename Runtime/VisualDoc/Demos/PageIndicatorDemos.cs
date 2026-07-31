using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Page Indicator documentation page.</summary>
    static class PageIndicatorDemos
    {
        [VisualDocDemo("pageindicator")]
        static VisualElement Basic()
        {
            var indicator = new PageIndicator { count = 4, value = 1 };
            return DemoUtils.Row(indicator);
        }
    }
}
