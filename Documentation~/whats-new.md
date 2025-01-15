---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.16] - 2025-01-15

### Removed

- Removed `Unity.AppUI.Core.AppUI.DismissAnyPopups` method. Popups are now registered per `Panel` element.

### Changed

- Refactored SliderBase and BaseSlider for performance improvements and better features. The RangeSliderBase class has been removed and RangeSliderFloat/RangeSliderInt are now derived from SliderBase directly.
- The `Platform`, `AppUIInput` and `GestureRecognizer` classes now use pre-allocated buffers to deal with Touch events in a single frame, and expects a `ReadOnlySpan<AppUITouch>` type to work with instead of `AppUITouch[]`.
- Changed `ticks` related properties in Slider components with a new `marks` property. To configure marks you can you the new `step` property or directly set your `customMarks`
- Updated App UI Native plugins for MacOS and iOS.

### Added

- Added `scale` property on slider components to support custom non-linear scaling of displayed values in the UI.
- Added the support of inverted track progress in slider components.
- Added Redux Debugging tool.
- Added `formatFunction` along with `formatString` and grouped them into a new `IFormattable<TValueType>` interface. The `formatFunction` will give you the possibility to customize entirely the string formatted value of a given `TValueType`.
- Added a `RestrictedValuePolicy` on slider components where you can choose if the slider can take a value related to the `step` or `customMarks` with modifier keys or not.
- `VisualElementExtensions.HasAncestorsOfType{T}` method to verify if an element as any ancestor of a specific type.
- Added the serialzation of last selected indices in Storybook window to save and restore selection.
- Added `orientation` property on slider components, to support vertical sliders.
- Added `VisualElementExtensions.GetLastAncestorOfType{T}` method to retrieve the last ancestor of a specific type in the tree.
- Added `VisualElementExtensions.EnableDynamicTransform` method to set/unset the `UsageHints.DynamicTransform` flag in your element's usageHints.
- Added `RectExtensions.Approximately` method to verify if two Rect object are approximately the same.
- Popup elements such as `Popover`, `Modal` or `MenuBuilder` now get their view's `userData` filled with the `Popup` instance itself. This way you can retrieve information about the `Popup` instance that created this popup element from within the visual tree.
- Added USS custom properties to choose the font style of AccordionItem header and TabItem text.

### Fixed

- Fixed calling `shown` event callbacks when a `Modal` is displayed.
- Fixed Tabs emphasized color in Editor-light theme.
- Use `RenderTexture.GetTemporary` instead of `new RenderTexture` to optimize RT allocations, especially on tile-based renderers such as mobile platforms.
- Fixed an exception thrown when dismiss any popup of a panel when this panel becomes out of focus.

