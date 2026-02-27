---
name: app-ui-navigation
description: Expert for App UI navigation system - NavGraph, NavHost, NavController, destinations, and visual controllers (AppBar, Drawer, BottomNavBar).
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# App UI Navigation Expert

Expert assistant for implementing navigation systems in Unity using the App UI framework. Specializes in NavGraph construction, NavHost setup, NavController usage, and visual navigation component control.

## When to Use This Skill

- Creating navigation graphs with destinations and actions
- Setting up NavHost with visual controllers
- Implementing custom navigation visual controllers
- Configuring AppBar, Drawer, BottomNavBar, and NavigationRail components
- Managing back stack and navigation state
- Creating nested navigation graphs
- Handling global vs local navigation actions
- Implementing NavigationScreen subclasses
- Generating code from navigation graphs

## Core Concepts

### Navigation System Architecture

The App UI navigation system consists of these key components:

1. **NavGraphViewAsset** - The main resource that contains navigation graphs
2. **NavGraph** - A set of pages that can be navigated between
3. **NavDestination** - A page in the navigation graph
4. **NavAction** - An action that navigates from one destination to another
5. **NavHost** - A component that hosts and displays a navigation graph
6. **NavController** - Controls navigation and back stack management
7. **INavVisualController** - Controls visual navigation components (AppBar, Drawer, etc.)

### Key Design Principles

1. **Start Destination Required** - Every NavGraph must have exactly one start destination
2. **Back Stack Managed** - NavController automatically manages the back stack
3. **Visual Component Control** - AppBar, Drawer, BottomNavBar, and NavigationRail are controlled via INavVisualController
4. **Nested Graphs Supported** - Create complex flows with nested navigation graphs
5. **Global Actions** - Actions without a source destination are global and available from any destination
6. **Local Actions** - Actions with a source destination are only available from that destination
7. **Code Generation** - Generate strongly-typed constants for destinations and actions

## Essential Patterns

### 1. Never Forget the Start Destination

A NavGraph MUST have a start destination. This is the first page shown:

```csharp
// CORRECT: Set start destination
navGraph.startDestination = homeDestination;

// WRONG: Missing start destination causes runtime errors
```

### 2. Always Implement INavVisualController

Control navigation UI components through a single controller:

```csharp
class MyNavVisualController : INavVisualController
{
    public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
    {
        appBar.title = destination.label;
    }

    public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController)
    {
        // Populate drawer
    }

    public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
    {
        // Populate bottom nav bar
    }

    public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController)
    {
        // Populate navigation rail
    }
}
```

### 3. Distinguish Global vs Local Actions

Global actions work from any destination, local actions only from specific sources:

```csharp
// CORRECT: Global action for logout (available everywhere)
var logoutAction = new NavAction { actionId = "logout", destination = loginGraph.startDestination };
navGraph.globalActions.Add(logoutAction);

// CORRECT: Local action (only from home)
var settingsAction = new NavAction { actionId = "openSettings", source = homeDestination, destination = settingsDestination };
navGraph.actions.Add(settingsAction);
```

### 4. Use DefaultDestinationTemplate for Standard Screens

The DefaultDestinationTemplate handles NavigationScreen instantiation and visual component setup:

```csharp
var destination = new NavDestination
{
    label = "Home",
    name = "home",
    template = new DefaultDestinationTemplate
    {
        screenType = typeof(HomeScreen),
        showAppBar = true,
        showDrawer = true,
        showBottomNavBar = true
    }
};
```

### 5. Properly Handle Arguments

Pass data to destinations using the Argument array:

```csharp
// Navigate with arguments
navController.Navigate("details", new[] {
    new Argument { key = "id", value = 123 }
});

// Receive arguments in NavigationScreen
public class DetailsScreen : NavigationScreen
{
    public override void OnEnter(NavController controller, NavDestination destination, Argument[] args)
    {
        var id = args.First(a => a.key == "id").value;
    }
}
```

### 6. Manage Back Stack Correctly

Control back stack behavior through NavAction properties:

```csharp
var action = new NavAction
{
    actionId = "navigateToHome",
    destination = homeDestination,
    // Clear back stack when navigating home
    backStackBehavior = BackStackBehavior.ClearStack
};
```

## Common Patterns

### App Bar Setup

```csharp
public void SetupAppBar(AppBar appBar, NavDestination destination, NavController navController)
{
    appBar.title = destination.label;

    // Add action buttons
    var editButton = new ActionButton { icon = "edit", label = "Edit" };
    editButton.clickable.clicked += () => navController.Navigate("editScreen");
    appBar.Add(editButton);
}
```

### Drawer Setup

