---
uid: whats-new
---

# What's New in **0.5.2**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added

- Added `acceptDrag` property to the `GridView` component to enable or disable the drag and drop feature.
- Added `menu` icon.
- Added `AddDivider` and `AddSection` methods to the `MenuBuilder` component.

### Fixed

- Fixed handling `acceptDrag` property in `Dragger` manipulator.
- Fixed MenuItem opening sub menus when the item is disabled.
- Fixed mipmap limit for Icons when a global limit is set in the Quality settings of the project.
- Fixed the capture of pointer during PointerDown event in `Dragger` manipulator. Now the pointer is captured only if the `Dragger` manipulator is active (i.e. the threshold has been reached).