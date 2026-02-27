// Navigation System Example

using Unity.AppUI.Navigation;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

// ============================================================================
// 1. CUSTOM NAVIGATION VISUAL CONTROLLER
// ============================================================================

public class MyNavVisualController : INavVisualController
{
    public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
    {
        // Set title from destination
        appBar.title = destination.label;

        // Show back button if there's history
        if (navController.backStack.Count > 1)
        {
            appBar.showBackButton = true;
        }

        // Add action buttons
        appBar.trailingContainer.Clear();
        var settingsBtn = new IconButton { icon = "settings" };
        settingsBtn.clickable.clicked += () => navController.Navigate(Actions.NavigateToSettings);
        appBar.trailingContainer.Add(settingsBtn);
    }

    public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
    {
        bottomNavBar.Clear();

        // Home tab
        var homeItem = new BottomNavBarItem("home", "Home", () =>
            navController.Navigate(Actions.NavigateToHome))
        {
            isSelected = destination.name == Destinations.Home
        };
        bottomNavBar.Add(homeItem);

        // Search tab
        var searchItem = new BottomNavBarItem("search", "Search", () =>
            navController.Navigate(Actions.NavigateToSearch))
        {
            isSelected = destination.name == Destinations.Search
        };
        bottomNavBar.Add(searchItem);

        // Profile tab
        var profileItem = new BottomNavBarItem("person", "Profile", () =>
            navController.Navigate(Actions.NavigateToProfile))
        {
            isSelected = destination.name == Destinations.Profile
        };
        bottomNavBar.Add(profileItem);
    }

    public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController)
    {
        if (destination.destinationTemplate is DefaultDestinationTemplate template && template.showDrawer)
        {
            drawer.Clear();

            // Add drawer items
            var homeItem = new DrawerItem("Home", "home");
            homeItem.clickable.clicked += () =>
            {
                navController.Navigate(Actions.NavigateToHome);
                drawer.Close();
            };
            drawer.Add(homeItem);

            var settingsItem = new DrawerItem("Settings", "settings");
            settingsItem.clickable.clicked += () =>
            {
                navController.Navigate(Actions.NavigateToSettings);
                drawer.Close();
            };
            drawer.Add(settingsItem);
        }
    }

    public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController)
    {
        if (destination.destinationTemplate is DefaultDestinationTemplate template && template.showNavigationRail)
        {
            navigationRail.mainContainer.Clear();

            var homeItem = new NavigationRailItem("home", "Home");
            homeItem.isSelected = destination.name == Destinations.Home;
            homeItem.clickable.clicked += () => navController.Navigate(Actions.NavigateToHome);
            navigationRail.mainContainer.Add(homeItem);

            var searchItem = new NavigationRailItem("search", "Search");
            searchItem.isSelected = destination.name == Destinations.Search;
            searchItem.clickable.clicked += () => navController.Navigate(Actions.NavigateToSearch);
            navigationRail.mainContainer.Add(searchItem);
        }
    }
}

// ============================================================================
// 2. GENERATED NAVIGATION CONSTANTS (from Navigation Graph Editor)
// ============================================================================

public static class Actions
{
    public const string NavigateToHome = "navigateToHome";
    public const string NavigateToSearch = "navigateToSearch";
    public const string NavigateToProfile = "navigateToProfile";
    public const string NavigateToSettings = "navigateToSettings";
    public const string NavigateToDetails = "navigateToDetails";
}

public static class Destinations
{
    public const string Home = "home";
    public const string Search = "search";
    public const string Profile = "profile";
    public const string Settings = "settings";
    public const string Details = "details";
}

public static class Graphs
{
    public const string MainGraph = "mainGraph";
    public const string SettingsGraph = "settingsGraph";
}

// ============================================================================
// 3. CUSTOM NAVIGATION SCREEN
// ============================================================================

public class HomePage : VisualElement, INavigationScreen
{
    private NavController m_NavController;

    public void OnEnter(NavController controller, NavDestination destination, Argument[] args)
    {
        m_NavController = controller;

        // Build UI
        var heading = new Heading { text = "Home" };
        Add(heading);

        var detailsBtn = new Button { title = "View Details" };
        detailsBtn.clickable.clicked += () =>
        {
            // Navigate with arguments
            m_NavController.Navigate(Actions.NavigateToDetails,
                new Argument("itemId", "123"),
                new Argument("itemName", "Example Item"));
        };
        Add(detailsBtn);
    }

    public void OnExit(NavController controller, NavDestination destination, Argument[] args)
    {
        // Cleanup when leaving this screen
        Clear();
    }
}

public class DetailsPage : VisualElement, INavigationScreen
{
    public void OnEnter(NavController controller, NavDestination destination, Argument[] args)
    {
        // Get arguments
        string itemId = null;
        string itemName = null;

        foreach (var arg in args)
        {
            if (arg.key == "itemId")
                itemId = arg.value;
            else if (arg.key == "itemName")
                itemName = arg.value;
        }

        // Build UI with arguments
        var heading = new Heading { text = $"Details: {itemName}" };
        Add(heading);

        var idLabel = new Text { text = $"ID: {itemId}" };
        Add(idLabel);

        var backBtn = new Button { title = "Go Back" };
        backBtn.clickable.clicked += () => controller.PopBackStack();
        Add(backBtn);
    }

    public void OnExit(NavController controller, NavDestination destination, Argument[] args)
    {
        Clear();
    }
}

// ============================================================================
// 4. NAVIGATION SETUP
// ============================================================================

public class NavigationSetup : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    private NavGraphViewAsset m_NavGraphAsset;

    private NavHost m_NavHost;

    void Start()
    {
        // Create NavHost
        m_NavHost = new NavHost();
        m_NavHost.navGraphViewAsset = m_NavGraphAsset;
        m_NavHost.visualController = new MyNavVisualController();

        // Add to UI
        var uiDocument = GetComponent<UnityEngine.UIElements.UIDocument>();
        uiDocument.rootVisualElement.Add(m_NavHost);

        // Navigate to start destination
        m_NavHost.navController.Navigate(Actions.NavigateToHome);
    }
}
