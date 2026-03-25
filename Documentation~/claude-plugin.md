---
uid: claude-plugin
---

# Claude Code Plugin

App UI includes a Claude Code plugin that provides AI-assisted development for building user interfaces. The plugin includes specialized skills that help you write App UI code faster and with fewer errors.

## What is Claude Code?

[Claude Code](https://code.claude.com/docs) is a command-line tool that brings AI assistance directly into your development workflow. It can read your codebase, understand context, and help you write, debug, and refactor code.

## Installation

### Prerequisites

1. **Claude Code CLI** - Install Claude Code following the [official installation guide](https://code.claude.com/docs/en/quickstart)
2. **App UI Package** - Ensure you have the App UI package installed in your Unity project

### Installing Skills via Package Manager (Recommended)

App UI includes a Package Manager extension that lets you install AI Agent skills directly from the Unity Editor.

1. Open **Window > Package Manager** in Unity
2. Select the **App UI** package
3. In the package detail pane, expand the **AI Agent Skills** foldout
4. Click **Install All** to install every available skill, or click **Install** next to individual skills

The extension copies skill files into your `.claude/skills` directory. It walks up from your project root looking for an existing `.claude` folder; if none is found, it defaults to `<project-root>/.claude/skills`.

> [!NOTE]
> If no `.claude` folder exists in or above your project, skills are installed relative to the project root. A warning is displayed if the resolved path falls under your home directory (`~/.claude/skills`). Consider creating a `.claude` folder in your project to keep skills project-scoped.

The extension also detects when installed skills are **outdated** (the package contains newer files) and offers an **Update** button. You can remove individual skills or use **Remove All** to uninstall them.

### Installing the Plugin (CLI)

The Claude Code plugin is located in the `Plugins~` folder of the App UI package. There are two ways to install it via the CLI:

#### Option 1: Add as a Marketplace

This method allows you to install and manage the plugin through Claude Code's plugin system.

1. Open Claude Code in your Unity project root directory
2. Add the plugin directory as a marketplace:

```
/plugin marketplace add ./Packages/com.unity.dt.app-ui/Plugins~
```

3. Install the plugin:

```
/plugin install app-ui@unity-app-ui
```

> [!NOTE]
> If you installed App UI via git URL or a local path, adjust the path accordingly to point to your App UI package location.

4. Verify the installation by running `/plugin` and checking the **Installed** tab.

#### Option 2: Load Directly (For Development/Testing)

You can load the plugin directly when starting Claude Code:

```bash
claude --plugin-dir ./Packages/com.unity.dt.app-ui/Plugins~
```

This loads the plugin for the current session without permanent installation.

## Available Skills

The App UI plugin provides multiple specialized skills to assist with different aspects of UI development:

### Core Skill

| Skill | Description |
|-------|-------------|
| **app-ui** | General-purpose App UI expert covering components, styling, MVVM, navigation, and overlays. This is the primary skill and entry point. |

### Specialized Skills

| Skill | Description |
|-------|-------------|
| **app-ui-navigation** | Deep expertise in the navigation system including NavGraph, NavHost, NavController, NavDestinations, and visual controllers (AppBar, Drawer, BottomNavBar, NavigationRail). |
| **app-ui-redux** | Redux state management patterns including Store, Slices, Reducers, Actions, AsyncThunks, middleware, and Redux DevTools integration. |
| **app-ui-mvvm** | MVVM architecture and dependency injection including ObservableObject, ObservableProperty, RelayCommand, UIToolkitAppBuilder, and service registration. |
| **app-ui-theming** | Theming and styling including USS variables, custom themes, dark/light mode switching, scale contexts, and BEM conventions. |

## Using Skills

Once the plugin is installed, skills are available automatically. Claude will use them when relevant to your task, or you can invoke them directly.

### Automatic Invocation

Claude automatically loads skills based on what you're working on. For example, if you ask about navigation, Claude will load the **app-ui-navigation** skill automatically.

### Direct Invocation

You can invoke a skill directly using slash commands. Plugin skills are namespaced with the plugin name:

```
/app-ui:app-ui
```

Or invoke specialized skills:

```
/app-ui:app-ui-navigation
/app-ui:app-ui-redux
/app-ui:app-ui-mvvm
/app-ui:app-ui-theming
```

> [!TIP]
> The core **app-ui** skill provides a solid foundation and will suggest specialized skills when appropriate. You can also invoke specialized skills directly when you need deeper guidance on specific topics like navigation flows or state management.

## Usage Examples

### Creating a New Screen with Navigation

Simply ask Claude about navigation and it will automatically use the relevant skills:

```
Create a settings screen with a back button that navigates to the home screen.
Include toggles for dark mode and notifications.
```

Or invoke the navigation skill directly:

```
/app-ui:app-ui-navigation Create a settings screen with navigation
```

### Setting Up Redux State Management

```
Create a Redux store for managing user authentication state.
Include login/logout actions and async thunks for API calls.
```

Or invoke directly:

```
/app-ui:app-ui-redux Set up authentication state management
```

### Building a Custom Theme

```
Create a custom dark theme with a blue primary color palette
and larger spacing for touch-friendly interfaces.
```

Or invoke directly:

```
/app-ui:app-ui-theming Create a custom dark theme with blue palette
```

### Implementing MVVM Architecture

```
Set up an AppBuilder with dependency injection for a todo list app.
Create a TodoViewModel with observable properties and commands.
```

Or invoke directly:

```
/app-ui:app-ui-mvvm Set up MVVM for a todo list app
```

## Skill Content

Each skill includes:

- **SKILL.md** - Overview, key concepts, and essential patterns
- **reference.md** - Comprehensive API reference and detailed documentation
- **examples/** - Ready-to-use code examples

You can browse the skill files directly in:

```
Packages/com.unity.dt.app-ui/Plugins~/skills/
```

## Best Practices

1. **Let Claude choose skills** - Claude automatically loads relevant skills based on your task. The core **app-ui** skill covers most use cases.

2. **Use direct invocation for specific topics** - When you need deep expertise on navigation, theming, or state management, invoke the specialized skill directly.

3. **Reference existing code** - Point Claude to your existing UXML files or C# scripts for context-aware suggestions.

4. **Iterate on examples** - Use the provided examples as starting points and ask Claude to customize them for your needs.

## Troubleshooting

### AI Agent Skills foldout not visible in Package Manager

The foldout only appears for packages that contain both a `Plugins~/skills` directory and a `Plugins~/skills.json` manifest. Verify these exist:

```bash
ls ./Packages/com.unity.dt.app-ui/Plugins~/skills/
ls ./Packages/com.unity.dt.app-ui/Plugins~/skills.json
```

### Plugin not found (CLI)

If Claude cannot find the plugin, verify the path:

```bash
ls ./Packages/com.unity.dt.app-ui/Plugins~/.claude-plugin
```

You should see `plugin.json` and `marketplace.json`.

### Skills not loading

Ensure the skills directory exists:

```bash
ls ./Packages/com.unity.dt.app-ui/Plugins~/skills/
```

You should see directories for each skill (app-ui, app-ui-navigation, etc.).

If skills still don't appear, try clearing the plugin cache and reinstalling:

```bash
rm -rf ~/.claude/plugins/cache
```

Then restart Claude Code and reinstall the plugin.

### Checking for errors

Run `/plugin` in Claude Code and check the **Errors** tab to see any plugin loading issues.

### Updating skills

When you update the App UI package, skills installed via the **Package Manager extension** can be updated by reopening the AI Agent Skills foldout — outdated skills will show an **Update** button. You can also click **Update All** to update every skill at once.

If you installed the plugin via the CLI as a marketplace, you can refresh by running:

```
/plugin marketplace update unity-app-ui
```

## Additional Resources

- [Claude Code Documentation](https://code.claude.com/docs)
- [Claude Code Skills](https://code.claude.com/docs/en/skills)
- [Claude Code Plugins](https://code.claude.com/docs/en/plugins)
- [App UI Documentation](xref:overview)
- [MVVM Introduction](xref:mvvm-intro)
- [Navigation System](xref:navigation)
- [State Management](xref:state-management)
- [Theming](xref:theming)
