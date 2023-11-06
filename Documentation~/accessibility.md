---
uid: accessibility
---

# Accessibility

Accessibility support in game engines can be more complex than other platforms, since there are no universal standards for
how to implement accessibility features. However, App UI provides a few features that can help make your UI more accessible to users with disabilities.

## Focus Outline

The focus outline is a visual indicator that shows which UI element is currently focused.
This indicator is only visible when the UI element has been focused via a keyboard navigation.

<p align="center">
  <img src="images/focus-outline.png" alt="Focus Outline">
</p>

## Keyboard navigation

One key feature for accessibility is keyboard navigation. This feature allows users to navigate through UI elements using the tab and shift+tab keys.
App UI supports keyboard navigation through a defined Focus Ring. When an element is focused, it is outlined with a focus indicator.
This feature is important for users who rely on a keyboard to navigate UIs.

<p align="center">
  <img src="images/keyboard-navigation.gif" alt="Keyboard navigation">
</p>

To enable keyboard navigation for App UI components, you can use the `focusable` property.
This property determines whether an element can receive focus from the keyboard.
By default, most App UI components are not focusable, so you will need to set this property if you want to enable keyboard navigation.

```csharp
using Unity.AppUI.UI;

var myButton = new Button();

// Legacy UITK focusable mode, to be navigated with tab key
myButton.focusable = true; 

// New focusable mode for more granular control
myButton.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn, OnFocusOut));
```
