# App UI Theming Reference

Complete reference guide for App UI theming system including USS variables, design tokens, theme management, and customization.

## Table of Contents

- [Theme File Structure](#theme-file-structure)
- [Built-in Themes](#built-in-themes)
- [USS Design Tokens](#uss-design-tokens)
- [Color Tokens](#color-tokens)
- [Spacing Tokens](#spacing-tokens)
- [Typography Tokens](#typography-tokens)
- [Creating Custom Themes](#creating-custom-themes)
- [Runtime Theme Switching](#runtime-theme-switching)
- [Scale Contexts](#scale-contexts)
- [BEM Naming Convention](#bem-naming-convention)
- [Custom Icons](#custom-icons)
- [Custom Typography](#custom-typography)
- [PanelSettings Configuration](#panelsettings-configuration)
- [Theme Context API](#theme-context-api)
- [Scale Context API](#scale-context-api)
- [Complete Examples](#complete-examples)

## Theme File Structure

### Overview

A complete theme in App UI consists of:

1. **Theme File (.tss)** - Imported by PanelSettings
2. **Stylesheet (.uss)** - Contains USS variables and styles
3. **Optional component overrides** - Custom component-specific styles

### Theme File (.tss)

```css
/* MyAppTheme.tss */
@import url("MyAppTheme.uss");

VisualElement {}
```

Purpose:
- Serves as the single theme reference point
- Imported by PanelSettings in the Inspector
- Aggregates all related stylesheets

### Stylesheet (.uss)

```css
/* MyAppTheme.uss */
@import url("App UI - Palette.uss");

.appui--myapptheme {
    /* Theme-specific variable overrides */
    --appui-primary-100: #E3F2FD;
    --appui-primary-200: #BBDEFB;

    /* Semantic color assignments */
    --appui-base-500: #1976D2;

    /* Component-specific overrides */
    --appui-spacing-100: 8px;
}

/* Custom component styles */
.appui-mycomponent {
    padding: var(--appui-spacing-200);
    background-color: var(--appui-backgrounds-100);
    border-radius: var(--appui-radius-m);
}
```

## Built-in Themes

### Available Themes

```
App UI.tss
├── App UI - Dark.uss (default dark theme)
├── App UI - Light.uss (default light theme)
├── App UI - Editor Dark.uss (editor UI optimization - dark)
└── App UI - Editor Light.uss (editor UI optimization - light)
```

### Theme Variants

Each theme has three scale variants:

```
App UI - Dark - Small.tss    (small scale, dark)
App UI - Dark - Medium.tss   (medium scale, dark) [default for dark]
App UI - Dark - Large.tss    (large scale, dark)

App UI - Light - Small.tss   (small scale, light)
App UI - Light - Medium.tss  (medium scale, light)
App UI - Light - Large.tss   (large scale, light)
```

### Selecting Themes in PanelSettings

**Default Runtime Theme:**
```
Path: Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss
```

**Custom Dark Theme (Medium):**
```
Path: Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI - Dark - Medium.tss
```

**Custom Light Theme (Large):**
```
Path: Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI - Light - Large.tss
```

## USS Design Tokens

### Token Organization

App UI organizes tokens in a hierarchical structure:

```
:root (Global variables)
├── Color Palette (base colors)
├── Semantic Colors (semantic meanings)
├── Component Tokens (component-specific)
└── Responsive Scales (size variants)
```

### Variable Naming Convention

```
--appui-{category}-{subcategory}-{density}
--appui-primary-100
--appui-spacing-200
--appui-font-weights-100
```

## Color Tokens

### Primary Color Range

```css
:root {
    --appui-primary-1300: #ffffff;      /* Lightest */
    --appui-primary-1200: #e9e9e9;
    --appui-primary-1100: #d3d3d3;
    --appui-primary-1000: #bcbcbc;
    --appui-primary-900: #a6a6a6;
    --appui-primary-800: #8f8f8f;
    --appui-primary-700: #7a7a7a;
    --appui-primary-600: #636363;
    --appui-primary-500: #4d4d4d;
    --appui-primary-400: #363636;
    --appui-primary-300: #202020;
    --appui-primary-200: #131313;
    --appui-primary-100: #0a0a0a;       /* Darkest */
}
```

### Semantic Color Groups

**Positive (Success):**
```css
--appui-positive-300: #10B981;  /* Darkest */
--appui-positive-200: #34D399;
--appui-positive-100: #6EE7B7;
--appui-positive-50: #A7F3D0;
--appui-positive-25: #D1FAE5;  /* Lightest */
```

**Warning:**
```css
--appui-warning-300: #F59E0B;
--appui-warning-200: #FBBF24;
--appui-warning-100: #FCD34D;
--appui-warning-50: #FDE68A;
--appui-warning-25: #FEF3C7;
```

**Destructive (Danger):**
```css
--appui-destructive-300: #EF4444;
--appui-destructive-200: #F87171;
--appui-destructive-100: #FCA5A5;
--appui-destructive-50: #FECACA;
--appui-destructive-25: #FEE2E2;
```

**Accent (Interactive):**
```css
--appui-accent-300: #3B82F6;
--appui-accent-200: #60A5FA;
--appui-accent-100: #93C5FD;
--appui-accent-50: #BFDBFE;
--appui-accent-25: #DBEAFE;
```

### Background and Foreground Colors

**Backgrounds:**
```css
--appui-backgrounds-300: #202020;  /* Darkest background */
--appui-backgrounds-200: #272727;
--appui-backgrounds-100: #2E2E2E;  /* Default background */
--appui-backgrounds-50: #363636;
--appui-backgrounds-25: #3D3D3D;   /* Lightest background */
```

**Foregrounds (Text):**
```css
--appui-foregrounds-200: #E9E9E9;  /* Darkest text */
--appui-foregrounds-100: #D3D3D3;  /* Default text */
--appui-foregrounds-50: #B3B3B3;
--appui-foregrounds-25: #8F8F8F;   /* Lightest text */
```

### Color Palette Categories

Available color palettes:
- Gray tones
- Dark Gray
- Light Gray
- Red/Pink
- Orange/Amber
- Yellow
- Green
- Teal
- Blue
- Purple
- Transparent (white/black with opacity)

## Spacing Tokens

### Standard Spacing Scale

```css
:root {
    --appui-spacing-50: 2px;      /* Extra tight */
    --appui-spacing-75: 4px;
    --appui-spacing-100: 8px;     /* Tight */
    --appui-spacing-150: 12px;
    --appui-spacing-200: 16px;    /* Standard */
    --appui-spacing-250: 20px;
    --appui-spacing-300: 24px;    /* Relaxed */
    --appui-spacing-400: 32px;
    --appui-spacing-500: 40px;    /* Loose */
    --appui-spacing-600: 48px;
    --appui-spacing-700: 56px;
    --appui-spacing-800: 64px;    /* Very loose */
}
```

### Usage Example

```css
.appui-mycomponent {
    padding: var(--appui-spacing-200);          /* 16px */
    margin-bottom: var(--appui-spacing-300);    /* 24px */
    gap: var(--appui-spacing-100);              /* 8px */
}
```

## Typography Tokens

### Font Weight Variables

```css
:root {
    --appui-font-weights-100: url("../../Fonts/Inter-Regular.ttf");
    --appui-font-weights-200: url("../../Fonts/Inter-SemiBold.ttf");
}
```

### Font Family Usage

```css
.appui-mycomponent {
    font-size: 14px;
    -unity-font: var(--appui-font-weights-100);  /* Regular weight */
    -unity-font-style: normal;
}

.appui-mycomponent--bold {
    -unity-font: var(--appui-font-weights-200);  /* SemiBold weight */
    -unity-font-style: bold;
}
```

### Custom Typography Setup

```css
:root {
    --appui-font-weights-100: resource("Fonts/CustomFont-Regular");
    --appui-font-weights-200: resource("Fonts/CustomFont-Bold");
}
```

## Creating Custom Themes

### Step 1: Plan Your Color Palette

Define your brand colors and map to the 25-step density scale:

```
Brand Primary Color: #0066CC
├── 1300 (Lightest): #E6F0FF
├── 1200: #CCE0FF
├── 1100: #B3D1FF
├── ... (17 more steps)
└── 100 (Darkest): #003366
```

### Step 2: Create the Stylesheet

```css
/* CustomBrandTheme.uss */
@import url("App UI - Palette.uss");

.appui--custombrand {
    /* Override primary colors */
    --appui-primary-100: #003366;
    --appui-primary-200: #004080;
    --appui-primary-300: #004D99;
    --appui-primary-400: #0066CC;
    --appui-primary-500: #1A75D9;
    --appui-primary-600: #3385E0;
    --appui-primary-700: #4D94E6;
    --appui-primary-800: #66A3ED;
    --appui-primary-900: #80B3F3;
    --appui-primary-1000: #99C2FA;
    --appui-primary-1100: #B3D1FF;
    --appui-primary-1200: #CCE0FF;
    --appui-primary-1300: #E6F0FF;

    /* Override accent colors */
    --appui-accent-300: #FF9500;
    --appui-accent-200: #FFB347;
    --appui-accent-100: #FFD187;
    --appui-accent-50: #FFE6B3;
    --appui-accent-25: #FFF0D9;

    /* Override semantic colors */
    --appui-positive-300: #2ECC71;
    --appui-warning-300: #F39C12;
    --appui-destructive-300: #E74C3C;

    /* Assign semantic tokens */
    --appui-base-500: #0066CC;
    --appui-backgrounds-100: #1A1A2E;
    --appui-backgrounds-50: #222237;
    --appui-foregrounds-100: #E8E8E8;
}

/* Custom component styles */
.appui-brandsection {
    padding: var(--appui-spacing-300);
    background-color: var(--appui-backgrounds-100);
    border-left: 4px solid var(--appui-accent-300);
}

.appui-brandsection__title {
    font-size: 18px;
    color: var(--appui-primary-400);
    -unity-font: var(--appui-font-weights-200);
}
```

### Step 3: Create the Theme File

```css
/* CustomBrandTheme.tss */
@import url("CustomBrandTheme.uss");

VisualElement {}
```

### Step 4: Reference in PanelSettings

1. Select the UIDocument component
2. Find PanelSettings in the Inspector
3. Assign your `CustomBrandTheme.tss` to the Theme Style Sheet field

## Runtime Theme Switching

### Using Theme Context

```csharp
using Unity.AppUI.Core;
using Unity.AppUI.UI;

public class ThemeSwitcher : MonoBehaviour
{
    private VisualElement _rootElement;

    public void SwitchToDarkTheme()
    {
        _rootElement.ProvideContext(new ThemeContext("dark"));
    }

    public void SwitchToLightTheme()
    {
        _rootElement.ProvideContext(new ThemeContext("light"));
    }

    public void SwitchToCustomTheme()
    {
        _rootElement.ProvideContext(new ThemeContext("appui--custombrand"));
    }

    public void GetCurrentTheme()
    {
        var themeContext = _rootElement.GetContext<ThemeContext>();
        Debug.Log($"Current theme: {themeContext.name}");
    }
}
```

### Theme Context Structure

```csharp
public record ThemeContext(string name) : IContext
{
    public string name { get; } = name;
}
```

### Listening to Theme Changes

```csharp
public class ThemeAwareComponent : BaseVisualElement
{
    public ThemeAwareComponent()
    {
        RegisterContextChangedCallback<ThemeContext>(OnThemeChanged);
    }

    void OnThemeChanged(ContextChangedEvent<ThemeContext> evt)
    {
        if (evt.context == null) return;

        Debug.Log($"Theme changed to: {evt.context.name}");
        UpdateComponentForTheme(evt.context.name);
    }

    void UpdateComponentForTheme(string themeName)
    {
        // Update component based on theme
        if (themeName == "dark")
        {
            // Dark theme setup
        }
        else if (themeName == "light")
        {
            // Light theme setup
        }
    }
}
```

## Scale Contexts

### Available Scales

```csharp
// Scale definitions
public enum ScaleVariant
{
    Small,    // Compact UI
    Medium,   // Standard (default)
    Large     // Accessible/Large displays
}
```

### Applying Scale at Runtime

```csharp
using Unity.AppUI.Core;

public class ResponsiveUIManager : MonoBehaviour
{
    private VisualElement _rootElement;

    public void SetSmallScale()
    {
        _rootElement.ProvideContext(new ScaleContext("small"));
    }

    public void SetMediumScale()
    {
        _rootElement.ProvideContext(new ScaleContext("medium"));
    }

    public void SetLargeScale()
    {
        _rootElement.ProvideContext(new ScaleContext("large"));
    }

    public void AutoSelectScale()
    {
        var dpi = Screen.dpi;
        var scale = dpi switch
        {
            < 100 => "small",
            >= 100 and < 200 => "medium",
            _ => "large"
        };

        _rootElement.ProvideContext(new ScaleContext(scale));
    }
}
```

### Scale Variant Files

```
App UI - Small.uss     (compact spacing and text)
App UI - Medium.uss    (standard scaling)
App UI - Large.uss     (large spacing and text for accessibility)
```

### Scale-Specific Variables

Scale files override base spacing and typography:

```css
/* App UI - Large.uss */
:root {
    --appui-spacing-100: 12px;      /* Increased from 8px */
    --appui-spacing-200: 20px;      /* Increased from 16px */
    --appui-spacing-300: 32px;      /* Increased from 24px */

    /* Font sizes also increased */
    --appui-font-size-caption: 14px; /* Increased from 12px */
    --appui-font-size-body: 16px;    /* Increased from 14px */
}
```

## BEM Naming Convention

### BEM Structure

Block: Top-level component
```css
.appui-button { }
.appui-panel { }
.appui-dropdown { }
```

Element: Part of a block
```css
.appui-button__icon { }
.appui-button__label { }
.appui-dropdown__trigger { }
.appui-dropdown__menu { }
```

Modifier: Variation of a block or element
```css
.appui-button--primary { }
.appui-button--disabled { }
.appui-button--small { }
.appui-button__icon--right { }
.appui-dropdown--open { }
.appui-dropdown__menu--visible { }
```

### Complete BEM Example

```css
/* Block */
.appui-card {
    padding: var(--appui-spacing-300);
    background-color: var(--appui-backgrounds-50);
    border-radius: 8px;
}

/* Element: header */
.appui-card__header {
    margin-bottom: var(--appui-spacing-200);
    border-bottom: 1px solid var(--appui-primary-200);
}

/* Element: title */
.appui-card__title {
    font-size: 18px;
    -unity-font: var(--appui-font-weights-200);
    color: var(--appui-foregrounds-100);
}

/* Element: content */
.appui-card__content {
    color: var(--appui-foregrounds-50);
}

/* Modifier: elevated */
.appui-card--elevated {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

/* Modifier: compact */
.appui-card--compact {
    padding: var(--appui-spacing-200);
}

/* Combined: element with modifier */
.appui-card--compact .appui-card__title {
    font-size: 14px;
}
```

## Custom Icons

### Icon Naming Convention

```
.appui-icon--{name}--{variant}
```

Variants (lowercase):
- regular
- bold
- light
- duotone
- thin
- fill

### Registering Custom Icons

```css
/* CustomIcons.uss */

.appui-icon--home--regular {
    --unity-image: url("Assets/Icons/home.png");
}

.appui-icon--home--bold {
    --unity-image: url("Assets/Icons/home-bold.png");
}

.appui-icon--settings--regular {
    --unity-image: url("Assets/Icons/settings.png");
}

.appui-icon--arrow--light {
    --unity-image: url("Assets/Icons/arrow-light.png");
}
```

### Using Custom Icons in UXML

```xml
<appui:Icon name="home" variant="Regular" />
<appui:Icon name="home" variant="Bold" />
<appui:Icon name="settings" variant="Regular" />
<appui:Icon name="arrow" variant="Light" />
```

### High DPI Icon Support

For high DPI displays, provide @2x versions:

```
Assets/
  Icons/
    home.png          (normal DPI)
    home@2x.png       (high DPI)
    settings.png
    settings@2x.png
```

Register with `resource()`:

```css
.appui-icon--home--regular {
    --unity-image: resource("Icons/home");
}

.appui-icon--settings--regular {
    --unity-image: resource("Icons/settings");
}
```

Note: Icons must be in a `Resources` folder for this to work.

### Icon Browser Tool

App UI provides an Icon Browser for generating USS classes:

1. Window > App UI > Icon Browser
2. Create or select a USS file for icons
3. Drag and drop texture assets into the browser
4. Tool auto-generates USS classes

### Icon Folder Structure for Browser

```
Resources/
  MyIcons/
    Regular/
      home.png
      settings.png
    Bold/
      home.png
      settings.png
    Light/
      home.png
```

The folder name becomes the variant name.

## Custom Typography

### Overriding Default Fonts

```css
:root {
    --appui-font-weights-100: url("path/to/YourFont-Regular.ttf");
    --appui-font-weights-200: url("path/to/YourFont-SemiBold.ttf");
}
```

### Using Unity Font Assets

```css
:root {
    --appui-font-weights-100: resource("Fonts/MyFont");
}
```

### Component-Level Typography

```css
.appui-mycomponent {
    font-size: 14px;
    -unity-font: var(--appui-font-weights-100);
    -unity-font-style: normal;
    letter-spacing: 0.5px;
    word-spacing: 2px;
}

.appui-mycomponent--bold {
    -unity-font: var(--appui-font-weights-200);
    -unity-font-style: bold;
}
```

### Fallback Fonts

Use Unity Font Assets for better fallback handling:

1. Create a Font Asset in Unity (Right-click > Create > TextMesh Pro > Font Asset)
2. Assign your TTF file
3. Add fallback fonts
4. Reference in USS with `resource()`

## PanelSettings Configuration

### Step-by-Step Setup

1. **Create or select a UIDocument**
   - Right-click in Project > UI Toolkit > Panel

2. **Add PanelSettings Reference**
   - In the UIDocument component, ensure a PanelSettings is assigned
   - Create a new PanelSettings asset if needed

3. **Assign Theme File**
   - Select the PanelSettings asset
   - In the Inspector, set "Theme Style Sheet"
   - Browse to your theme .tss file

4. **Configure Panel Root**
   - In your UXML, ensure the root element is:
   ```xml
   <appui:Panel>
       <!-- Your UI here -->
   </appui:Panel>
   ```

5. **Set Scale (Optional)**
   - If using scaled themes, select the appropriate scale variant
   - Or override scale at runtime via ScaleContext

### PanelSettings Properties

```csharp
public class PanelSettings
{
    public StyleSheet themeStyleSheet { get; set; }  // Your theme file
    public int sortingOrder { get; set; }            // Layer ordering
    public float scale { get; set; }                 // Overall UI scale
    public Vector2Int resolution { get; set; }       // UI resolution
    public ScreenMatchMode screenMatchMode { get; }  // Scaling mode
}
```

## Theme Context API

### ThemeContext Class

```csharp
namespace Unity.AppUI.Core
{
    public record ThemeContext(string name) : IContext
    {
        public string name { get; }
    }
}
```

### Getting Theme Context

```csharp
var themeContext = element.GetContext<ThemeContext>();
var currentThemeName = themeContext?.name ?? "unknown";
```

### Providing Theme Context

```csharp
// Set to built-in theme
element.ProvideContext(new ThemeContext("dark"));
element.ProvideContext(new ThemeContext("light"));

// Set to custom theme
element.ProvideContext(new ThemeContext("appui--custombrand"));

// Remove context
element.ProvideContext<ThemeContext>(null);
```

### Listening to Theme Changes

```csharp
element.RegisterContextChangedCallback<ThemeContext>(OnThemeChanged);

void OnThemeChanged(ContextChangedEvent<ThemeContext> evt)
{
    Debug.Log($"Theme: {evt.context.name}");
}

// Unregister when done
element.UnregisterContextChangedCallback<ThemeContext>(OnThemeChanged);
```

## Scale Context API

### ScaleContext Class

```csharp
namespace Unity.AppUI.Core
{
    public record ScaleContext(string name) : IContext
    {
        public string name { get; }
    }
}
```

### Valid Scale Names

```csharp
public static class ScaleVariant
{
    public const string Small = "small";
    public const string Medium = "medium";   // default
    public const string Large = "large";
}
```

### Managing Scale Context

```csharp
// Get current scale
var scaleContext = element.GetContext<ScaleContext>();
var currentScale = scaleContext?.name ?? "medium";

// Set scale
element.ProvideContext(new ScaleContext("large"));

// Listen to scale changes
element.RegisterContextChangedCallback<ScaleContext>(OnScaleChanged);

void OnScaleChanged(ContextChangedEvent<ScaleContext> evt)
{
    Debug.Log($"Scale: {evt.context.name}");
    RefreshLayoutForScale(evt.context.name);
}
```

## Complete Examples

### Example 1: Complete Custom Theme

```css
/* BrandTheme.uss */
@import url("App UI - Palette.uss");

.appui--brandtheme {
    /* Primary brand colors */
    --appui-primary-100: #0A1628;
    --appui-primary-200: #132A4D;
    --appui-primary-300: #1C3D73;
    --appui-primary-400: #255199;
    --appui-primary-500: #2E65BF;
    --appui-primary-600: #3D78D9;
    --appui-primary-700: #5A8FED;
    --appui-primary-800: #78A6FF;
    --appui-primary-900: #96BFFF;
    --appui-primary-1000: #B4D9FF;
    --appui-primary-1100: #CCE8FF;
    --appui-primary-1200: #E5F3FF;
    --appui-primary-1300: #F5FAFF;

    /* Accent - vibrant orange */
    --appui-accent-300: #FF8C00;
    --appui-accent-200: #FFA630;
    --appui-accent-100: #FFB84D;
    --appui-accent-50: #FFCC80;
    --appui-accent-25: #FFE0B2;

    /* Semantic colors */
    --appui-positive-300: #27AE60;
    --appui-warning-300: #F39C12;
    --appui-destructive-300: #E74C3C;

    /* Component assignments */
    --appui-base-500: #255199;
    --appui-backgrounds-100: #0F1E30;
    --appui-backgrounds-50: #162A3F;
    --appui-foregrounds-100: #E8EFF7;
}

/* Custom section component */
.appui-section {
    padding: var(--appui-spacing-300);
    margin-bottom: var(--appui-spacing-200);
    background-color: var(--appui-backgrounds-50);
    border-radius: 8px;
}

.appui-section__header {
    margin-bottom: var(--appui-spacing-200);
}

.appui-section__title {
    font-size: 16px;
    -unity-font: var(--appui-font-weights-200);
    color: var(--appui-primary-400);
}

.appui-section__divider {
    height: 1px;
    background-color: var(--appui-primary-200);
}

.appui-section__content {
    color: var(--appui-foregrounds-100);
}

.appui-section--featured {
    border-left: 4px solid var(--appui-accent-300);
    background: linear-gradient(90deg,
        rgba(255, 140, 0, 0.05) 0%,
        transparent 100%);
}
```

### Example 2: Theme Switcher Component

```csharp
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class ThemeSwitcher : BaseVisualElement
{
    private Button _darkButton;
    private Button _lightButton;
    private Button _customButton;
    private Label _statusLabel;
    private VisualElement _parentElement;

    public ThemeSwitcher(VisualElement parent)
    {
        _parentElement = parent;

        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.gap = new Length(8, LengthUnit.Pixel);
        container.style.padding = new Thickness(16);

        _darkButton = new Button(() => SwitchTheme("dark")) { text = "Dark" };
        _lightButton = new Button(() => SwitchTheme("light")) { text = "Light" };
        _customButton = new Button(() => SwitchTheme("appui--brandtheme")) { text = "Brand" };

        _statusLabel = new Label();
        _statusLabel.style.marginTop = 10;

        container.Add(_darkButton);
        container.Add(_lightButton);
        container.Add(_customButton);
        Add(container);
        Add(_statusLabel);

        // Listen for theme changes
        RegisterContextChangedCallback<ThemeContext>(OnThemeChanged);
    }

    private void SwitchTheme(string themeName)
    {
        _parentElement.ProvideContext(new ThemeContext(themeName));
    }

    private void OnThemeChanged(ContextChangedEvent<ThemeContext> evt)
    {
        _statusLabel.text = $"Current Theme: {evt.context.name}";
    }
}
```

### Example 3: Responsive Scale Adjuster

```csharp
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

public class ResponsiveScaleManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _rootElement;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _rootElement = _uiDocument.rootVisualElement;

        // Set initial scale based on screen DPI
        AdjustScaleForDevice();

        // Recalculate on screen resize
        _rootElement.RegisterCallback<GeometryChangedEvent>(evt => AdjustScaleForDevice());
    }

    private void AdjustScaleForDevice()
    {
        var scale = CalculateScale();
        _rootElement.ProvideContext(new ScaleContext(scale));
        Debug.Log($"Screen scale set to: {scale}");
    }

    private string CalculateScale()
    {
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var minDimension = Mathf.Min(screenWidth, screenHeight);

        return minDimension switch
        {
            < 600 => "small",       // Phone
            < 1200 => "medium",     // Tablet/small monitor
            _ => "large"            // Desktop/large display
        };
    }
}
```

## Performance Considerations

1. **Theme switching** - Minimal performance impact, applied through USS selectors
2. **Scale changes** - Re-layout required, use sparingly
3. **Custom colors** - Use variables instead of hardcoding for better performance
4. **Font loading** - Pre-cache custom fonts in font assets
5. **Icon density** - Use @2x icons only on high-DPI devices

## Troubleshooting

### Theme not applying
- Verify `.appui--` prefix in class name
- Check PanelSettings has correct theme file assigned
- Ensure Panel component is root element

### Colors look wrong in dark/light mode
- Verify variables are assigned in both light and dark theme files
- Check contrast ratios for accessibility
- Test in Editor and Play mode

### Icons not showing
- Verify USS class follows `.appui-icon--{name}--{variant}` pattern
- Check icon file path is correct
- For high-DPI, use `resource()` with @2x files in Resources folder

### Scale not applying
- Ensure scale variant exists in selected theme
- Check scale values: "small", "medium", or "large"
- Verify PanelSettings references the scale-specific theme
