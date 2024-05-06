---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [2.0.0-pre.2] - 2024-05-07

### Added

- Added `PinchGestureRecognizer` implementation for the new Gesture Recognizer System.
- Added an experimental method `Platform.GetSystemColor` to fetch color values defined by the Operating System for specific UI element types. This can be useful if you want to precisely follow the color palette of a high-contrast theme directed by the OS.
- Added "Icon Browser", a new Editor tool that enables users to generate UI-Toolkit stylesheets with a specific list of icons.
- Added a new experimental Gesture Recognizer System.
- Added the ability to subscribe and check if the current operating system is in Reduce-Motion Accessibility Mode (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current Text Scale Factor of the currently used window (Unity Player window or the Game view window in the Editor) (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current operating system is in High-Contrast Mode (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current operating system is in LeftToRight or RightToLeft layout direction (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current Scale Factor of the currently used window (Unity Player window or the Game view window in the Editor) (Windows/Mac/Android/iOS).
- Added the ability to subscribe and check if the current operating system is in Dark Mode (Windows/Mac/Android/iOS).

### Changed

- Refactored every native plugin provided by the package.
- Changed the Trackpad sample project to work properly with the new events coming from the new Gesture Recognizer System.

### Fixed

- Fixed meta files for native plugins on Windows platform.
- Fixed an early return in the PreProcessBuild callback of App UI when no persistent AppUISettings have been found.

