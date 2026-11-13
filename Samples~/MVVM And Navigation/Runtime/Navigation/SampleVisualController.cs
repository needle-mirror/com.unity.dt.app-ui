using Unity.AppUI.UI;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    /// <summary>
    /// Global visual controller that configures navigation chrome (AppBar, BottomNavBar, etc.)
    /// for every destination in the sample.
    /// </summary>
    class SampleVisualController : Navigation.INavVisualController
    {
        public void SetupAppBar(
            AppBar appBar,
            Navigation.NavDestination destination,
            Navigation.NavController navController)
        {
            appBar.title = destination.label;
        }

        public void SetupBottomNavBar(
            BottomNavBar bottomNavBar,
            Navigation.NavDestination destination,
            Navigation.NavController navController)
        {
            var homeItem = new BottomNavBarItem(
                "home",
                "Home",
                () => navController.Navigate(Actions.go_to_home))
            {
                isSelected = destination.name == Destinations.home
            };
            bottomNavBar.Add(homeItem);
        }

        public void SetupDrawer(
            Drawer drawer,
            Navigation.NavDestination destination,
            Navigation.NavController navController)
        {
            // No drawer content in this sample.
        }

        public void SetupNavigationRail(
            NavigationRail navigationRail,
            Navigation.NavDestination destination,
            Navigation.NavController navController)
        {
            // No navigation rail content in this sample.
        }
    }
}
