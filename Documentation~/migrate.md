---
uid: migrate
---

# Migration Guide

## Application Context

The `ApplicationContext` class doesn't exist anymore. Here is how to migrate your code:

### Before

```cs
// get the current theme
var theme = myElement.GetContext().theme;
// get the current scale
var scale = myElement.GetContext().scale;
```

### After

```cs
// get the current theme
var themeOrNull = myElement.GetContext<ThemeContext>()?.theme;
// get the current scale
var scaleOrNull = myElement.GetContext<ScaleContext>()?.scale;
```

## ContextProvider

While the `ContextProvider` class still exist, now any `VisualElement` has the ability to provide a context.

### Before

```cs
// create a context provider
var contextProvider = new ContextProvider();
// override the theme
contextProvider.theme = myTheme;
// override the scale
contextProvider.scale = myScale;
```

### After

```cs
// override the theme
myElement.ProvideContext(new ThemeContext(myTheme));
// override the scale
myElement.ProvideContext(new ScaleContext(myScale));
// provide a null context to reset the context and use ancestor context
myElement.ProvideContext<ThemeContext>(null);
myElement.ProvideContext<ScaleContext>(null);
```

## Checkbox and Toggle sizes

The `Checkbox` and `Toggle` classes don't have a `size` property anymore. You need to remove any occurence of this property in your code.

## Nullable properties on visual elements

App UI now uses `Optional<T>` instead of `Nullable<T>` (or `T?`) for properties on visual elements that are defined as *overrides*. 

Since this custom data structure doesn't support null coallescing operator (`??`), you need to replace any occurence of this operator in your code.

### Before

```cs
// get the minimum value or 0.0f if the property is not set
var minimum = myFloatField.lowValue ?? 0.0f;
// unset the minimum value
myFloatField.lowValue = null;
```

### After

```cs
// get the minimum value or 0.0f if the property is not set
var minimum = myFloatField.lowValue.IsSet ? myFloatField.lowValue.Value : 0.0f;
// unset the minimum value  
myFloatField.lowValue = Optional<float>.none;
```

## Disabled state on visual elements

The `disabled` property on App UI components has been made private. You should not use this property in your code, since you can directly use the `SetEnabled` method and the `enabledSelf` property on any visual element.

The `disabled` attribute in UXML has been replaced by `enabled` UXML attribute. This change has been made in order to be more consistent with Unity 2023.3, where `enabled` UXML attribute is provided by UI Toolkit on any [VisualElement](xref:UnityEngine.UIElements.VisualElement).

## ActionGroup and Divider orientation

The `ActionGroup` and `Divider` classes don't have a `vertical` boolean property anymore. It has been replaced by an `direction` enum property.

### Before

```cs
// create a vertical action group
var actionGroup = new ActionGroup() { vertical = true };
// create a vertical divider
var divider = new Divider() { vertical = true };
```

```xml
<!-- create a vertical action group -->
<appui:ActionGroup vertical="true" />
<!-- create a vertical divider -->
<appui:Divider vertical="true" />
```

### After

```cs
// create a vertical action group
var actionGroup = new ActionGroup() { direction = Direction.Vertical };
// create a vertical divider
var divider = new Divider() { direction = Direction.Vertical };
```

```xml
<!-- create a vertical action group -->
<appui:ActionGroup direction="Vertical" />
<!-- create a vertical divider -->
<appui:Divider direction="Vertical" />
```

## Color related components

The `ColorEntry` class doesn't exist anymore in order to be more consistent with Unity's API.
Here is how to migrate your code:

The `ColorSlider.colorRange` property now uses a [Gradient](xref:UnityEngine.Gradient) value type instead of a list of [ColorEntry](xref:Unity.AppUI.Core.ColorEntry) objects.

### Before

```cs
// get the color range
List<ColorEntry> range = myColorSlider.colorRange;
// set the color range
myColorSlider.colorRange = new List<ColorEntry> {
    new ColorEntry(Color.red, 0.0f),
    new ColorEntry(Color.green, 0.5f),
    new ColorEntry(Color.blue, 1.0f)
};
```

### After

```cs
// get the color range
Gradient range = myColorSlider.colorRange;
// set the color range
myColorSlider.colorRange = new Gradient {
    colorKeys = new GradientColorKey[] {
        new GradientColorKey(Color.red, 0.0f),
        new GradientColorKey(Color.green, 0.5f),
        new GradientColorKey(Color.blue, 1.0f)
    }
};
```

The `ColorSwatch` class uses the [Gradient](xref:UnityEngine.Gradient) value type instead of a list of [ColorEntry](xref:Unity.AppUI.Core.ColorEntry) objects.

### Before

```cs
// get the color
List<ColorEntry> colors = myColorSwatch.value;
// set the color
myColorSwatch.value = new List<ColorEntry> {
    new ColorEntry(Color.red, 0.0f),
    new ColorEntry(Color.green, 0.5f),
    new ColorEntry(Color.blue, 1.0f)
};
```

### After

```cs
// get the color
Gradient colors = myColorSwatch.value;
// set the color
myColorSwatch.value = new Gradient {
    colorKeys = new GradientColorKey[] {
        new GradientColorKey(Color.red, 0.0f),
        new GradientColorKey(Color.green, 0.5f),
        new GradientColorKey(Color.blue, 1.0f)
    }
};
```