```csharp
public void SetupDrawer(Drawer drawer, NavDestination destination, NavController navController)
{
    drawer.Add(new DrawerHeader { title = "Navigation" });
    drawer.Add(new Divider { vertical = false });

    var homeItem = new MenuItem { icon = "home", label = "Home", selectable = true };
    homeItem.SetValueWithoutNotify(destination.name == "home");
    homeItem.clickable.clicked += () => navController.Navigate("home");
    drawer.Add(homeItem);

    var settingsItem = new MenuItem { icon = "settings", label = "Settings", selectable = true };
    settingsItem.SetValueWithoutNotify(destination.name == "settings");
    settingsItem.clickable.clicked += () => navController.Navigate("settings");
    drawer.Add(settingsItem);
}
```

### BottomNavBar Setup

```csharp
public void SetupBottomNavBar(BottomNavBar bottomNavBar, NavDestination destination, NavController navController)
{
    var homeItem = new BottomNavBarItem("home", "Home", () => navController.Navigate("home"))
    {
        isSelected = destination.name == "home"
    };
    bottomNavBar.Add(homeItem);

    var exploreItem = new BottomNavBarItem("explore", "Explore", () => navController.Navigate("explore"))
    {
        isSelected = destination.name == "explore"
    };
    bottomNavBar.Add(exploreItem);
}
```

### NavigationRail Setup

```csharp
public void SetupNavigationRail(NavigationRail navigationRail, NavDestination destination, NavController navController)
{
    navigationRail.anchor = NavigationRailAnchor.Start;
    navigationRail.labelType = LabelType.Selected;

    var homeItem = new NavigationRailItem { icon = "home", label = "Home", selected = destination.name == "home" };
    homeItem.clickable.clicked += () => navController.Navigate("home");
    navigationRail.mainContainer.Add(homeItem);

    var settingsItem = new NavigationRailItem { icon = "settings", label = "Settings", selected = destination.name == "settings" };
    settingsItem.clickable.clicked += () => navController.Navigate("settings");
    navigationRail.mainContainer.Add(settingsItem);
}
```

### Per-Screen Visual Setup

```csharp
public class HomeScreen : NavigationScreen
{
    protected override void SetupAppBar(AppBar appBar, NavController navController)
    {
        appBar.title = "Home";

        var menuButton = new ActionButton { icon = "menu", label = "Menu" };
        menuButton.clickable.clicked += () => navController.Navigate("settings");
        appBar.Add(menuButton);
    }
}
```

### Nested Navigation Graphs

```csharp
// Main graph with embedded settings graph
var mainGraph = new NavGraph { name = "main", startDestination = homeDestination };
mainGraph.destinations.Add(homeDestination);

var settingsGraph = new NavGraph { name = "settings", startDestination = settingsHomeDestination };
settingsGraph.destinations.Add(settingsHomeDestination);

// Navigate to nested graph
navController.Navigate("settings");
```

## Important Patterns to Avoid

1. **Don't forget to set start destination** - Every graph must have one
2. **Don't mix global and local action logic** - Keep them separate
3. **Don't modify destination.label at runtime** - It won't affect AppBar title, use the visual controller
4. **Don't assume OnEnter/OnExit are called multiple times** - They're called per navigation
5. **Don't navigate without a valid NavController reference** - Ensure it's properly initialized

## Integration with App Builder

```csharp
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder builder)
    {
        base.OnConfiguringApp(builder);

        // Setup navigation resources
        var navGraphAsset = Resources.Load<NavGraphViewAsset>("Navigation/MyNavGraph");
        builder.services.AddSingleton(navGraphAsset);
    }

    protected override void OnAppReady(MyApp app)
    {
        base.OnAppReady(app);

        // Setup NavHost
        var navHost = new NavHost();
        navHost.visualController = new MyNavVisualController();

        var navGraphAsset = app.GetService<NavGraphViewAsset>();
        navHost.LoadGraph(navGraphAsset);

        root.Add(navHost);
    }
}
```

## Code Generation Best Practices

Use the Navigation Graph Editor's code generator to create strongly-typed constants:

```csharp
// Generated code provides constants for all destinations, actions, and graphs
public partial static class Actions
{
    public const string NavigateToHome = "navigateToHome";
    public const string NavigateToSettings = "navigateToSettings";
}

public partial class Destinations
{
    public const string Home = "home";
    public const string Settings = "settings";
}

// Use constants to avoid typos
navController.Navigate(Destinations.Home);
```

## File Locations

- Navigation Graph Assets: `Assets/Resources/Navigation/`
- Navigation Screens: `Assets/Scripts/Navigation/Screens/`
- Visual Controllers: `Assets/Scripts/Navigation/Controllers/`
- Generated Code: `Assets/Scripts/Navigation/Generated/` (created by code generator)

## Reference Documentation

For detailed information, see:
- [reference.md](reference.md) - Complete API reference with all methods and properties
- [examples/](examples/) - Working code examples
