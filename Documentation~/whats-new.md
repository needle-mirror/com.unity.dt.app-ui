---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

### Added

- Added new *Placement* possibilities in for `AnchorPopup` components. 
  Now you will be able to anchor a popup inside the anchor element instead of outside of it.
- Added `zoomChanged` and `scrollOffsetChanged` events to the `Canvas` component.
- Added `Commanding` documentation page.

### Fixed

- Fixed setting the opacity of `Popover` component at the right frame to avoid to see the content of the popover during the animation.

### Changed

- The `Canvas` component now listens to `PointerDownEvent` in `TrickleDown` phase to avoid to miss the event when the pointer is over a child element.