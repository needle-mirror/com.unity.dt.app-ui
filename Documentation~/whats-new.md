---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.1.0-pre.1] - 2025-07-10

### Added

- Added input field text selection context menu to cut/copy/paste the currently selected text (starting Unity 2023.1).
- Added EyeDropper system (runtime-only) to pick a color in the last game frame.
- Added `goToStrategy` property in `SwipeView` component to choose how `GoTo` method should behave regarding animation direction.

### Fixed

- fix: Add missing InsideTopLeft case in ComputePosition switch statement
- Fixed App UI Settings registration in EditorBuildSettings.
- fix: Add tip/arrow placement logic for Inside\* popover placements
- Fixed event propagation bug in `Pressable` which sent the wrong pointer position value to ancestors when `keepEventPropagation` was `true`.

