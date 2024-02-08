---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [1.0.0-pre.15] - 2024-02-09

### Removed

- Removed the `margin` property from the Tray component.
- Removed the `size` property of the `Tray` component. The Tray component will fit its content in the screen automatically and doen't need anymore any specific size to be set.
- Removed the `expandable` property from the Tray component. A Tray component is should not be expandable by definition. If you are willing to have some scrollable content inside a Tray component, a new component called ScrollableTray will be avilable in future release of App UI.

### Fixed

- Fixed styling on BottomNavBar items
- Fixed refresh of the `ActionGroup` component
- Fixed Tooltip maximum size

### Added

- Added monitoring of AccordionItem content size to make AccordionItem fit its content when it is already open.

### Changed

- Use the ConditionalWeakTable for new releases of Unity where a fix from IL2CPP has landed.

