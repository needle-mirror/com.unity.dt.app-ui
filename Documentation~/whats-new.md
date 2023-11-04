---
uid: whats-new
---

# What's New in **0.6.5**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added

- Added missing XML documentation parts for the public API of the package.
- Added the `primaryManipulator` property to the `Canvas` component to get the primary pointer manipulator used by the canvas when no modifier is pressed.
- Added the `autoResize` property to the `TextArea` component to grow the text area when the text overflows.
- Added the handling of double-click in the `TextArea` resizing handle to re-enable the auto-resize feature.
- Added the `outsideScrollEnabled` property to the `AnchorPopup` components to enable or disable the outside scroll handling. The default value is `false`.
- Added a smoother animation for the `AnchorPopup` components when the popup is about to be shown.

### Fixed

- Fixed the Canvas background to support light theme.
- Fixed the styling of DropdownItem checkmark.
- Fixed the styling of MenuItem checkmark.
- Fixed the styling of Slider components.
- Fixed the styling of TextArea component.
- Fixed the styling of TouchSlider components.
- Fixed the previous value sent in `ChangeEvent` of `NumericalField`, `VectorField` and `Picker` (Dropdown) components.
- Improved the synchronization of the `AnchorPopup` components to refresh their position in the layout faster.

### Changed 

- Changed the USS selector for component-level aliases to use directly `:root` selector instead of `<component_main_uss_class>` selector.