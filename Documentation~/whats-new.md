---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [1.0.0-pre.14] - 2024-01-30

### Fixed

- Fixed ColorSwatch refresh when the Gradient reference value didn't change but Gradient keys have changed
- Added support of Windows 11 for ARM
- Fixed ColorPicker Alpha channel slider refresh when the picked color has changed
- Fixed calls for ContextChanged callbacks registered by the context provider itself

### Added

- Added new IInputElement interface in the public API
- Added the possibility to setup navigation visual components from your NavigationScreen implementation directly. You are still able to create a global setup using your own implementation of the INavVisualController interface that you have set on your NavHost.
- Added the support of a specific drag direction and drag threshold in the Draggable manipulator. Specifying a direction will avoid to prevent scrolling in the opposite direction if this maniuplator is used inside a ScrollView for example.
- Added forceUseTooltipSystem property on the Panel component to force the use of App UI tooltips system regardless the state of UI-Toolkit default tooltip system. This can be useful in a context where UITK tooltips can't be displayed in the Editor.
- Added mention about full integration of UI Toolkit with the New Input System starting 2023.2 in the documentation

### Changed

- All user input related UI controls now inherit from the new IInputElement interface. This has no impact on the current implementation (some addition in certain components).
- Use ConditionalWeakTable whenever possible (Editor and no IL2CPP builds)

