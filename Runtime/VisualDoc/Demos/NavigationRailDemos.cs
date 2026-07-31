using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Navigation Rail documentation page.</summary>
    static class NavigationRailDemos
    {
        [VisualDocDemo("navigation-rail")]
        static VisualElement Basic()
        {
            var rail = new NavigationRail();
            rail.AddToClassList("demo-block");
            rail.Add(new NavigationRailItem { icon = "house", label = "Home", selected = true });
            rail.Add(new NavigationRailItem { icon = "heart", label = "Favorites" });
            rail.Add(new NavigationRailItem { icon = "gear", label = "Settings" });

            return DemoUtils.Row(rail);
        }
    }
}
