---
name: app-ui
description: Expert assistant for developing UI in Unity using App UI framework. Use when creating components, styling, MVVM architecture, navigation, or any UI-related tasks.
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# App UI Expert for Unity

Expert assistant for building user interfaces in Unity using the App UI framework.

## Overview

App UI is Unity's comprehensive framework for building beautiful, high-performance user interfaces. It supports multi-platform development (Android, iOS, Windows, macOS, WebGL) and is built on UI Toolkit.

## Core Requirements

- Unity 2021.3 LTS or later
- Familiarity with UI Toolkit

## Key Namespace

```csharp
using Unity.AppUI.UI;
```

## UXML Namespace Declaration

```xml
<UXML xmlns="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    <appui:Panel>
        <!-- Your UI elements -->
    </appui:Panel>
</UXML>
```

## Essential Patterns

### 1. Always Start with Panel

The Panel component is mandatory - it provides context (theme, language, layout direction) and the layering system:

```xml
<appui:Panel>
    <appui:Button title="Hello World!" />
</appui:Panel>
```

### 2. Theme Setup

Reference a theme in your PanelSettings:
- Default: `Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss`
- Variants: Dark/Light, Small/Medium/Large, Editor variants

### 3. Component Usage

**Buttons and Actions:**
```xml
<appui:Button title="Click me" />
<appui:IconButton icon="add" />
<appui:ActionButton icon="edit" label="Edit" />
```

**Always use clickable for events (not ClickEvent):**
```csharp
button.clickable.clicked += () => Debug.Log("Clicked");
```

**Input Components:**
```xml
<appui:TextField />
<appui:TextArea />
<appui:Checkbox />
<appui:Toggle />
<appui:Dropdown />
<appui:SliderFloat low-value="0" high-value="100" />
```

**Layout Components:**
```xml
<appui:StackView />
<appui:SwipeView />
<appui:PageView />
<appui:SplitView direction="Horizontal" />
```

### 4. MVVM Architecture

**Observable ViewModel:**
```csharp
[ObservableObject]
public partial class MyViewModel
{
    [ObservableProperty]
    private string _name;

    [RelayCommand]
    void Submit()
    {
        // Handle submit
    }
}
```

**App Builder Pattern:**
```csharp
public class MyAppBuilder : UIToolkitAppBuilder<MyApp>
{
    protected override void OnConfiguringApp(AppBuilder builder)
    {
        base.OnConfiguringApp(builder);
        builder.services.AddTransient<MainPage>();
        builder.services.AddTransient<MainViewModel>();
    }
}
```

### 5. Redux State Management

```csharp
// Define state
public record CounterState { public int Count { get; init; } }

// Create store with slices
var store = StoreFactory.CreateStore(new[] {
    StoreFactory.CreateSlice("counter", new CounterState(), builder => {
        builder.AddCase(Actions.Increment, (state, action) =>
            state with { Count = state.Count + 1 });
    })
});

// Subscribe and dispatch
store.Subscribe<CounterState>("counter", state => Debug.Log(state.Count));
store.Dispatch(Actions.Increment.Invoke());
```

### 6. Overlays and Popups

```csharp
// Popover
var popover = Popover.Build(target, content)
    .SetPlacement(PopoverPlacement.Bottom)
    .SetArrowVisible(true);
popover.Show();

// Modal
var modal = Modal.Build(content)
    .SetFullScreenMode(ModalFullScreenMode.None);
modal.Show();

// Toast
var toast = Toast.Build(panel, "Message", NotificationDuration.Short);
toast.Show();

// Menu
MenuBuilder.Build(anchor)
    .AddAction(1, "Item", "icon", evt => Debug.Log("clicked"))
    .Show();
```

### 7. Navigation

```csharp
// Setup NavHost with visual controller
var navHost = new NavHost();
navHost.visualController = new MyNavController();

// Navigate
navController.Navigate("destinationName");
```

### 8. Styling with USS

**BEM naming convention:**
```css
.appui-button {
    padding: 0 var(--appui-spacing-100);
}
```

**Custom themes:**
```css
.appui--myTheme {
    --appui-primary-100: #E3F2FD;
}
```

### 9. Custom Icons

```css
.appui-icon--myicon--regular {
    --unity-image: url("path/to/icon.png");
}
```

```xml
<appui:Icon name="myicon" variant="Regular" />
```

### 10. Localization

```csharp
// Get localized string
var ctx = element.GetContext<LangContext>();
var text = await ctx.GetLocalizedStringAsync("@tableName:entryKey");
```

## Important Guidelines

1. **Always use Panel as root** - Required for contexts and overlays
2. **Use clickable.clicked** - Not RegisterCallback<ClickEvent>
3. **Use USS variables** - From themes for consistent styling
4. **Use source generators** - [ObservableObject], [ObservableProperty], [RelayCommand]
5. **RadioGroup uses string keys** - Not int indices
6. **Use AddCase for reducers** - Not Add method

## File Locations

- Themes: `Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/`
- Icons: `Packages/com.unity.dt.app-ui/PackageResources/Icons/`
- Fonts: `Packages/com.unity.dt.app-ui/PackageResources/Fonts/`

## Reference Documentation

For detailed information, see:
- [reference.md](reference.md) - Complete API patterns
- [examples/](examples/) - Code examples

## Related Skills

This skill serves as the general-purpose entry point for App UI development, covering the fundamentals of all aspects of the framework. For specialized guidance on particular topics, consider enabling the corresponding skill:

- **app-ui-navigation** - Deep dive into navigation system (NavGraph, NavHost, visual controllers)
- **app-ui-redux** - Redux state management (Store, Slices, AsyncThunks)
- **app-ui-mvvm** - MVVM pattern and dependency injection (ObservableObject, AppBuilder, DI)
- **app-ui-theming** - Theming and styling (custom themes, USS variables, dark/light mode)

If the user needs detailed help with any of these topics, suggest they enable the corresponding skill for more comprehensive guidance. These skills provide deeper patterns, more examples, and specialized expertise.
