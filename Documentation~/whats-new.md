---
uid: whats-new
---

# What's New

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the **Changelog** page.

The main updates in this release include:

## [1.0.0-pre.9] - 2023-12-05

### Fixed

- Fixed Selection handling in ActionGroup.
- Fixed MacOS Native plugin to support Intel 64bits architecture with IL2CPP

### Changed

- In Numerical fields and sliders, the string formatting for percentage values follows now the C# standard. The formatted string of the value `1` using `0P` or `0%` will give you `100%`. If you want the user to be able to type `100` in order to get a `100%` as a formatted string in a numerical field, we suggest to use `0\%` string format (be sure that the `highValue` of your field is set to `100` too).

### Added

- Added `isOpen` property setter to be able to set the open state of the `Drawer` component without animation.

