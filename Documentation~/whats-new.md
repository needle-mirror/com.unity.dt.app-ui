---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.13] - 2024-11-28

### Added

- Added `RegisterUpdateCallback` to register a callback on VisualElement that needs to be notified when App UI's main loop has done a new iteration.
- Added `VisualElementExtensions.IsOnScreen` method to determine if a `VisualElement` is on-screen or off-screen.
- Added `leadingContainer` and `leadingContentTemplate` properties to the `AccordionItem` element.

### Changed

- Rewrite of App UI shaders in pure `HLSL` instead of legacy `CG` language.
- Refactored the `FindResponderInChain` native method in MacOS platform to avoid undefined behaviours.
- `CircularProgress` and `LinearProgress` elements with `Indeterminate` variant won't be marked as `DirtyRepaint` if they are off-screen anymore.

### Fixed

- Fixed random segmentation fault on MacOS platform which appeared after domain reloads.
- Prevent GC to collect Platform configuration to not break communication with native plugins (MacOS and iOS).

