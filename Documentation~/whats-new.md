---
uid: whats-new
---

# What's New in **0.6.2**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added 

- Added `closeOnSelection` property to the `MenuTrigger` component.
- Added `closeOnSelection` property to the `ActionGroup` component to close the popover menu (if any) when an item is selected.
- Added `primaryButton`, `secondaryButton` and `cancelButton` public access to the `AlertDialog` component.
- Added the autofocus of the first available action in the `AlertDialog` component.

### Fixed

- Fixed `closeOnSelection` property in `MenuBuilder` component.
- Fixed `TextInput` styling for Unity 2023.1+
- Fixed `InvalidCastException` at startup of santandlone builds (in both Mono and IL2CPP)
- Fixed `NullReferenceException` when dismissing a Popup from a destroyed UI-Toolkit panel.
- Fixed sub-menu indicator icon in `MenuItem` component.
- Fixed context click handling in `GridView` component.