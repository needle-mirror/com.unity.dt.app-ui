---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.5] - 2024-06-18

### Changed

- Complete rewrite of the SplitView component. The SplitView is no more a derived from TwoPaneSplitView from UI-Toolkit, but a full custom component that supports any number of panes.

### Fixed

- Fixed reset of Dropdown value when changing its source items.
- Fixed a visual bug where the checkmark symbol didn't appear on DropdownItem or MenuItem that have a `selected` state.

### Added

- Added `indicatorPosition` property in `AccordionItem` component, in order to swap the indicator position either at start or end of the heading row.
- Added `check` regular icon as a required icon in App UI themes.

