---
uid: native-integration
---

# Native Integration

App UI supports communication with the operating system through [native plugins](xref:NativePlugins). 
For now plugins have been developed for the following platforms:
- [Android](xref:AndroidNativePlugins)
- [iOS](xref:PluginsForIOS)
- [Windows](xref:PluginsForDesktop)
- [macOS](xref:PluginsForDesktop)

## Screen Informations

Thanks to the [Screen](xref:UnityEngine.Screen) class from Unity, you can get information about the screen size and orientation.
However we found that the relationship between the [dpi](xref:UnityEngine.Screen.dpi) and 
the [UI Toolkit PanelSettings](xref:UnityEngine.UIElements.PanelSettings) is different from a platform to another.

We provide a [mainScreenScale](xref:Unity.AppUI.Core.Platform.mainScreenScale) property which gives an accurate scale factor
that takes into account the dpi of the screen but also user-defined display scaling from the operating system.

When you enable the [Auto Scale Mode](xref:setup#app-ui-settings-file) on the UI Toolkit PanelSettings,
the UI elements will be scaled according to the [referenceDpi](xref:Unity.AppUI.Core.Platform.referenceDpi) value which is calculated
using the [mainScreenScale](xref:Unity.AppUI.Core.Platform.mainScreenScale) property.

## System Theme

App UI provides a [systemTheme](xref:Unity.AppUI.Core.Platform.systemTheme) property that allows you to get the current system theme.
We also provide a [systemThemeChanged](xref:Unity.AppUI.Core.Platform.systemThemeChanged) event that you can subscribe to in order to be notified
when the system theme changes.

It is common to see apps that let the user choose between a light and a dark theme, or stick to the system theme.
You can offer the same feature by using the [systemTheme](xref:Unity.AppUI.Core.Platform.systemTheme) property.

Here is an example using the [systemTheme](xref:Unity.AppUI.Core.Platform.systemTheme) property
inside a [RadioGroup](xref:Unity.AppUI.UI.RadioGroup):

```csharp
// Get the closest provider, assuming you call this method from a UI Toolkit element
// contained in a hierarchy that has a ThemeContext provider
var provider = this.GetContextProvider<ThemeContext>();

// Create the callback that will be called when the system theme changes
void OnSystemThemeChanged(string systemTheme) => provider.theme = systemTheme;

// Setup the RadioGroup
var themeSwitcher = new RadioGroup();
themeSwitcher.Add(new Radio("System"));
themeSwitcher.Add(new Radio("Dark"));
themeSwitcher.Add(new Radio("Light"));
void SetTheme()
{
    Platform.systemThemeChanged -= OnSystemThemeChanged;
    switch (themeSwitcher.value)
    {
        case 0:
            provider.theme = Platform.systemTheme;
            Platform.systemThemeChanged += OnSystemThemeChanged;
            break;
        case 1:
            provider.theme = "dark";
            break;
        case 2:
            provider.theme = "light";
            break;
    }
    PlayerPrefs.SetInt("theme", themeSwitcher.value);
}
themeSwitcher.RegisterValueChangedCallback(_ => SetTheme());

// Load and set the theme value
themeSwitcher.SetValueWithoutNotify(PlayerPrefs.GetInt("theme", 1));
SetTheme();
```

To know more about theming, see [the dedicated documentation page](xref:theming).

## Haptic Feedback

App UI provides a [RunHapticFeedback](xref:Unity.AppUI.Core.Platform)
method that allows you to trigger haptic feedback on supported platforms.
For now, the only supported platforms are iOS and Android.

> [!NOTE]
> In the current state of App UI components, none of them use haptic feedback directly.
> However, you can use this method to trigger haptic feedback when you need it.