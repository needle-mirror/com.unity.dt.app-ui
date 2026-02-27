# App UI Navigation Reference

Complete reference for the App UI navigation system including all classes, methods, and common usage patterns.

## Table of Contents

1. [NavGraphViewAsset](#navgraphviewasset)
2. [NavGraph](#navgraph)
3. [NavDestination](#navdestination)
4. [NavAction](#navaction)
5. [NavHost](#navhost)
6. [NavController](#navcontroller)
7. [INavVisualController](#inavisualcontroller)
8. [NavigationScreen](#navigationscreen)
9. [NavDestinationTemplate](#navdestinationtemplate)
10. [Back Stack Management](#back-stack-management)
11. [Nested Navigation Graphs](#nested-navigation-graphs)
12. [Visual Navigation Components](#visual-navigation-components)

## NavGraphViewAsset

Main asset that contains one or more navigation graphs.

### Properties

```csharp
public class NavGraphViewAsset : ScriptableObject
{
    // The root navigation graph(s)
    public List<NavGraph> graphs { get; set; }
}
```

### Usage

```csharp
// Load from Resources
var navGraphAsset = Resources.Load<NavGraphViewAsset>("Navigation/MyNavGraph");

// Or create at runtime
var navGraphAsset = ScriptableObject.CreateInstance<NavGraphViewAsset>();
var mainGraph = new NavGraph { name = "main" };
navGraphAsset.graphs.Add(mainGraph);
```

## NavGraph

A set of pages (destinations) and actions that connect them. Each graph must have a start destination.

### Properties

```csharp
public class NavGraph : ScriptableObject
{
    // Unique identifier for this graph
    public string name { get; set; }

    // The first destination shown when navigating to this graph
    public NavDestination startDestination { get; set; }

    // All destinations in this graph
    public List<NavDestination> destinations { get; set; }

    // Actions that can be performed within this graph
    public List<NavAction> actions { get; set; }

    // Global actions available from any destination in this graph and nested ones
    public List<NavAction> globalActions { get; set; }

    // Nested navigation graphs
    public List<NavGraph> nestedGraphs { get; set; }
}
```

### Creating a NavGraph

```csharp
// Create a new graph
var graph = new NavGraph
{
    name = "main",
    startDestination = homeDestination,
    destinations = new List<NavDestination> { homeDestination, settingsDestination }
};

// Set start destination (must be done)
graph.startDestination = homeDestination;

// Add destinations
graph.destinations.Add(detailsDestination);

// Add local actions
var action = new NavAction
{
    actionId = "openSettings",
    source = homeDestination,
    destination = settingsDestination
};
graph.actions.Add(action);

// Add global actions (available from anywhere)
var logoutAction = new NavAction
{
    actionId = "logout",
    destination = loginDestination
};
graph.globalActions.Add(logoutAction);
```

## NavDestination

Represents a page or screen in the navigation graph.

### Properties

```csharp
public class NavDestination : ScriptableObject
{
    // Unique identifier within the graph
    public string name { get; set; }

    // Display label for this destination
    public string label { get; set; }

    // Template that creates the visual screen for this destination
    public NavDestinationTemplate template { get; set; }

    // Nested graph for this destination (optional)
    public NavGraph nestedGraph { get; set; }
}
```

### Creating Destinations

```csharp
// Basic destination
var homeDestination = new NavDestination
{
    name = "home",
    label = "Home",
    template = new DefaultDestinationTemplate
    {
        screenType = typeof(HomeScreen),
        showAppBar = true,
        showDrawer = true,
        showBottomNavBar = true
    }
};

// Destination with custom template
var customDestination = new NavDestination
{
    name = "custom",
    label = "Custom Screen",
    template = new MyCustomTemplate()
};

// Destination with nested graph
var settingsDestination = new NavDestination
{
    name = "settings",
    label = "Settings",
    nestedGraph = settingsGraph  // Navigating here enters the nested graph
};
```

## NavAction

Defines an action that navigates from one destination to another, or from anywhere (global action).

### Properties

```csharp
public class NavAction : ScriptableObject
{
    // Unique identifier for this action
    public string actionId { get; set; }

    // Source destination (null for global actions)
    public NavDestination source { get; set; }

    // Destination to navigate to
    public NavDestination destination { get; set; }

    // How to handle the back stack
    public BackStackBehavior backStackBehavior { get; set; }

    // Animation or transition type
    public NavTransition transition { get; set; }
}
```

### BackStackBehavior

```csharp
public enum BackStackBehavior
{
    // Push the destination onto the back stack (default)
    Push = 0,

    // Navigate back to the destination if it exists in the stack
    PopUntilInclusive = 1,

    // Navigate back to the destination and remove it
    PopUntil = 2,

    // Clear the entire back stack and start fresh
    ClearStack = 3,

    // Replace the current destination
    Replace = 4
}
```

### Creating Actions

```csharp
// Global action (available from any destination)
var logoutAction = new NavAction
{
    actionId = "logout",
    destination = loginDestination,
    backStackBehavior = BackStackBehavior.ClearStack
};
graph.globalActions.Add(logoutAction);

// Local action (only from home destination)
var toDetailsAction = new NavAction
{
    actionId = "viewDetails",
    source = homeDestination,
    destination = detailsDestination,
    backStackBehavior = BackStackBehavior.Push
};
graph.actions.Add(toDetailsAction);

// Action with stack clearing (navigate "up")
var toHomeAction = new NavAction
{
    actionId = "navigateUp",
    source = detailsDestination,
    destination = homeDestination,
    backStackBehavior = BackStackBehavior.PopUntilInclusive
};
graph.actions.Add(toHomeAction);
```

## NavHost

A component that hosts a navigation graph and displays destinations.

### Properties

```csharp
public class NavHost : VisualElement
{
    // Asset containing the navigation graph
    public NavGraphViewAsset graphViewAsset { get; set; }

    // Controller for visual navigation components
    public INavVisualController visualController { get; set; }

    // The active NavController
    public NavController navController { get; }

    // Fired when navigation occurs
    public event Action<NavController, NavDestination> onNavigate;

    // Fired when a destination is entered
    public event Action<NavController, NavDestination> onDestinationEnter;

    // Fired when a destination is exited
    public event Action<NavController, NavDestination> onDestinationExit;
}
```

### Common Methods

```csharp
// Load a navigation graph
navHost.LoadGraph(navGraphViewAsset);

// Load a specific graph from the asset
navHost.LoadGraph(navGraphViewAsset, "mainGraph");

// Get the NavController
var controller = navHost.navController;

// Subscribe to navigation events
navHost.onNavigate += (controller, destination) =>
{
    Debug.Log($"Navigated to {destination.label}");
};
```

### Setup Example

```csharp
var navHost = new NavHost();

// Set visual controller to handle AppBar, Drawer, etc.
navHost.visualController = new MyNavVisualController();

// Load the navigation graph
var graphAsset = Resources.Load<NavGraphViewAsset>("Navigation/MainGraph");
navHost.LoadGraph(graphAsset);

// Listen to navigation events
navHost.onNavigate += (controller, destination) =>
{
    Debug.Log($"Navigated to {destination.name}");
};

// Add to UI
root.Add(navHost);
```

## NavController

Manages navigation state and the back stack. Created automatically by NavHost.

### Properties

```csharp
public class NavController : IDisposable
{
    // Current destination
    public NavDestination currentDestination { get; }

    // Current navigation graph
    public NavGraph currentGraph { get; }

    // The navigation host this controller is attached to
    public NavHost navHost { get; }

    // Back stack entries
    public IReadOnlyList<NavBackStackEntry> backStack { get; }

    // Can navigate back?
    public bool canNavigateBack { get; }

    // Fired before navigation
    public event Action<NavController, NavDestination> beforeNavigate;

    // Fired after navigation
    public event Action<NavController, NavDestination> afterNavigate;
}
```

### Common Methods

```csharp
// Navigate by action ID
navController.Navigate("actionId");

// Navigate by action ID with arguments
navController.Navigate("actionId", new[] {
    new Argument { key = "id", value = 123 },
    new Argument { key = "name", value = "Item" }
});

// Navigate to a destination directly
navController.Navigate(destination);

// Navigate to a destination with arguments
navController.Navigate(destination, new[] { ... });

// Navigate back
if (navController.canNavigateBack)
{
    navController.NavigateBack();
}

// Get the back stack
var stack = navController.backStack;
foreach (var entry in stack)
{
    Debug.Log($"Stack: {entry.destination.name}");
}

// Subscribe to navigation events
navController.beforeNavigate += (controller, destination) =>
{
    Debug.Log($"About to navigate to {destination.name}");
};

navController.afterNavigate += (controller, destination) =>
{
    Debug.Log($"Navigated to {destination.name}");
};
```

### Navigation Examples

```csharp
// Navigate with action ID
navController.Navigate("openDetails");

// Navigate with arguments
navController.Navigate("openDetails", new[] {
    new Argument { key = "itemId", value = "12345" },
    new Argument { key = "title", value = "My Item" }
});

// Navigate back
if (navController.canNavigateBack)
    navController.NavigateBack();

// Navigate with specific back stack behavior
// (defined in the NavAction for that action)
navController.Navigate("navigateHome");  // ClearStack behavior

// Check current destination
if (navController.currentDestination.name == "home")
{
    Debug.Log("We're at home");
}
```

## INavVisualController

Interface for controlling visual navigation components (AppBar, Drawer, BottomNavBar, NavigationRail).

### Methods

```csharp
public interface INavVisualController
{
    // Called when AppBar is required by the destination
    void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController);

    // Called when Drawer is required by the destination
    void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController);

    // Called when BottomNavBar is required by the destination
    void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController);

    // Called when NavigationRail is required by the destination
    void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController);
}
```

### Complete Implementation Example

```csharp
public class MyNavVisualController : INavVisualController
{
    public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
    {
        appBar.title = destination.label;

        // Add action buttons if needed
        var editButton = new ActionButton { icon = "edit", label = "Edit" };
        editButton.clickable.clicked += () => navController.Navigate("edit");
        appBar.Add(editButton);
    }

    public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController)
    {
        drawer.Add(new DrawerHeader { title = "Navigation" });
        drawer.Add(new Divider { vertical = false });

        // Home
        var homeItem = new MenuItem { icon = "home", label = "Home", selectable = true };
        homeItem.SetValueWithoutNotify(destination.name == "home");
        homeItem.clickable.clicked += () => navController.Navigate("navigateToHome");
        drawer.Add(homeItem);

        // Settings
        var settingsItem = new MenuItem { icon = "settings", label = "Settings", selectable = true };
        settingsItem.SetValueWithoutNotify(destination.name == "settings");
        settingsItem.clickable.clicked += () => navController.Navigate("navigateToSettings");
        drawer.Add(settingsItem);

        // Divider
        drawer.Add(new Divider { vertical = false });

        // Logout
        var logoutItem = new MenuItem { icon = "logout", label = "Logout", selectable = false };
        logoutItem.clickable.clicked += () => navController.Navigate("logout");
        drawer.Add(logoutItem);
    }

    public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
    {
        // Home
        var homeItem = new BottomNavBarItem("home", "Home", () => navController.Navigate("navigateToHome"))
        {
            isSelected = destination.name == "home"
        };
        bottomNavBar.Add(homeItem);

        // Explore
        var exploreItem = new BottomNavBarItem("explore", "Explore", () => navController.Navigate("navigateToExplore"))
        {
            isSelected = destination.name == "explore"
        };
        bottomNavBar.Add(exploreItem);

        // Saved
        var savedItem = new BottomNavBarItem("saved", "Saved", () => navController.Navigate("navigateToSaved"))
        {
            isSelected = destination.name == "saved"
        };
        bottomNavBar.Add(savedItem);

        // Profile
        var profileItem = new BottomNavBarItem("person", "Profile", () => navController.Navigate("navigateToProfile"))
        {
            isSelected = destination.name == "profile"
        };
        bottomNavBar.Add(profileItem);
    }

    public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController)
    {
        navigationRail.anchor = NavigationRailAnchor.Start;
        navigationRail.labelType = LabelType.Selected;
        navigationRail.groupAlignment = GroupAlignment.Center;

        var homeItem = new NavigationRailItem { icon = "home", label = "Home", selected = destination.name == "home" };
        homeItem.clickable.clicked += () => navController.Navigate("navigateToHome");
        navigationRail.mainContainer.Add(homeItem);

        var settingsItem = new NavigationRailItem { icon = "settings", label = "Settings", selected = destination.name == "settings" };
        settingsItem.clickable.clicked += () => navController.Navigate("navigateToSettings");
        navigationRail.mainContainer.Add(settingsItem);

        var logoutItem = new NavigationRailItem { icon = "logout", label = "Logout", selected = false };
        logoutItem.clickable.clicked += () => navController.Navigate("logout");
        navigationRail.trailingContainer.Add(logoutItem);
    }
}
```

### Conditional Visual Controller Setup

```csharp
public class ResponsiveNavVisualController : INavVisualController
{
    private readonly Vector2 m_ResponsiveBreakpoint = new(800, 600);

    public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
    {
        appBar.title = destination.label;
    }

    public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController)
    {
        // Only show drawer on larger screens
        if (drawer.parent.layout.width >= m_ResponsiveBreakpoint.x)
        {
            drawer.visible = true;
            // Populate drawer...
        }
    }

    public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
    {
        // Populate bottom nav...
    }

    public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController)
    {
        // Populate rail...
    }
}
```

## NavigationScreen

Base class for screens that automatically handle SetupAppBar, SetupDrawer, SetupBottomNavBar, and SetupNavigationRail.

### Methods

```csharp
public abstract class NavigationScreen : VisualElement, INavigationScreen
{
    // Called when entering this screen
    public virtual void OnEnter(NavController controller, NavDestination destination, Argument[] args)
    {
        // Override to handle enter logic
    }

    // Called when exiting this screen
    public virtual void OnExit(NavController controller, NavDestination destination, Argument[] args)
    {
        // Override to handle exit logic
    }

    // Called to setup AppBar for this screen
    protected virtual void SetupAppBar(AppBar appBar, NavController navController)
    {
        // Override to customize AppBar
    }

    // Called to setup Drawer for this screen
    protected virtual void SetupDrawer(Drawer drawer, NavController navController)
    {
        // Override to customize Drawer
    }

    // Called to setup BottomNavBar for this screen
    protected virtual void SetupBottomNavBar(BottomNavBar bottomNavBar, NavController navController)
    {
        // Override to customize BottomNavBar
    }

    // Called to setup NavigationRail for this screen
    protected virtual void SetupNavigationRail(NavigationRail navigationRail, NavController navController)
    {
        // Override to customize NavigationRail
    }
}
```

### Implementation Example

```csharp
public class HomeScreen : NavigationScreen
{
    private Label m_WelcomeLabel;

    public override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
    {
        base.OnEnter(controller, destination, args);

        m_WelcomeLabel = new Label("Welcome to Home");
        Add(m_WelcomeLabel);
    }

    protected override void SetupAppBar(AppBar appBar, NavController navController)
    {
        base.SetupAppBar(appBar, navController);
        appBar.title = "Home";

        var menuButton = new ActionButton { icon = "menu", label = "Menu" };
        menuButton.clickable.clicked += () => navController.Navigate("settings");
        appBar.Add(menuButton);
    }

    protected override void SetupBottomNavBar(BottomNavBar bottomNavBar, NavController navController)
    {
        base.SetupBottomNavBar(bottomNavBar, navController);

        var exploreItem = new BottomNavBarItem("explore", "Explore", () => navController.Navigate("openExplore"))
        {
            isSelected = false
        };
        bottomNavBar.Add(exploreItem);
    }

    public override void OnExit(NavController controller, NavDestination destination, Argument[] args)
    {
        // Cleanup if needed
        base.OnExit(controller, destination, args);
    }
}
```

## NavDestinationTemplate

Base class for customizing how destinations create screens.

### Methods

```csharp
public abstract class NavDestinationTemplate : ScriptableObject
{
    // Create the screen for this destination
    public abstract INavigationScreen CreateScreen(NavHost host);
}
```

### DefaultDestinationTemplate Properties

```csharp
public class DefaultDestinationTemplate : NavDestinationTemplate
{
    // The NavigationScreen type to instantiate
    public Type screenType { get; set; }

    // Show AppBar component
    public bool showAppBar { get; set; }

    // Show Drawer component
    public bool showDrawer { get; set; }

    // Show BottomNavBar component
    public bool showBottomNavBar { get; set; }

    // Show NavigationRail component
    public bool showNavigationRail { get; set; }
}
```

### Custom Template Example

```csharp
public class PrefabDestinationTemplate : NavDestinationTemplate
{
    [SerializeField]
    private GameObject m_ScreenPrefab;

    public override INavigationScreen CreateScreen(NavHost host)
    {
        var screenGo = Instantiate(m_ScreenPrefab);
        var navigationScreen = screenGo.GetComponent<INavigationScreen>();
        return navigationScreen ?? throw new InvalidOperationException("Prefab must have INavigationScreen component");
    }
}
```

## Back Stack Management

The NavController automatically manages the back stack. Control behavior through NavAction properties.

### Understanding Back Stack Behavior

```csharp
// Push (default): destination added to stack
// Stack: [Home] → navigate to Details → Stack: [Home, Details]
var pushAction = new NavAction
{
    actionId = "viewDetails",
    destination = detailsDestination,
    backStackBehavior = BackStackBehavior.Push
};

// PopUntilInclusive: navigate back to destination and remove it
// Stack: [Home, Details] → popUntilInclusive(Home) → Stack: [Home]
var popInclusiveAction = new NavAction
{
    actionId = "navigateToHome",
    destination = homeDestination,
    backStackBehavior = BackStackBehavior.PopUntilInclusive
};

// PopUntil: navigate back to destination and keep it
// Stack: [Home, Details, List] → popUntil(Home) → Stack: [Home]
var popAction = new NavAction
{
    actionId = "navigateToHome",
    destination = homeDestination,
    backStackBehavior = BackStackBehavior.PopUntil
};

// ClearStack: clear entire back stack and start fresh
// Stack: [Home, Details, List] → clearStack(Home) → Stack: [Home]
var clearAction = new NavAction
{
    actionId = "logout",
    destination = loginDestination,
    backStackBehavior = BackStackBehavior.ClearStack
};

// Replace: replace current destination without adding to stack
// Stack: [Home, Details] → replace(List) → Stack: [Home, List]
var replaceAction = new NavAction
{
    actionId = "replaceWithList",
    destination = listDestination,
    backStackBehavior = BackStackBehavior.Replace
};
```

### Programmatic Back Stack Access

```csharp
// Check if can navigate back
if (navController.canNavigateBack)
{
    navController.NavigateBack();
}

// Examine back stack
foreach (var entry in navController.backStack)
{
    Debug.Log($"Stack entry: {entry.destination.name}");
}

// Get current destination
var current = navController.currentDestination;
Debug.Log($"Current: {current.name}");

// Navigate to specific destination with arguments
var args = new[] {
    new Argument { key = "id", value = "123" },
    new Argument { key = "title", value = "Item" }
};
navController.Navigate("details", args);
```

## Nested Navigation Graphs

Create complex navigation structures by nesting graphs.

### Basic Nested Graph

```csharp
// Create a settings sub-graph
var settingsGraph = new NavGraph
{
    name = "settings",
    startDestination = settingsHomeDestination
};
settingsGraph.destinations.Add(settingsHomeDestination);
settingsGraph.destinations.Add(accountSettingsDestination);

// Add action in settings graph
var accountAction = new NavAction
{
    actionId = "viewAccount",
    source = settingsHomeDestination,
    destination = accountSettingsDestination
};
settingsGraph.actions.Add(accountAction);

// Create destination that uses nested graph
var settingsDestination = new NavDestination
{
    name = "settings",
    label = "Settings",
    nestedGraph = settingsGraph
};

// Add to main graph
mainGraph.destinations.Add(settingsDestination);

// Navigate to nested graph
navController.Navigate("settings");
```

### Global Actions in Nested Graphs

```csharp
// Global action available from anywhere, including nested graphs
var logoutAction = new NavAction
{
    actionId = "logout",
    destination = loginDestination,
    backStackBehavior = BackStackBehavior.ClearStack
};
mainGraph.globalActions.Add(logoutAction);

// Can navigate to logout from any destination, even nested ones
navController.Navigate("logout");
```

## Visual Navigation Components

Reference for the visual components controlled by INavVisualController.

### AppBar

```csharp
public class AppBar : VisualElement
{
    // Screen title
    public string title { get; set; }

    // Show menu button to open drawer
    public bool showMenuButton { get; set; }

    // Add action button(s)
    public void Add(ActionButton actionButton) { ... }
}

// Usage
appBar.title = "My Screen";
var button = new ActionButton { icon = "edit", label = "Edit" };
appBar.Add(button);
```

### Drawer

```csharp
public class Drawer : VisualElement
{
    // Add items to drawer
    public void Add(VisualElement item) { ... }

    // Clear all items
    public void Clear() { ... }
}

// Usage
drawer.Add(new DrawerHeader { title = "Menu" });
var item = new MenuItem { icon = "home", label = "Home" };
drawer.Add(item);
```

### BottomNavBar

```csharp
public class BottomNavBar : VisualElement
{
    // Add navigation items
    public void Add(BottomNavBarItem item) { ... }

    // Clear all items
    public void Clear() { ... }
}

public class BottomNavBarItem : VisualElement
{
    public string icon { get; set; }
    public string label { get; set; }
    public bool isSelected { get; set; }
    public Action onClicked { get; set; }
}

// Usage
var item = new BottomNavBarItem("home", "Home", () => Debug.Log("Clicked"))
{
    isSelected = true
};
bottomNavBar.Add(item);
```

### NavigationRail

```csharp
public class NavigationRail : VisualElement
{
    // Position the rail
    public NavigationRailAnchor anchor { get; set; }

    // How to display labels
    public LabelType labelType { get; set; }

    // Align items in main container
    public GroupAlignment groupAlignment { get; set; }

    // Main items container
    public VisualElement mainContainer { get; }

    // Leading items (top)
    public VisualElement leadingContainer { get; }

    // Trailing items (bottom)
    public VisualElement trailingContainer { get; }
}

public class NavigationRailItem : VisualElement
{
    public string icon { get; set; }
    public string label { get; set; }
    public bool selected { get; set; }
}

// Usage
navigationRail.anchor = NavigationRailAnchor.Start;
var item = new NavigationRailItem { icon = "home", label = "Home", selected = true };
navigationRail.mainContainer.Add(item);
```

## Common Patterns and Recipes

### Multi-Tab Navigation

```csharp
public class MyNavVisualController : INavVisualController
{
    private Dictionary<string, BottomNavBarItem> m_NavItems = new();

    public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
    {
        var destinations = new[] { "home", "explore", "saved", "profile" };

        foreach (var dest in destinations)
        {
            var item = new BottomNavBarItem("icon_" + dest, dest.ToUpper(), () =>
            {
                navController.Navigate("navigateTo" + dest.Capitalize());
            })
            {
                isSelected = destination.name == dest
            };
            bottomNavBar.Add(item);
            m_NavItems[dest] = item;
        }
    }

    public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController) { }
    public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController) { }
    public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController) { }
}
```

### Deep Linking

```csharp
public class DeepLinkHandler
{
    private NavController m_NavController;

    public void HandleDeepLink(string link)
    {
        // Parse link: "app://home/details?id=123"
        var uri = new System.Uri(link);
        var path = uri.LocalPath.TrimStart('/');
        var args = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var arguments = new List<Argument>();
        foreach (var key in args.AllKeys)
        {
            arguments.Add(new Argument { key = key, value = args[key] });
        }

        m_NavController.Navigate(path, arguments.ToArray());
    }
}
```

### Conditional Navigation Based on State

```csharp
public class AuthNavigationController : INavVisualController
{
    private IAuthService m_AuthService;

    public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
    {
        appBar.title = destination.label;

        if (m_AuthService.IsAuthenticated)
        {
            var logoutButton = new ActionButton { icon = "logout", label = "Logout" };
            logoutButton.clickable.clicked += () => navController.Navigate("logout");
            appBar.Add(logoutButton);
        }
    }

    public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController) { }
    public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController) { }
    public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController) { }
}
```
