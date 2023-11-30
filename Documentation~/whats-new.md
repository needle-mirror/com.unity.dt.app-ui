---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [1.0.0-pre.8] - 2023-11-30

### Fixed

- Fixed Unity crashes when polling TabItem elements inside Tabs component.
- Fixed mouse capture during Pointer down event in Pressable manipulator for Unity versions older than 2023.1.
- Fixed tab key handling to switch focus in `TextArea` component.
- Fixed UI Kit Sample with Progress components' color overrides.
- Fixed `border-radius` usage in `ExVisualElement` component.
- Fixed typo in App UI Elevation's USS selector.
- Fixed box-shadows border-radius calculation
- Fixed `isPrimaryActionDisabled` and `isSecondaryActionDisabled` property setters

### Added

- Added new methods to push sub-menus in `MenuBuilder` class.
- Added `submit-on-enter` property in `TextArea` component.
- Added `submit-modifiers` property in `TextArea` component.
- Added `submitted` event property in `TextArea` component.
- Added `subMenuOpened` event property in `MenuItem` UI component.
- Added `accent` property to the `FloatingActionButton` component.

### Changed

- Runtime Tooltips won't be displayed if the picked element has `.is-open` USS class currently applied.

