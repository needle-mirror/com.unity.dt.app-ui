---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [1.0.0-pre.12] - 2024-01-03

### Changed

- Color related components, such as `ColorSlider` or `ColorSwatch`, support now a value of type `UnityEngine.Gradient` instead of `UnityEngine.Color` for more flexibility.
- Removed the `disabled` boolean property on App UI components from the public C# API. The `disabled` attribute in UXML has been replaced by `enabled` UXML attribute. This change has been made in order to be more consistent with Unity 2023.3, where `enabled` UXML attribute is provided by UI Toolkit on any VisualElement.
- Replaced `Nullable<T>` properties in components by a custom serializable implementation called `Optional<T>`
- Renamed `Panel.dir` property with `Panel.layoutDirection`.
- Replaced the `Divider.vertical` property by `Divider.direction` enum property.
- Removed the `ApplicationContext` class and `VisualElementExtensions.GetContext()` method, replaced by the ne `ProvideContext` and `RegisterContextChangedCallback` API.
- Replaced `ActionGroup.vertical` property by `ActionGroup.direction` enum property.

### Added

- Added support of new UI Toolkit Runtime Bindings feature through bindable properties in each App UI components. More than 420 properties can now be bound (2023.2+).
- Added support to new UI Toolkit Uxml Serialization using source code generators.
- Added `BaseVisualElement` and `BaseTextElement` classes which are used as base class for mostly every App UI component
- Added numerous custom PropertyDrawers for a better experience in UIBuilder (2023.3+)
- Added the support of `Fixed` gradient blend mode in ColorSwatch shader.
- Added `.appui-row` USS classes which support current layout direction context.

### Removed

- Starting Unity 2023.3, App UI will not provide any `UxmlFactory` or `UxmlTraits` implementation, as they become obsolete and replaced by the new UITK Uxml serialization system.

### Fixed

- Fixed Sliders handles to not exceed the range of track element.
- Fixed color blending in the ColorSwatch custom shader.
- Fixed `AppBar` story in Storybook samples.
- Fixed some styling issues on different components.
- Cleaned up some warning messages to not get anything written in the console during package installation.
- Fixed small errors in UI Kit sample.
- Fixed a refresh bug in the App UI Storybook window.

