---
uid: whats-new
---

# What's New in **0.5.1**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added

- Added `selectedIndex` property to the `Picker` component to get or set the selected index for a single selection mode.
- Added a new `Enumerable` extension method called `GetFirst` to get the first item of an enumerable collection or a default value if the collection is empty.
- Added `isPrimaryActionDisabled` and `isSecondaryActionDisabled` properties to the `AlertDialog` component.
- Added styling for `AlertDialog` component semantic variants.

### Fixed

- Fixed the `Picker` component to avoid to select multiple items if the selection mode is set to `Single`.
- Fixed `Menu.CloseSubMenus` method to close sub-menus opened by `MenuItem` components contained inside `MenuSection` components.
- Fixed double click handling in `GridView` component.