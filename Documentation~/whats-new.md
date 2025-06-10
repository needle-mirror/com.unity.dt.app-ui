---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.20] - 2025-06-10

### Removed

- Removed `Canvas.scrollDirection` property in order to support correctly the scroll direction set in the Operating System's preferences.

### Fixed

- Fixed tooltip that weren't showing up anymore since last pre-release.
- Fixed `VisualElementExtensions.IsInvisible` method to check recursively in ancestors if any element is considered as _invisible_.

### Changed

- Use `resolvedStyle.translate` instead of `transform.position` to move elements such as the `Canvas` container.
- Replaced the dropdown with an ObjectField to pick an available App UI settings asset.

### Added

- Added a safety check in TooltipManipulator to ensure the anchor element of the scheduled tooltip is still valid (not _invisible_) when it is time to _animate in_ the tooltip.
- Added monitoring of key press events in the TooltipManipulator in order to dismiss any existing tooltip when the user interacts with the keyboard.

