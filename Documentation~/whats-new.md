---
uid: whats-new
---

# What's New in **1.0.0-pre.3**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Fixed

- Fixed warning message about Z-axis scale in the `AnchorPopup` component.

### Changed

- The `Pressable` manipulator now uses the `keepPropagation` property also in its `PointerUpEvent` callback to avoid to propagate the event to the parent element.