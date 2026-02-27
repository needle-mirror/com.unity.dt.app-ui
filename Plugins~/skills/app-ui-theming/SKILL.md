---
name: app-ui-theming
description: Expert for App UI theming and styling - USS variables, custom themes, dark/light mode, scale factors, and BEM conventions.
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# App UI Theming Expert

Master the App UI theming system to customize colors, typography, spacing, and create consistent, branded UI experiences.

## Overview

App UI provides a powerful theming system based on USS (Unity Style Sheet) variables and design tokens. This allows you to create cohesive visual experiences across your applications while maintaining consistency through a comprehensive design system.

## When to Use This Skill

Use this expert when you need to:
- Create custom themes for your application
- Override default colors, typography, or spacing
- Implement dark/light mode switching
- Configure scale factors for responsive UI
- Register custom icons or fonts
- Apply BEM naming conventions to custom styles
- Understand USS design tokens and variables

## Key Concepts

### Theme System Architecture

The App UI theming system is built on layered USS stylesheets:

1. **Palette** (`App UI - Palette.uss`) - Base color definitions
2. **Aliases** (`App UI - Alias.uss`) - Semantic color mappings
3. **Theme Classes** (`App UI - Dark.uss`, `App UI - Light.uss`) - Theme-specific overrides
4. **Scale Variants** (`App UI - Small.uss`, `App UI - Medium.uss`, `App UI - Large.uss`) - Responsive scaling

### Built-in Themes

App UI provides four built-in themes, each available in three scale variants:

- **light** - Light theme for daytime/bright environments
- **dark** - Dark theme for night-time/dark environments (default)
- **editor-light** - Editor-optimized light theme
- **editor-dark** - Editor-optimized dark theme

### Scale Contexts

Scale contexts adjust UI size based on device pixel density:

- **small** - For compact displays
- **medium** - Standard scaling (default)
- **large** - For accessibility and larger displays

## Essential Patterns

### 1. Theme File Structure

A complete custom theme requires two files:

**Stylesheet (.uss)** - Contains USS variables with the custom theme class:
```css
.appui--customThemeName {
    --appui-primary-100: #E3F2FD;
    --appui-primary-200: #BBDEFB;
    --appui-primary-300: #90CAF9;
    /* ... more color overrides ... */
}
```

**Theme File (.tss)** - References the stylesheet for use in PanelSettings:
```css
@import url("custom-theme.uss");

VisualElement {}
```

### 2. Required Theme Variable Prefixes

Always use the `appui--` prefix for custom theme class names:

```css
/* Correct */
.appui--mytheme { }
.appui--brandColors { }

/* Incorrect */
.mytheme { }
.custom-theme { }
```

### 3. USS Design Tokens

App UI provides organized design tokens across multiple categories:

**Color Tokens:**
- Primary colors: `--appui-primary-*` (25 through 1300)
- Accent colors: `--appui-accent-*`
- Semantic colors: `--appui-positive-*`, `--appui-warning-*`, `--appui-destructive-*`
- Backgrounds: `--appui-backgrounds-*`
- Foregrounds: `--appui-foregrounds-*`

**Spacing Tokens:**
- `--appui-spacing-*` for padding, margin, gaps
- Range from 50 to 800 in increments

**Typography:**
- `--appui-font-weights-100` - Regular weight
- `--appui-font-weights-200` - SemiBold weight
- Font family customization via `:root` selector

### 4. Runtime Theme Switching

Switch themes at runtime using ThemeContext:

```csharp
// Get the current theme context
var element = GetThemeAwareElement();
var themeContext = element.GetContext<ThemeContext>();

// Switch to different theme
element.ProvideContext(new ThemeContext("light"));
element.ProvideContext(new ThemeContext("dark"));
element.ProvideContext(new ThemeContext("appui--customTheme"));
```

### 5. Scale Context for Responsive UI

Adjust UI size based on device scale:

