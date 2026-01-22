---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.4] - 2026-01-22

### Added

- Added support of DevicePixelRatio in WebGL native plugin to determine the current ScaleFactor applied to the canvas.
- Added Uxml cloneTree and element binding by name source generators. See the documentation for more information.
- Added runtime data binding support to `BaseGridView` component.
- Added the support of custom ColorPicker inside the ColorField element via customPicker property.
- Added support of system theme in WebGL. Use `Platform.darkMode` or register your callback on the `Platform.darkModeChanged` event to make changes in your application based on the current system theme.
- Added support of drag-to-increment-value feature in `InputLabel` UI element.
- Added support of system clipboard in WebGL platform for text fields.

### Fixed

- Composite Numerical fields (such as Vector fields, Bounds fields, etc) previous value returned in the ChangeEvent is now the correct one.
- Fixed applying anchored popup positions only if the computed values are different than the existing ones to avoid floating point precision issues in the layout.
- App UI Icons StyleSheet Asset now has a StyleSheet icon in the Project window when creating the StyleSheet Asset.

