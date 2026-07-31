using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the AppBar documentation page.</summary>
    static class AppBarDemos
    {
        [VisualDocDemo("appbar")]
        static VisualElement Basic()
        {
            var appBar = new AppBar { title = "Dashboard" };
            appBar.AddAction(new ActionButton { icon = "magnifying-glass" });
            appBar.AddAction(new ActionButton { icon = "gear" });

            return DemoUtils.Row(appBar);
        }
    }
}