```csharp
// Get scale context
var scaleContext = element.GetContext<ScaleContext>();

// Change scale at runtime
element.ProvideContext(new ScaleContext("small"));
element.ProvideContext(new ScaleContext("medium"));
element.ProvideContext(new ScaleContext("large"));
```

### 6. BEM Naming Convention

App UI follows BEM (Block, Element, Modifier) for all CSS class names:

```css
/* Block */
.appui-button { }

/* Element */
.appui-button__icon { }

/* Modifier */
.appui-button--primary { }
.appui-button--disabled { }

/* Combined */
.appui-button__icon--right { }
```

When creating custom styles:
```css
.appui-mycomponent { }
.appui-mycomponent__header { }
.appui-mycomponent--dark { }
.appui-mycomponent__header--highlighted { }
```

### 7. Custom Icon Registration

Register custom icons following App UI naming conventions:

```css
.appui-icon--myicon--regular {
    --unity-image: url("path/to/myicon.png");
}

.appui-icon--myicon--bold {
    --unity-image: url("path/to/myicon-bold.png");
}
```

Icon variants supported:
- regular
- bold
- light
- duotone
- thin
- fill

Use in UXML:
```xml
<appui:Icon name="myicon" variant="Regular" />
<appui:Icon name="myicon" variant="Bold" />
```

### 8. Custom Typography

Override default fonts at the root level:

```css
:root {
    --appui-font-weights-100: url("path/to/CustomFont-Regular.ttf");
    --appui-font-weights-200: url("path/to/CustomFont-SemiBold.ttf");
}
```

Or use Unity Font Assets for better fallback support:
```css
:root {
    --appui-font-weights-100: resource("Fonts/MyCustomFont");
}
```

### 9. PanelSettings Configuration

Always reference your theme file in PanelSettings:

1. Create a Theme file (.tss) in your project
2. Open the UIDocument Inspector
3. Select the PanelSettings component
4. Drag your Theme file into the "Theme Style Sheet" field
5. Ensure the Panel root element is present in your UXML

### 10. Creating a Complete Custom Theme

Full example structure:

```
Assets/
  MyThemeAssets/
    MyTheme.tss (Theme file)
    MyTheme.uss (Stylesheet)
```

**MyTheme.uss:**
```css
@import url("App UI - Palette.uss");

.appui--mytheme {
    --appui-primary-100: #E8F4F8;
    --appui-primary-200: #B8DFF0;
    --appui-primary-300: #88CAE8;
    /* Complete palette override */
}

/* Custom component styles */
.appui-mycomponent {
    padding: var(--appui-spacing-200);
    background-color: var(--appui-backgrounds-100);
}
```

**MyTheme.tss:**
```css
@import url("MyTheme.uss");

VisualElement {}
```

## Important Guidelines

1. **Always use `appui--` prefix** - Required for proper theme identification and context switching
2. **Import base palette when extending** - Use `@import url("App UI - Palette.uss")` for color variable access
3. **Use design tokens consistently** - Prefer USS variables over hardcoded values
4. **Plan color hierarchy** - Define primary, secondary, accent, and semantic colors
5. **Test both light and dark modes** - Ensure readability in all theme variants
6. **Document custom variables** - Add comments explaining your theme variables
7. **Validate scale factors** - Test UI scaling with small/medium/large context values
8. **Use resource() for high DPI assets** - When supporting multiple DPI densities

## Color Token Density

App UI uses a 25-step color density scale (25, 50, 75... up to 1300) allowing:
- Fine-grained color control
- Predictable color progression
- Easy light/dark theme mapping
- Accessible contrast ratios

## File Locations

- **Default Themes**: `Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/`
- **Icons**: `Packages/com.unity.dt.app-ui/PackageResources/Icons/`
- **Fonts**: `Packages/com.unity.dt.app-ui/PackageResources/Fonts/`

## Reference Documentation

For detailed information, see:
- [reference.md](reference.md) - Complete theming API and variable reference
- [examples/](examples/) - Custom theme examples and templates
