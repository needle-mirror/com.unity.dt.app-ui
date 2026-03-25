---
name: app-ui
description: "Expert assistant for developing UI in Unity using App UI framework. Use when creating components, styling, MVVM architecture, navigation, or any UI-related tasks. Trigger this skill whenever the user mentions App UI, Unity UI Toolkit components like Panel/Button/TextField, UXML layouts with appui namespace, USS styling with appui variables, overlays (popover, modal, toast, menu), or building any Unity user interface. Also trigger when the user asks to create a form, dialog, settings screen, dashboard, or any visual component in Unity."
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

The Panel component is mandatory -- it provides context (theme, language, layout direction) and the layering system for overlays. Without Panel as the root, contexts and overlays will not work.

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

### 4. Overlays and Popups

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

### 5. Localization

```csharp
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

Consult [reference.md](reference.md) when you need exact API signatures, component property tables, context system details, or platform integration methods beyond what's covered above. See [examples/](examples/) for complete working code samples.

## Specialized Skills

This skill covers App UI fundamentals -- components, overlays, and general patterns. For tasks that go deeper into specific areas, also consult the corresponding specialized skill:

- **app-ui-navigation** - Consult for NavGraph, NavHost, NavController, screen navigation, AppBar/Drawer/BottomNavBar/NavigationRail setup, back stack management, and nested graphs.
- **app-ui-redux** - Consult for Redux Store, Slices, Reducers, ActionCreators, AsyncThunks, middleware, subscriptions, and Redux DevTools.
- **app-ui-mvvm** - Consult for ObservableObject, ObservableProperty, RelayCommand, AppBuilder, service registration, dependency injection, and data binding.
- **app-ui-theming** - Consult for custom themes, USS variables/design tokens, dark/light mode, scale factors, BEM conventions, custom icons, and custom typography.
