---
uid: whats-new
---

# What's New in **0.5.5**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added 

- [Backport] Added `closeOnSelection` property to the `MenuTrigger` component.
- [Backport] Added `closeOnSelection` property to the `ActionGroup` component to close the popover menu (if any) when an item is selected.
- [Backport] Added `primaryButton`, `secondaryButton` and `cancelButton` public access to the `AlertDialog` component.
- [Backport] Added the autofocus of the first available action in the `AlertDialog` component.

### Fixed

- [Backport] Fixed `closeOnSelection` property in `MenuBuilder` component.
- [Backport] Fixed `TextInput` styling for Unity 2023.1+
- [Backport] Fixed `InvalidCastException` at startup of santandlone builds (in both Mono and IL2CPP)
- [Backport] Fixed `NullReferenceException` when dismissing a Popup from a destroyed UI-Toolkit panel.
- [Backport] Fixed context click handling in `GridView` component.