using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Bottom Navigation Bar documentation page.</summary>
    static class BottomNavigationBarDemos
    {
        [VisualDocDemo("bottom-navigation-bar")]
        static VisualElement Basic()
        {
            var navBar = new BottomNavBar();
            navBar.Add(new BottomNavBarItem("house", "Home", null) { isSelected = true });
            navBar.Add(new BottomNavBarItem("magnifying-glass", "Search", null));
            navBar.Add(new BottomNavBarItem("user", "Profile", null));

            return DemoUtils.Row(navBar);
        }
    }
}
