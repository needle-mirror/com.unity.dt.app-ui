---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.2.0-pre.3] - 2025-12-08

### Added

- A reference of the latest opened StyleSheet in IconBrowser tool is now stored per project. The next time you will open the tool, it will reload the latest StyleSheet.
- Added support of CoreCLR for the migration from Mono scripting backend for Unity 6.5+

### Fixed

- Fixed the AppUI settings assets search and auto-assignation before building a project.
- Fixed columns size and added more info in Storybook window.
- Fixed `VisualElementExtensions.IsOnScreen` method which gave wrong result in world-space panels.
- Fixed code path activation for native plugins depending on target platform

