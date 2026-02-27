---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.6] - 2026-02-27

### Added

- Add `destroyItem` callback to BaseGridView and fix doc comments
- Added HelpBox UI element
- Added `disableAnimation` property on Popup elements in order to force disable display animation for a given Popup UI Element.
- Added SortingOrder proeprty in Popups. This enable the possibility to insert popups in behind others exisiting ones.
- UxmlCloneTreeGenerator now collects UxmlElementNameAttribute bindings from inherited base classes, respecting C# shadowing rules.
- Added `check-circle` and `x-circle` icons as part of the minimal icons set

### Fixed

- Fixed UpHandler calls in Draggable manipulator.
- Fixed exception thrown on Tabs element when refreshing the indicator with an out of range value.
- Fixed sending ChangeEvent in numerical fields as soon as they are attached to a panel if their value has been changed already even without notification.

