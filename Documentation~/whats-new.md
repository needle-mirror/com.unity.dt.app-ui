---
uid: whats-new
---

# What's New in **0.5.0**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added

- Added `outsideClickStrategy` property in `AnchorPopup` component.
- Added `DropZone` component.
- Added `DropTarget` manipulator.
- Added a sample for `DropZone` and `DropTarget` components usage.
- Added `--border-style` custom USS property to support `dotted` and `dashed` border styles.
- Added `--border-speed` custom USS property to animate the border style.
- Added `Picker` component.
- Added `closeOnSelection` property for `Picker` and `Dropdown` components to close the picker when an item is selected.

### Fixed

- Fixed the `Pressable` manipulator to only handle the Left Mouse Button by default.
- Fixed current tooltip element check in `TooltipManipulator`.
- Fixed incremental value when interacting with keyboard in `ColorSlider` component.
- Fixed clearing selection in `GridView` when a user clicks outside of the grid.
- Fixed `itemsChosen` event in `GridView` to be fired only when the selection is not empty.
- Fixed Navigation keys handling in the `GridView` to clamp the selection to the grid items.

### Changed

- The `Pressable` manipulator nows inherits from `PointerManipulator` instead of `Manipulator`.
- Changed the `GridView.GetIndexByPosition` method to use a world-space position instead of a local-space position and renamed it to `GetIndexByWorldPosition`.
- TouchSlider component will now loose focus when a slide interaction has ended.
- When calling `GridView.Reset()` method, the selection won't be restored if no custom `GridView.getItemId` function has been provided. 
- When using `--box-shadow-type: 1` (inset box-shadow), the `--box-shadow-spread` value was interpreted with the same direction as outset box-shadow. This has been fixed so you can use a positive spread value to go inside the element and a negative spread value to go outside the element.
- The `Dropdown` component inherits from `Picker` component. Users will be able to create custom dropdown-like components by inheriting from `Picker` component.
- The `Dropdown` component now has a selection mode property to choose between single and multiple selection modes.
- The `Dropdown` component now uses `DropdownItem` component instead of `MenuItem` component.
- Removed `default-value` UXML property from `Dropdown` component. The preferred way to set the default value is by code.
