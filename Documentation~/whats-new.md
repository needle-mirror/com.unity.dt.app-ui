---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.12] - 2024-11-18

### Added

- Navigation: Added `NavigationRail` component which can be used inside NavigationScreen views.
- Added `autoShrink` property to the `TextArea` component to automatically shrink the component when the text fits in a smaller area. You need to set `autoResize` property to `true` to use the shrinking feature.
- Added `ResizeHandle` component. This special component can be added to your layout to resize a given `target` when dragging it. This component is currently used in the new resizable `Popover` element.
- Added `resizable` and `resizableDirection` properties in `Popover` class. You are now able to resize a `Popover` using the bottom-right corner of the popover pane. A resizable Popover will not be repositionned.
- Added Clipboard handling support for **UTF8 Text** and **PNG** format on _iOS_, _Windows_, _macOS_, and _Linux_ platforms. Please refer to the **Native Integration** documentation page for more information.

### Changed

- App UI shaders are now optionally embedded in Player builds. You can change this setting in your main App UI settings instance. The default value is `true`.

### Fixed

- Fixed Dark Gray 1200 color value in Design Tokens for Editor-Dark theme.
- Fixed a bug where `Popover` elements were not correctly anchored in Unity versions older than 6.
- Fixed usage of AppCompat theme for AppUI GameActivity on Android platform.
- Fixed an exception thrown when trying to update UXML schemas in the Editor.
- Register trickledown events for popups on the first child of the visual tree root element instead of the root element itself to avoid leaks.

