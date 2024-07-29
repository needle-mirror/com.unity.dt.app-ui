---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.7] - 2024-07-30

### Changed

- Changed DialogTrigger.keyboardDismissDisabled to DialogTrigger.keyboardDismissEnabled for consistency.
- Renamed Popup.parentView to Popup.containerView for more clarity.

### Added

- Added Modal.outsideClickDismissEnabled and Modal.outsideClickStrategy properties to support dismissing Modals by clicking outside of them.
- Added Popup<T>.SetContainerView method to set a custom container which will be the parent of the popup's view.
- Made `AnchorPopup.GetMovableElement` method public for easier access and increase customization possibilities.

### Fixed

- Fixed "Shape" icon.
- Fixed border color variable for AccordionItem.
- Fixed CultureInfo used during source code generation.

### Removed

- Removed intrusive Debug.Log calls from Platform class on Windows platform.
- Removed warning message when using Single selection type in an overflown ActionGroup.

