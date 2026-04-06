---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.8] - 2026-04-06

### Added

- Added `sizeModifier` callback property to `ResizeHandle` to allow custom size adjustments during resize.
- Added `UxmlTraits` support for `ResizeHandle` component.
- Added `SetPosition` and `SetSize` methods to `Popover` for programmatic positioning and sizing.
- Added `movable` property to `Popover` UI elements.
- Added `movable` property to `DialogTrigger` to support movable popovers via UXML.

### Changed

- `AnchorPopup.RefreshPosition` is now `public` instead of `protected`.
- `ColorField` popover is now movable by default.
- Default `resizeDirection` for `DialogTrigger` changed from `Vertical` to `Free`.

### Fixed

- Added missing `[CreateProperty]` attribute on `clickable` property across all pressable components to fix runtime data binding for `clickable.command` path (UUM-138492)

